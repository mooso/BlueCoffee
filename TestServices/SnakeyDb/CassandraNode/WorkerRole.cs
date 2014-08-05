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
using Microsoft.Experimental.Azure.Cassandra;
using System.IO;

namespace CassandraNode
{
	public class WorkerRole : NodeWithJavaBase
	{
		private CassandraNodeRunner _cassandraRunner;

		protected override void GuardedRun()
		{
			_cassandraRunner.Run();
		}

		protected override void PostJavaInstallInitialize()
		{
			InstallCassandra();
		}

		private void InstallCassandra()
		{
			var nodes = RoleEnvironment.CurrentRoleInstance.Role.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString());
			Trace.WriteLine("Cassandra nodes we'll use: " + String.Join(",", nodes));
			var config = new CassandraConfig(
				clusterName: "AzureCluster",
				clusterNodes: nodes,
				dataDirectories: new[] { Path.Combine(DataDirectory, "Data") },
				commitLogDirectory: Path.Combine(DataDirectory, "CommitLog"),
				savedCachesDirectory: Path.Combine(DataDirectory, "SavedCaches"),
				ringDelay: TimeSpan.FromMinutes(5) // Role instances can start up at different times, 30 seconds is not enough.
			);
			_cassandraRunner = new CassandraNodeRunner(
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_cassandraRunner.Setup();
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}
	}
}
