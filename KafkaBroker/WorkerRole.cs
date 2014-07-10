using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Kafka;
using Microsoft.Experimental.Azure.ZooKeeper;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;

namespace KafkaBroker
{
	public class WorkerRole : RoleEntryPoint
	{
		private JavaInstaller _javaInstaller;
		private KafkaBrokerRunner _kafkaRunner;

		public override void Run()
		{
			try
			{
				_kafkaRunner.Run();
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
				InstallJava();
				InstallKafka();
			}
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
			return base.OnStart();
		}

		private void InstallJava()
		{
			_javaInstaller = new JavaInstaller(Path.Combine(InstallDirectory, "Java"));
			_javaInstaller.Setup();
		}

		private void InstallKafka()
		{
			var zookeeperRole = RoleEnvironment.Roles["Zookeeper"];
			var zookeeperHosts = zookeeperRole.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString());
			var myBrokerId = Int32.Parse(RoleEnvironment.CurrentRoleInstance.Id.Split('_').Last());
			_kafkaRunner = new KafkaBrokerRunner(
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				configsDirectory: Path.Combine(DataDirectory, "Config"),
				logsDirectory: Path.Combine(DataDirectory, "Logs"),
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				zooKeeperHosts: zookeeperHosts,
				zooKeeperPort: ZooKeeperConfig.DefaultPort,
				brokerId: myBrokerId,
				javaHome: _javaInstaller.JavaHome);
			_kafkaRunner.Setup();
		}

		private static string InstallDirectory
		{
			get { return RoleEnvironment.GetLocalResource("InstallDir").RootPath; }
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
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
	}
}
