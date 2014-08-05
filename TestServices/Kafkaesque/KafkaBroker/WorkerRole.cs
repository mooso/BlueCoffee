using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Kafka;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;

namespace KafkaBroker
{
	public class WorkerRole : NodeWithJavaBase
	{
		private KafkaBrokerRunner _kafkaRunner;
		private const int ZooKeeperPort = 2181;

		protected override void GuardedRun()
		{
			_kafkaRunner.Run();
		}

		protected override void PostJavaInstallInitialize()
		{
			InstallKafka();
		}

		private void InstallKafka()
		{
			var zookeeperRole = RoleEnvironment.Roles["ZooKeeperNode"];
			var zookeeperHosts = zookeeperRole.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString());
			var myBrokerId = Int32.Parse(RoleEnvironment.CurrentRoleInstance.Id.Split('_').Last());
			_kafkaRunner = new KafkaBrokerRunner(
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

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}
	}
}
