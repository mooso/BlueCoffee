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
			var zooKeeperDirectory = Path.Combine(tempDirectory, "ZooKeeper");
			var jarsDirectory = Path.Combine(zooKeeperDirectory, "lib");
			var zooKeeperRunner = new ZooKeeperNodeRunner(
					resourceFileDirectory: ResourcePaths.ZooKeeperResourcesPath,
					dataDirectory: Path.Combine(zooKeeperDirectory, "data"),
					configsDirectory: Path.Combine(zooKeeperDirectory, "conf"),
					logsDirectory: Path.Combine(zooKeeperDirectory, "log"),
					jarsDirectory: jarsDirectory,
					javaHome: TestJavaRunner.JavaHome);
			zooKeeperRunner.Setup();
			var killer = new ProcessKiller();
			var zooKeeperTask = Task.Factory.StartNew(() => zooKeeperRunner.Run(false, killer));
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
	}
}
