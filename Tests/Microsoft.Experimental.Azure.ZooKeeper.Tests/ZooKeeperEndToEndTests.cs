using Microsoft.Experimental.Azure.CommonTestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ZooKeeper.Tests
{
	[TestClass]
	public class ZooKeeperEndToEndTests
	{
		[TestMethod]
		[Ignore]
		public void SingleZooKeeperNodeTest()
		{
			var tempDirectory = @"C:\ZooKeeperTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var killer = new ProcessKiller();
			var zooKeeperDirectory = Path.Combine(tempDirectory, "ZooKeeper");
			var jarsDirectory = Path.Combine(zooKeeperDirectory, "lib");
			var zooKeeperTask = RunZooKeeper(killer, zooKeeperDirectory);
			try
			{
				var output = TestJavaRunner.RunJavaResourceFile(
					testClass: GetType(),
					javaResourceName: "SimpleZooKeeperTester.java",
					libDirectory: jarsDirectory);
				Assert.AreEqual("Success", output.StandardOutput.Trim(), output.StandardError);
			}
			finally
			{
				killer.KillAll();
				Task.WaitAll(zooKeeperTask);
			}
		}

		[TestMethod]
		[Ignore]
		public void ThreeZooKeeperNodesTest()
		{
			var tempDirectory = @"C:\ZooKeeperTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var killer = new ProcessKiller();
			const int numNodes = 3;
			var zooKeeperTasks = new Task[numNodes];
			var allNodes = Enumerable.Range(0, numNodes)
				.Select(i => new ZooKeeperQuorumPeer("localhost", 2881 + i, 3881 + i))
				.ToList();
			Parallel.For(0, numNodes, i =>
			{
				var zooKeeperDirectory = Path.Combine(tempDirectory, "ZooKeeper" + (i + 1));
				zooKeeperTasks[i] = RunZooKeeper(killer, zooKeeperDirectory, clientPort: 2181 + i, allNodes: allNodes, myId: i + 1);
			});
			Thread.Sleep(1000); // TODO: properly wait for the cluster to be up.
			var jarsDirectory = Path.Combine(tempDirectory, "ZooKeeper1", "lib");
			try
			{
				var output = TestJavaRunner.RunJavaResourceFile(
					testClass: GetType(),
					javaResourceName: "TwoNodeZooKeeperTester.java",
					libDirectory: jarsDirectory,
					arguments: "localhost:2181 localhost:2182");
				Assert.AreEqual("Success", output.StandardOutput.Trim(), output.StandardError);
			}
			finally
			{
				killer.KillAll();
				Task.WaitAll(zooKeeperTasks);
			}
		}

		private static Task RunZooKeeper(ProcessKiller killer, string zooKeeperDirectory,
			int clientPort = ZooKeeperConfig.DefaultClientPort,
			IEnumerable<ZooKeeperQuorumPeer> allNodes = null, int myId = 1)
		{
			var zooKeeperRunner = new ZooKeeperNodeRunner(
					resourceFileDirectory: ResourcePaths.ZooKeeperResourcesPath,
					config: new ZooKeeperConfig(Path.Combine(zooKeeperDirectory, "data"),
						clientPort: clientPort,
						allNodes: allNodes,
						myId: myId),
					configsDirectory: Path.Combine(zooKeeperDirectory, "conf"),
					logsDirectory: Path.Combine(zooKeeperDirectory, "log"),
					jarsDirectory: Path.Combine(zooKeeperDirectory, "lib"),
					javaHome: TestJavaRunner.JavaHome);
			zooKeeperRunner.Setup();
			var zooKeeperTask = Task.Factory.StartNew(() => zooKeeperRunner.Run(false, killer));
			return zooKeeperTask;
		}
	}
}
