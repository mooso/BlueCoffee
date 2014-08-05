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
	public class WorkerRole : NodeWithJavaBase
	{
		private ESNodeRunner _esRunner;

		protected override void GuardedRun()
		{
			_esRunner.Run();
		}

		protected override void PostJavaInstallInitialize()
		{
			InstallCassandra();
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
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_esRunner.Setup();
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}
	}
}
