using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Cassandra
{
	/// <summary>
	/// The base class for a typical Azure Cassandra node.
	/// </summary>
	public abstract class CassandraNodeBase : NodeWithJavaBase
	{
		private CassandraNodeRunner _cassandraRunner;
		private const string CassandraDirectory = "Cassandra";

		/// <summary>
		/// The resouce directories to download.
		/// </summary>
		protected override IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { CassandraDirectory }.Concat(base.ResourceDirectoriesToDownload);
			}
		}

		/// <summary>
		/// Overrides the Run method to run Cassandra.
		/// </summary>
		protected sealed override void GuardedRun()
		{
			_cassandraRunner.Run();
		}

		/// <summary>
		/// Overrides initialization to setup Cassandra.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallCassandra();
		}

		/// <summary>
		/// Get the list of IP addresses for all the Cassandra nodes in the cluster.
		/// </summary>
		/// <returns>Default implementation returns the addresses of all the instances in this role.</returns>
		protected virtual IEnumerable<string> DiscoverCassandraNodes()
		{
			return RoleEnvironment.CurrentRoleInstance.Role.Instances.Select(GetIPAddress);
		}

		/// <summary>
		/// Gets the data directory - by default we look for a "DataDirectory" local resource.
		/// </summary>
		protected virtual string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDirectory").RootPath; }
		}

		private void InstallCassandra()
		{
			var nodes = DiscoverCassandraNodes();
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
				resourceFileDirectory: GetResourcesDirectory(CassandraDirectory),
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_cassandraRunner.Setup();
		}
	}
}
