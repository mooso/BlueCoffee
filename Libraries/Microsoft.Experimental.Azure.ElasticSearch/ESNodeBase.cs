using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ElasticSearch
{
	/// <summary>
	/// The base class for a typical Azure Elastic Search node.
	/// </summary>
	public abstract class ESNodeBase : NodeWithJavaBase
	{
		private ESNodeRunner _esRunner;
		private const string ElasticSearchDirectory = "ElasticSearch";

		/// <summary>
		/// The resource directories to download.
		/// </summary>
		protected override IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { ElasticSearchDirectory }.Concat(base.ResourceDirectoriesToDownload);
			}
		}

		/// <summary>
		/// Overrides the Run method to run ES.
		/// </summary>
		protected sealed override void GuardedRun()
		{
			_esRunner.Run();
		}

		/// <summary>
		/// Overrides initialization to setup ES.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallES();
		}

		/// <summary>
		/// Get the list of IP addresses for all the ES nodes in the cluster.
		/// </summary>
		/// <returns>Default implementation returns the addresses of all the instances in this role.</returns>
		protected virtual IEnumerable<string> DiscoverESNodes()
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

		private void InstallES()
		{
			var nodes = DiscoverESNodes();
			Trace.WriteLine("ES nodes we'll use: " + String.Join(",", nodes));
			var config = new ESConfig(
				clusterName: "AzureCluster",
				enableMulticastDiscovery: false,
				masterNodes: nodes,
				dataDirectories: new[] { Path.Combine(DataDirectory, "Data") }
			);
			_esRunner = new ESNodeRunner(
				resourceFileDirectory: GetResourcesDirectory(ElasticSearchDirectory),
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				homeDirectory: InstallDirectory,
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_esRunner.Setup();
		}
	}
}
