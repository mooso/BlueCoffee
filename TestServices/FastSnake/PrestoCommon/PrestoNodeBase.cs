using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Presto;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrestoCommon
{
	public abstract class PrestoNodeBase : NodeWithJavaBase
	{
		private PrestoNodeRunner _prestoRunner;

		protected override void GuardedRun()
		{
			_prestoRunner.Run();
		}

		protected override void PostJavaInstallInitialize()
		{
			InstallPresto();
		}

		protected abstract bool IsCoordinator { get; }

		private void InstallPresto()
		{
			var coordinator = RoleEnvironment.Roles["PrestoCoordinator"].Instances
				.Select(GetIPAddress)
				.First();
			Trace.TraceInformation("Coordinator node we'll use: " + coordinator);
			var cassandraNodes = RoleEnvironment.Roles["CassandraNode"].Instances
				.Select(GetIPAddress);
			Trace.TraceInformation("Cassandra nodes we'll use: " + String.Join(",", cassandraNodes));
			var config = new PrestoConfig(
				environmentName: "azurecluster",
				nodeId: RoleEnvironment.CurrentRoleInstance.Id.Replace('(', '_').Replace(')', '_').Replace('.', '_'),
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				pluginConfigDirectory: Path.Combine(InstallDirectory, "etc"),
				pluginInstallDirectory: Path.Combine(InstallDirectory, "plugin"),
				discoveryServerUri: "http://" + coordinator + ":8080",
				catalogs: new[] { new PrestoCassandraCatalogConfig(cassandraNodes) },
				isCoodrinator: IsCoordinator,
				isWorker: !IsCoordinator,
				isDiscoveryServer: IsCoordinator,
				httpPort: IsCoordinator ? 8080 : 8081);
			_prestoRunner = new PrestoNodeRunner(
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_prestoRunner.Setup();
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}
	}
}
