using Microsoft.Experimental.Azure.CommonTestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
		public void TwoZooKeeperNodesTest()
		{
			var tempDirectory = @"C:\ZooKeeperTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var killer = new ProcessKiller();
			var zooKeeper1Directory = Path.Combine(tempDirectory, "ZooKeeper1");
			var zooKeeper2Directory = Path.Combine(tempDirectory, "ZooKeeper2");
			var jarsDirectory = Path.Combine(zooKeeper1Directory, "lib");
			var zooKeeper1Task = RunZooKeeper(killer, zooKeeper1Directory);
			var zooKeeper2Task = RunZooKeeper(killer, zooKeeper2Directory, port: 2182);
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
				Task.WaitAll(zooKeeper1Task, zooKeeper2Task);
			}
		}

		private static Task RunZooKeeper(ProcessKiller killer, string zooKeeperDirectory, int port = 2181)
		{
			var zooKeeperRunner = new ZooKeeperNodeRunner(
					resourceFileDirectory: ResourcePaths.ZooKeeperResourcesPath,
					config: new ZooKeeperConfig(Path.Combine(zooKeeperDirectory, "data"), port: port),
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
