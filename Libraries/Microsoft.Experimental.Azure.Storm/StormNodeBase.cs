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
		/// true for nimbus, false for supervisors.
		/// </summary>
		protected abstract bool IsNimbus { get; }

		/// <summary>
		/// Override GuardedRun to run Storm.
		/// </summary>
		protected sealed override void GuardedRun()
		{
			if (IsNimbus)
			{
				_stormRunner.RunNimbus();
			}
			else
			{
				_stormRunner.RunSupervisor();
			}
		}

		/// <summary>
		/// Override initialization to install Storm.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallStorm();
		}

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
		/// The Azure role for the Zoo Keeper nodes. Defaults to the role "ZooKeeperNode".
		/// </summary>
		protected virtual Role ZooKeeperRole
		{
			get
			{
				return RoleEnvironment.Roles["ZooKeeperNode"];
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

		private void InstallStorm()
		{
			var nimbus = DiscoverNimbusNode();
			Trace.TraceInformation("Nimbus node we'll use: " + nimbus);
			var zookeeperHosts = DiscoverZooKeeperHosts();
			var config = new StormConfig(
				stormLocalDirectory: Path.Combine(DataDirectory, "storm-local"),
				nimbusHost: nimbus,
				zooKeeperServers: zookeeperHosts,
				zooKeeperPort: ZooKeeperPort,
				maxNodeMemoryMb: MachineTotalMemoryMb - 512);
			_stormRunner = new StormRunner(
				resourceFileDirectory: GetResourcesDirectory(StormDirectory),
				stormHomeDirectory: Path.Combine(InstallDirectory, "Storm"),
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
