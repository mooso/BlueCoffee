using DarkNotes;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StormDrpcWebUI
{
	public sealed class DrpcQuery
	{
		private static Lazy<DrpcQuery> _instance = new Lazy<DrpcQuery>(Initialize);
		private string _localStormDir;
		private string _drpcHost;
		private readonly DarkJava _java;
		private readonly dynamic _client;
		private const string JavaPlatformDirectory = "JavaPlatform";
		private const string StormDirectory = "Storm";

		private DrpcQuery(DarkJava java, dynamic client)
		{
			_java = java;
			_client = client;
		}

		private static DrpcQuery Initialize()
		{
			DownloadResources();
			var javaRoot = ExtractJdk();
			var stormRoot = ExtractStormJars();
			var drpcNode = RoleEnvironment.IsEmulated ? "localhost" :
				RoleEnvironment.Roles["Supervisor"].Instances[0].InstanceEndpoints.First().Value.IPEndpoint.Address.ToString();
			return Initialize(javaRoot: javaRoot, stormRoot: stormRoot, drpcNode: drpcNode);
		}

		private static DrpcQuery Initialize(string javaRoot, string stormRoot, string drpcNode)
		{
			var allJars = String.Join(";", Directory.EnumerateFiles(stormRoot));
			dynamic java = DarkJava.CreateVm(
				jvmDllPath: Path.Combine(javaRoot, "java", "jre", "bin", "server", "jvm.dll"),
				options: new[] { JavaOption.DefineProperty("java.class.path", allJars) });
			// Import some packages
			java.ImportPackage("java.lang");
			// Initialize the client
			var client = java.backtype.storm.utils.DRPCClient.@new(drpcNode, 3772);
			return new DrpcQuery(java, client);
		}

		public static DrpcQuery Instance { get { return _instance.Value; } }

		public int GetWordCount(string words)
		{
			string json = _client.execute("words", words);
			var resultSet = (JArray)JsonConvert.DeserializeObject(json);
			var tuple = (JArray)(resultSet[0]);
			return (int)tuple[0];
		}

		private static string ExtractJdk()
		{
			var javaRoot = Path.Combine(InstallDirectory, "Java");
			ZipFile.ExtractToDirectory(Path.Combine(GetResourcesDirectory(JavaPlatformDirectory), "openjdk7.zip"), javaRoot);
			return javaRoot;
		}

		private static string ExtractStormJars()
		{
			var StormRoot = Path.Combine(InstallDirectory, "Storm");
			ZipFile.ExtractToDirectory(Path.Combine(GetResourcesDirectory(StormDirectory), "Jars.zip"), StormRoot);
			return StormRoot;
		}

		private static string GetResourcesDirectory(string componentName)
		{
			return Path.Combine(RootResourcesDirectory, componentName);
		}

		private static IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { JavaPlatformDirectory, StormDirectory };
			}
		}

		private static string InstallDirectory
		{
			get { return RoleEnvironment.GetLocalResource("InstallDirectory").RootPath; }
		}

		private static string RootResourcesDirectory
		{
			get { return Path.Combine(InstallDirectory, "Resources"); }
		}

		private static void DownloadResources()
		{
			var resourcesContainer = GetResourcesContainer();
			Parallel.ForEach(ResourceDirectoriesToDownload, directory =>
			{
				var cloudDirectory = resourcesContainer.GetDirectoryReference(directory);
				var localDirectory = Path.Combine(RootResourcesDirectory, directory);
				Directory.CreateDirectory(localDirectory);
				Parallel.ForEach(cloudDirectory.ListBlobs().OfType<CloudBlockBlob>(), blob =>
				{
					var blobSimpleName = blob.Name.Substring(blob.Name.LastIndexOf('/') + 1);
					blob.DownloadToFile(Path.Combine(localDirectory, blobSimpleName), FileMode.Create);
				});
			});
		}

		private static CloudBlobContainer GetResourcesContainer()
		{
			var connectionString = RoleEnvironment.GetConfigurationSettingValue(
				"BlueCoffee.Resources.Account.ConnectionString");
			var containerName = RoleEnvironment.GetConfigurationSettingValue(
				"BlueCoffee.Resources.Container.Name");
			var storageAccount = CloudStorageAccount.Parse(connectionString);
			return storageAccount.CreateCloudBlobClient().GetContainerReference(containerName);
		}
	}
}