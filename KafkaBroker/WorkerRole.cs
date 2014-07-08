using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.IO.Compression;
using JavaUtils;
using System.IO;

namespace KafkaBroker
{
	public class WorkerRole : RoleEntryPoint
	{
		private string _javaInstallHome;
		private string _jarsHome;
		private string _dataDirectory; // TODO: Put on Azure Files or somewhere permanent
		private string _javaHome;
		private string _configsDirectory;
		private string _logsDirectory;
		private string _kafkaServerPropertiesPath;
		private string _kafkaLog4jPropertiesPath;

		public override void Run()
		{
			try
			{
				var runner = new JavaRunner(_javaHome);
				const string className = "kafka.Kafka";
				var classPathEntries = JavaRunner.GetClassPathForJarsInDirectories(_jarsHome);
				runner.RunClass(className,
					_kafkaServerPropertiesPath,
					classPathEntries,
					defines: new Dictionary<string, string>
					{
						{ "log4j.configuration", "file:\"" + _kafkaLog4jPropertiesPath + "\"" }
					});
			}
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
		}

		public override bool OnStart()
		{
			try
			{
				DiscoverDirectories();
				ExtractJars();
				JavaInstaller.ExtractJdk(_javaInstallHome);
				WriteKafkaServerConfigFile(); // TODO: Handle role environment changes to rewrite the file and restart the server.
				WriteKafkaLog4jFile();
			}
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
			return base.OnStart();
		}

		private void DiscoverDirectories()
		{
			var installResource = RoleEnvironment.GetLocalResource("InstallDir");
			_javaInstallHome = Path.Combine(installResource.RootPath, "Java");
			_javaHome = Path.Combine(_javaInstallHome, "java");
			_jarsHome = Path.Combine(installResource.RootPath, "Jars");
			var dataResource = RoleEnvironment.GetLocalResource("DataDir");
			_dataDirectory = Path.Combine(dataResource.RootPath, "Data");
			Directory.CreateDirectory(_dataDirectory);
			_configsDirectory = Path.Combine(dataResource.RootPath, "Config");
			Directory.CreateDirectory(_configsDirectory);
			_logsDirectory = Path.Combine(dataResource.RootPath, "Logs");
			Directory.CreateDirectory(_logsDirectory);
			_kafkaServerPropertiesPath = Path.Combine(_configsDirectory, "server.properties");
			_kafkaLog4jPropertiesPath = Path.Combine(_configsDirectory, "log4j.properties");
		}

		private void UploadExceptionToBlob(Exception ex)
		{
			var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
			var container = storageAccount
					.CreateCloudBlobClient()
					.GetContainerReference("logs");
			container.CreateIfNotExists();
			container
					.GetBlockBlobReference("Exception from " + RoleEnvironment.CurrentRoleInstance.Id + " on " + DateTime.Now)
					.UploadText(ex.ToString());
		}

		private void WriteKafkaServerConfigFile()
		{
			var zookeeperRole = RoleEnvironment.Roles["Zookeeper"];
			var zookeeperHosts = zookeeperRole.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString());
			var zookeeperConnectionString = ZookeeperConfig.GetZookeeperConnectionString(zookeeperHosts);
			var myBrokerId = Int32.Parse(RoleEnvironment.CurrentRoleInstance.Id.Split('_').Last());
			var config = KafkaServerConfig.Default(myBrokerId, _dataDirectory, zookeeperConnectionString);
			config.WriteToFile(_kafkaServerPropertiesPath);
		}

		private void WriteKafkaLog4jFile()
		{
			var config = new KafkaLog4jConfig(_logsDirectory);
			config.WriteToFile(_kafkaLog4jPropertiesPath);
		}

		private void ExtractJars()
		{
			using (var rawStream = typeof(WorkerRole).Assembly.GetManifestResourceStream("KafkaBroker.Resources.Jars.zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(_jarsHome);
			}
		}
	}
}
