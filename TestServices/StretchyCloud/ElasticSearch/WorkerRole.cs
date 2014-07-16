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
using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.ElasticSearch;
using System.IO;

namespace ElasticSearch
{
	public class WorkerRole : RoleEntryPoint
	{
		private JavaInstaller _javaInstaller;
		private ESNodeRunner _esRunner;

		public override void Run()
		{
			try
			{
				_esRunner.Run();
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
				InstallCassandra();
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

		private void InstallCassandra()
		{
			var nodes = RoleEnvironment.CurrentRoleInstance.Role.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString());
			Trace.WriteLine("ES nodes we'll use: " + String.Join(",", nodes));
			var config = new ESConfig(
				clusterName: "AzureCluster",
				enableMulticastDiscovery: false,
				masterNodes: nodes,
				dataDirectories: new[] { Path.Combine(DataDirectory, "Data") }
			);
			_esRunner = new ESNodeRunner(
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				homeDirectory: InstallDirectory,
				javaHome: _javaInstaller.JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_esRunner.Setup();
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
