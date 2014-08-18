using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Experimental.Azure.ZooKeeper
{
	/// <summary>
	/// The base class for a typical Azure Zoo Keeper node.
	/// </summary>
	public abstract class ZooKeeperNodeBase : NodeWithJavaBase
	{
		private ZooKeeperNodeRunner _nodeRunner;
		private const string ZooKeeperDirectory = "ElasticSearch";

		/// <summary>
		/// The resource directories to download.
		/// </summary>
		protected override IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { ZooKeeperDirectory }.Concat(base.ResourceDirectoriesToDownload);
			}
		}

		/// <summary>
		/// Overrides the Run method to run Zoo Keeper.
		/// </summary>
		protected override void GuardedRun()
		{
			_nodeRunner.Run();
		}

		/// <summary>
		/// Overrides initialization to setup Zoo Keeper.
		/// </summary>
		protected override void PostJavaInstallInitialize()
		{
			InstallZooKeeper();
		}

		/// <summary>
		/// Gets the data directory - by default we look for a "DataDirectory" local resource.
		/// </summary>
		protected virtual string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDirectory").RootPath; }
		}

		private void InstallZooKeeper()
		{
			_nodeRunner = new ZooKeeperNodeRunner(
				resourceFileDirectory: GetResourcesDirectory(ZooKeeperDirectory),
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				configsDirectory: Path.Combine(DataDirectory, "Config"),
				logsDirectory: Path.Combine(DataDirectory, "Logs"),
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: JavaHome);
			_nodeRunner.Setup();
		}
	}
}
