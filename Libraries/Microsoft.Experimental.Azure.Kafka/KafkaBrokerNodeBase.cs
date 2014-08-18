using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Kafka
{
	/// <summary>
	/// The base class for a typical Azure Kafka broker node.
	/// </summary>
	public abstract class KafkaBrokerNodeBase : NodeWithJavaBase
	{
		private KafkaBrokerRunner _kafkaRunner;
		private const string KafkaDirectory = "Kafka";

		/// <summary>
		/// The resource directories to download.
		/// </summary>
		protected override IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { KafkaDirectory }.Concat(base.ResourceDirectoriesToDownload);
			}
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
			return ZooKeeperRole.Instances.Select(GetIPAddress);
		}

		/// <summary>
		/// Gets the data directory - by default we look for a "DataDirectory" local resource.
		/// </summary>
		protected virtual string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDirectory").RootPath; }
		}

		/// <summary>
		/// Overrides the Run method to run Kafka.
		/// </summary>
		protected sealed override void GuardedRun()
		{
			_kafkaRunner.Run();
		}

		/// <summary>
		/// Overrides initialization to setup Kafka.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallKafka();
		}

		private void InstallKafka()
		{
			var zookeeperHosts = DiscoverZooKeeperHosts();
			var myBrokerId = Int32.Parse(RoleEnvironment.CurrentRoleInstance.Id.Split('_').Last());
			_kafkaRunner = new KafkaBrokerRunner(
				resourceFileDirectory: GetResourcesDirectory(KafkaDirectory),
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				configsDirectory: Path.Combine(DataDirectory, "Config"),
				logsDirectory: Path.Combine(DataDirectory, "Logs"),
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				zooKeeperHosts: zookeeperHosts,
				zooKeeperPort: ZooKeeperPort,
				brokerId: myBrokerId,
				javaHome: JavaHome);
			_kafkaRunner.Setup();
		}
	}
}
