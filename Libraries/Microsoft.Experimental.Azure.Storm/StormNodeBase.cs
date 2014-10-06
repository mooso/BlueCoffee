using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Storm
{
	/// <summary>
	/// Common base class for a Storm node Azure role.
	/// </summary>
	public abstract class StormNodeBase : NodeWithJavaBase
	{
		private StormRunner _stormRunner;
		private const string StormDirectory = "Storm";

		/// <summary>
		/// The resource directories to download.
		/// </summary>
		protected override IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { StormDirectory }.Concat(base.ResourceDirectoriesToDownload);
			}
		}

		/// <summary>
		/// The type of work this node is doing.
		/// </summary>
		protected abstract StormNodeType NodeType { get; }

		/// <summary>
		/// The Storm runner we're using.
		/// </summary>
		protected StormRunner StormRunner { get { return _stormRunner; } }

		/// <summary>
		/// Override GuardedRun to run Storm.
		/// </summary>
		protected sealed override void GuardedRun()
		{
			Task stormTask;
			switch (NodeType)
			{
				case StormNodeType.Nimbus:
					stormTask = Task.Factory.StartNew(() => _stormRunner.RunNimbus());
					break;
				case StormNodeType.Supervisor:
					stormTask = Task.Factory.StartNew(() => _stormRunner.RunSupervisor());
					break;
				case StormNodeType.UI:
					stormTask = Task.Factory.StartNew(() => _stormRunner.RunUI());
					break;
				case StormNodeType.Drpc:
					stormTask = Task.Factory.StartNew(() => _stormRunner.RunDrpc());
					break;
				case StormNodeType.NimbusWithUI:
					stormTask = Task.WhenAll(
						Task.Factory.StartNew(() => _stormRunner.RunNimbus()),
						Task.Factory.StartNew(() => _stormRunner.RunUI()));
					break;
				case StormNodeType.SupervisorWithDrpc:
					stormTask = Task.WhenAll(
						Task.Factory.StartNew(() => _stormRunner.RunSupervisor()),
						Task.Factory.StartNew(() => _stormRunner.RunDrpc()));
					break;
				case StormNodeType.Custom:
					stormTask = Task.FromResult(0);
					break;
				default:
					throw new InvalidOperationException("Unknown storm node type: " + NodeType);
			}
			var otherTask = StartOtherWork();
			Task.WaitAll(otherTask, stormTask);
		}

		/// <summary>
		/// Optional method to start any tasks that will run in parallel with Storm on this node.
		/// </summary>
		/// <returns></returns>
		protected virtual Task StartOtherWork()
		{
			return Task.FromResult(0);
		}

		/// <summary>
		/// Override initialization to install Storm.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallStorm();
			PostStormInstallInitialize();
		}

		/// <summary>
		/// Optional method to do any initialization after installing Storm.
		/// </summary>
		protected virtual void PostStormInstallInitialize()
		{ }

		/// <summary>
		/// The Azure role for nimbus, defaults to the role named Nimbus.
		/// </summary>
		protected virtual Role NimbusRole
		{
			get
			{
				return RoleEnvironment.Roles["Nimbus"];
			}
		}

		/// <summary>
		/// The Azure role for the DRPC server.
		/// Defaults to either the role named StormDrpc or Supervisor if either exist, otherwise null.
		/// Can be null if this Storm cluster won't have DRPC servers.
		/// </summary>
		protected virtual Role DrpcRole
		{
			get
			{
				Role drpcRole;
				if (RoleEnvironment.Roles.TryGetValue("StormDrpc", out drpcRole) ||
					RoleEnvironment.Roles.TryGetValue("Supervisor", out drpcRole))
				{
					return drpcRole;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// The directory where we'll put storm-local, defaults to the local resource "DataDirectory".
		/// </summary>
		protected virtual string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDirectory").RootPath; }
		}

		/// <summary>
		/// The port that the ZooKeeper nodes are listening on. Defaults to 2181.
		/// </summary>
		protected virtual int ZooKeeperPort { get { return 2181; } }

		/// <summary>
		/// The Azure role for the Zoo Keeper nodes. Defaults to the role "ZooKeeperNode" or "ZooKeeper".
		/// </summary>
		protected virtual Role ZooKeeperRole
		{
			get
			{
				return GetRole("ZooKeeperNode", "ZooKeeper");
			}
		}

		/// <summary>
		/// Discovers the Zoo Keeper node addresses. Defaults to all the nodes in the ZooKeeperRole.
		/// </summary>
		/// <returns>The list of IP addresses for Zoo Keeper nodes.</returns>
		protected virtual IEnumerable<string> DiscoverZooKeeperHosts()
		{
			if (RoleEnvironment.IsEmulated)
			{
				return new[] { "localhost" };
			}
			return ZooKeeperRole.Instances.Select(GetIPAddress);
		}

		/// <summary>
		/// Get the IP address for the Nimbus node.
		/// </summary>
		/// <returns>Default implementation returns the first instance in the NimbusRole role.</returns>
		protected virtual string DiscoverNimbusNode()
		{
			if (RoleEnvironment.IsEmulated)
			{
				return "localhost";
			}
			return NimbusRole.Instances
					.Select(GetIPAddress)
					.First();
		}

		/// <summary>
		/// Get the IP address for the DRPC nodes if they exist.
		/// </summary>
		/// <returns>Default implementation returns all the instances in the DrpcRole role if it's not null.</returns>
		protected virtual IEnumerable<string> DiscoverDrpcNodes()
		{
			if (DrpcRole == null)
			{
				return null;
			}
			if (RoleEnvironment.IsEmulated)
			{
				return new[] { "localhost" };
			}
			return DrpcRole.Instances
					.Select(GetIPAddress);
		}

		/// <summary>
		/// The Storm home directory.
		/// </summary>
		protected string StormHomeDirectory { get { return Path.Combine(InstallDirectory, "Storm"); } }

		private void InstallStorm()
		{
			var nimbus = DiscoverNimbusNode();
			Trace.TraceInformation("Nimbus node we'll use: " + nimbus);
			var zookeeperHosts = DiscoverZooKeeperHosts();
			var config = new StormConfig(
				stormLocalDirectory: Path.Combine(DataDirectory, "storm-local"),
				nimbusHost: nimbus,
				drpcServers: DiscoverDrpcNodes(),
				zooKeeperServers: zookeeperHosts,
				zooKeeperPort: ZooKeeperPort,
				maxNodeMemoryMb: MachineTotalMemoryMb - 512);
			_stormRunner = new StormRunner(
				resourceFileDirectory: GetResourcesDirectory(StormDirectory),
				stormHomeDirectory: StormHomeDirectory,
				javaHome: JavaHome,
				logsDirectory: Path.Combine(DataDirectory, "Logs"),
				config: config);
			_stormRunner.Setup();
		}

		private static int MachineTotalMemoryMb
		{
			get
			{
				return (int)(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / (1024 * 1024));
			}
		}
	}
}
