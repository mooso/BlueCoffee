using DarkNotes;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SharkFrontEnd
{
	public class HiveDataSource
	{
		private static Lazy<HiveDataSource> _instance = new Lazy<HiveDataSource>(Initialize);
		private readonly dynamic _java;
		private readonly string _jdbcUri;
		private const string JavaPlatformDirectory = "JavaPlatform";
		private const string SparkDirectory = "Spark";

		private HiveDataSource(DarkJava java, string serverIP)
		{
			_java = java;
			_jdbcUri = String.Format("jdbc:hive2://{0}:8082", serverIP);
		}

		private static HiveDataSource Initialize()
		{
			DownloadResources();
			var javaRoot = ExtractJdk();
			var sparkRoot = ExtractSparkJars();
			var sharkNode = RoleEnvironment.IsEmulated ? "localhost" :
				RoleEnvironment.Roles["Shark"].Instances[0].InstanceEndpoints.First().Value.IPEndpoint.Address.ToString();
			return HiveDataSource.Initialize(javaRoot: javaRoot, sparkRoot: sparkRoot, serverIP: sharkNode);
		}

		private static HiveDataSource Initialize(string javaRoot, string sparkRoot, string serverIP)
		{
			var allJars = String.Join(";", Directory.EnumerateFiles(sparkRoot));
			dynamic java = DarkJava.CreateVm(
				jvmDllPath: Path.Combine(javaRoot, "java", "jre", "bin", "server", "jvm.dll"),
				options: new[] { JavaOption.DefineProperty("java.class.path", allJars) });
			// Import some packages
			java.ImportPackage("java.lang");
			java.ImportPackage("java.sql");
			// Initialize the driver class
			var hiveThrowawayDriver = java.org.apache.hive.jdbc.HiveDriver.@new();
			// Initalize the singleton
			return new HiveDataSource(java, serverIP);
		}

		public static HiveDataSource Instance { get { return _instance.Value; } }

		public List<dynamic> ExecuteQuery(string query)
		{
			dynamic conn = _java.DriverManager.getConnection(_jdbcUri);
			dynamic statement = conn.createStatement();
			dynamic resultSet = statement.executeQuery(query);
			dynamic metadata = resultSet.getMetaData();
			var returnList = new List<dynamic>();
			int columnCount = (int)metadata.getColumnCount();
			string[] columnNames = new string[columnCount];
			for (int i = 0; i < columnCount; i++)
			{
				columnNames[i] = (string)metadata.getColumnName(i + 1);
			}
			while ((bool)resultSet.next())
			{
				var newRow = (IDictionary<string, object>)new ExpandoObject();
				for (int i = 0; i < columnCount; i++)
				{
					newRow[columnNames[i]] = (string)resultSet.getObject(i + 1).toString();
				}
				returnList.Add(newRow);
			}
			try
			{
				conn.close();
			}
			catch (Exception ex)
			{
				Trace.TraceWarning("Error while closing: " + ex);
			}
			return returnList;
		}

		private static string ExtractJdk()
		{
			var javaRoot = Path.Combine(InstallDirectory, "Java");
			ZipFile.ExtractToDirectory(Path.Combine(GetResourcesDirectory(JavaPlatformDirectory), "openjdk7.zip"), javaRoot);
			return javaRoot;
		}

		private static string ExtractSparkJars()
		{
			var sparkRoot = Path.Combine(InstallDirectory, "Spark");
			ZipFile.ExtractToDirectory(Path.Combine(GetResourcesDirectory(SparkDirectory), "Jars.zip"), sparkRoot);
			return sparkRoot;
		}

		private static string GetResourcesDirectory(string componentName)
		{
			return Path.Combine(RootResourcesDirectory, componentName);
		}

		private static IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { JavaPlatformDirectory, SparkDirectory };
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