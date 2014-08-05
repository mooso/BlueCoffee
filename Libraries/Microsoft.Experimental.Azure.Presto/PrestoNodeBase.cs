using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Presto
{
	/// <summary>
	/// Common base class for a Presto node Azure role.
	/// </summary>
	public abstract class PrestoNodeBase : NodeWithJavaBase
	{
		private PrestoNodeRunner _prestoRunner;

		/// <summary>
		/// Configure the catalogs to use for this Presto node.
		/// </summary>
		/// <returns>The catalogs.</returns>
		protected abstract IEnumerable<PrestoCatalogConfig> ConfigurePrestoCatalogs();

		/// <summary>
		/// true for coordinators, false for workers.
		/// </summary>
		protected abstract bool IsCoordinator { get; }

		/// <summary>
		/// Override GuardedRun to run Presto.
		/// </summary>
		protected sealed override void GuardedRun()
		{
			_prestoRunner.Run();
		}

		/// <summary>
		/// Override initialization to install Presto.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallPresto();
		}

		/// <summary>
		/// The Azure role for the coordinator, defaults to the role named PrestoCoordinator.
		/// </summary>
		protected virtual Role PrestoCoordinatorRole
		{
			get
			{
				return RoleEnvironment.Roles["PrestoCoordinator"];
			}
		}

		/// <summary>
		/// The data directory, defaults to the local resource "DataDirectory".
		/// </summary>
		protected virtual string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDirectory").RootPath; }
		}

		/// <summary>
		/// The TCP port to use for the HTTP binding for the coordinator.
		/// </summary>
		protected virtual int CoordinatorHttpPort { get { return 8080; } }

		/// <summary>
		/// The TCP port to use for the HTTP binding for the worker.
		/// </summary>
		protected virtual int WorkerHttpPort { get { return 8081; } }

		private void InstallPresto()
		{
			var coordinator = PrestoCoordinatorRole.Instances
				.Select(GetIPAddress)
				.First();
			Trace.TraceInformation("Coordinator node we'll use: " + coordinator);
			var config = new PrestoConfig(
				environmentName: "azurecluster",
				nodeId: SanitizedNodeId,
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				pluginConfigDirectory: Path.Combine(InstallDirectory, "etc"),
				pluginInstallDirectory: Path.Combine(InstallDirectory, "plugin"),
				discoveryServerUri: "http://" + coordinator + ":" + CoordinatorHttpPort,
				catalogs: ConfigurePrestoCatalogs(),
				isCoodrinator: IsCoordinator,
				isWorker: !IsCoordinator,
				isDiscoveryServer: IsCoordinator,
				maxNodeMemoryMb: MachineTotalMemoryMb - 512,
				maxTaskMemoryMb: MachineTotalMemoryMb - 1024,
				httpPort: IsCoordinator ? CoordinatorHttpPort : WorkerHttpPort);
			_prestoRunner = new PrestoNodeRunner(
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_prestoRunner.Setup();
		}

		private static string SanitizedNodeId
		{
			get
			{
				const string badCharacters = "().";
				var currentId = RoleEnvironment.CurrentRoleInstance.Id;
				foreach (var currentChar in badCharacters)
				{
					currentId = currentId.Replace(currentChar, '_');
				}
				return currentId;
			}
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
