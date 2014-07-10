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
using System.IO;
using Microsoft.Experimental.Azure.JavaPlatform;
using System.IO.Compression;
using Microsoft.Experimental.Azure.ZooKeeper;

namespace Zookeeper
{
	public class WorkerRole : RoleEntryPoint
	{
		private JavaInstaller _javaInstaller;
		private ZooKeeperNodeRunner _nodeRunner;

		public override void Run()
		{
			try
			{
				_nodeRunner.Run();
			}
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
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

		public override bool OnStart()
		{
			try
			{
				InstallJava();
				InstallZooKeeper();
				return base.OnStart();
			}
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
		}

		private void InstallJava()
		{
			_javaInstaller = new JavaInstaller(Path.Combine(InstallDirectory, "Java"));
			_javaInstaller.Setup();
		}

		private void InstallZooKeeper()
		{
			_nodeRunner = new ZooKeeperNodeRunner(
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				configsDirectory: Path.Combine(DataDirectory, "Config"),
				logsDirectory: Path.Combine(DataDirectory, "Logs"),
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: _javaInstaller.JavaHome);
			_nodeRunner.Setup();
		}

		private static string InstallDirectory
		{
			get { return RoleEnvironment.GetLocalResource("InstallDir").RootPath; }
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}
	}
}
