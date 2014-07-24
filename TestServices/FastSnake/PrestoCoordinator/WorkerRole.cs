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
using Microsoft.Experimental.Azure.Presto;
using System.IO;

namespace PrestoCoordinator
{
	public class WorkerRole : RoleEntryPoint
	{
		private JavaInstaller _javaInstaller;
		private PrestoNodeRunner _prestoRunner;

		public override void Run()
		{
			try
			{
				_prestoRunner.Run();
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
				InstallPresto();
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

		private void InstallPresto()
		{
			var coordinator = RoleEnvironment.CurrentRoleInstance.Role.Instances
				.Select(GetIPAddress)
				.First();
			Trace.TraceInformation("Coordinator node we'll use: " + coordinator);
			var cassandraNodes = RoleEnvironment.Roles["CassandraNode"].Instances
				.Select(GetIPAddress);
			Trace.TraceInformation("Cassandra nodes we'll use: " + String.Join(",", cassandraNodes));
			var config = new PrestoConfig(
				environmentName: "azurecluster",
				nodeId: RoleEnvironment.CurrentRoleInstance.Id,
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				pluginConfigDirectory: Path.Combine(InstallDirectory, "etc"),
				pluginInstallDirectory: Path.Combine(InstallDirectory, "plugin"),
				discoveryServerUri: "http://" + coordinator + ":8080",
				catalogs: new[] { new PrestoCassandraCatalogConfig(cassandraNodes) },
				isCoodrinator: true,
				isWorker: false,
				isDiscoveryServer: true);
			_prestoRunner = new PrestoNodeRunner(
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: _javaInstaller.JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_prestoRunner.Setup();
		}

		private static string GetIPAddress(RoleInstance i)
		{
			return i.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString();
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
