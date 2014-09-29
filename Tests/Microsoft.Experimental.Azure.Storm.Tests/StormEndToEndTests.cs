using Microsoft.Experimental.Azure.CommonTestUtilities;
using Microsoft.Experimental.Azure.ZooKeeper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Storm.Tests
{
	[TestClass]
	public class StormEndToEndTests
	{
		private const string JavaHome = @"C:\Program Files\Java\jdk1.7.0_21";

		[TestMethod]
		public void RunStormClusterTest()
		{
			var tempDirectory = @"C:\StormTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var zooKeeperDirectory = Path.Combine(tempDirectory, "ZooKeeper");
			var stormDirectory = Path.Combine(tempDirectory, "Storm");
			var zooKeeperRunner = new ZooKeeperNodeRunner(
					resourceFileDirectory: ResourcePaths.ZooKeeperResourcesPath,
					dataDirectory: Path.Combine(zooKeeperDirectory, "data"),
					configsDirectory: Path.Combine(zooKeeperDirectory, "conf"),
					logsDirectory: Path.Combine(zooKeeperDirectory, "log"),
					jarsDirectory: Path.Combine(zooKeeperDirectory, "lib"),
					javaHome: JavaHome);
			zooKeeperRunner.Setup();
			var zooKeeperTask = Task.Factory.StartNew(() => zooKeeperRunner.Run(false));
			var stormConfig = new StormConfig(
				nimbusHost: "localhost",
				zooKeeperServers: new[] { "localhost" },
				stormLocalDirectory: Path.Combine(stormDirectory, "storm-local"));
			var stormRunner = new StormRunner(
				resourceFileDirectory: ResourcePaths.StormResourcesPath,
				stormHomeDirectory: stormDirectory,
				javaHome: JavaHome,
				logsDirectory: Path.Combine(stormDirectory, "logs"),
				config: stormConfig);
			stormRunner.Setup();
			var stormNimbusTask = Task.Factory.StartNew(() => stormRunner.RunNimbus(false));
			var stormSupervisorTask = Task.Factory.StartNew(() => stormRunner.RunSupervisor(false));
			Task.WaitAll(zooKeeperTask, stormNimbusTask, stormSupervisorTask);
		}
	}
}
