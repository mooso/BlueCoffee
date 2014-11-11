using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
		private const string ZooKeeperDirectory = "ZooKeeper";

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
			var zkNodes = RoleEnvironment.CurrentRoleInstance.Role.Instances.Select(GetIPAddress).ToList();
			zkNodes.Sort(StringComparer.OrdinalIgnoreCase); // So everyone gets the same list in the same order
			int myId = zkNodes.IndexOf(GetIPAddress(RoleEnvironment.CurrentRoleInstance)) + 1;
			Trace.TraceInformation("All ZK nodes: ({0}). My ID: {1}",
				string.Join(",", zkNodes), myId);
			ZooKeeperConfig config;
			if (zkNodes.Count <= 1)
			{
				// Single node configuration
				config = new ZooKeeperConfig(Path.Combine(DataDirectory, "Data"));
			}
			else
			{
				// Multi-node configuration
				config = new ZooKeeperConfig(Path.Combine(DataDirectory, "Data"),
					allNodes: zkNodes.Select(n => new ZooKeeperQuorumPeer(n)),
					myId: myId);
			}
			_nodeRunner = new ZooKeeperNodeRunner(
				resourceFileDirectory: GetResourcesDirectory(ZooKeeperDirectory),
				config: config,
				configsDirectory: Path.Combine(DataDirectory, "Config"),
				logsDirectory: Path.Combine(DataDirectory, "Logs"),
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: JavaHome);
			_nodeRunner.Setup();
		}
	}
}
