using DarkNotes;
using Microsoft.Experimental.Azure.JavaPlatform;
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
			var installer = new JavaAzureInstaller();
			installer.DownloadResources(new[] { installer.JavaPlatformDirectory, "Storm" });
			installer.InstallJava();
			var stormRoot = Path.Combine(installer.InstallDirectory, "Storm");
			installer.ExtractResourceArchive("Storm", "Jars.zip", stormRoot);
			var drpcNode = RoleEnvironment.IsEmulated ? "localhost" :
				RoleEnvironment.Roles["Supervisor"].Instances[0].InstanceEndpoints.First().Value.IPEndpoint.Address.ToString();
			return Initialize(javaRoot: installer.JavaHome, stormRoot: stormRoot, drpcNode: drpcNode);
		}

		private static DrpcQuery Initialize(string javaRoot, string stormRoot, string drpcNode)
		{
			var allJars = String.Join(";", Directory.EnumerateFiles(stormRoot));
			dynamic java = DarkJava.CreateVm(
				jvmDllPath: Path.Combine(javaRoot, "jre", "bin", "server", "jvm.dll"),
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
	}
}