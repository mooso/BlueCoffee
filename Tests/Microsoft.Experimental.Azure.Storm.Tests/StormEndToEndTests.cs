using Microsoft.Experimental.Azure.CommonTestUtilities;
using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.ZooKeeper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Storm.Tests
{
	[TestClass]
	public class StormEndToEndTests
	{
		[TestMethod]
		[Ignore]
		public void RunStormClusterTest()
		{
			var tempDirectory = @"C:\StormTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var zooKeeperDirectory = Path.Combine(tempDirectory, "ZooKeeper");
			var stormDirectory = Path.Combine(tempDirectory, "Storm");
			var testJarDirectory = Path.Combine(tempDirectory, "testjar");

			var killer = new ProcessKiller();

			var stormRunner = SetupStorm(stormDirectory);

			var testJar = CompileTestJar(testJarDirectory, stormDirectory);

			var zooKeeperTask = RunZooKeeper(zooKeeperDirectory, killer);

			var stormNimbusTask = Task.Factory.StartNew(() => stormRunner.RunNimbus(false, killer));
			var stormSupervisorTask = Task.Factory.StartNew(() => stormRunner.RunSupervisor(false, killer));

			ConditionAwaiter.WaitForLogSnippet(Path.Combine(stormDirectory, "logs", "Storm.log"), "Starting Nimbus server");

			var testOutputFilePath = Path.Combine(testJarDirectory, "Output.txt");
			var testJarTask = Task.Factory.StartNew(() => stormRunner.RunJar("SimpleTopology", testJar,
				arguments: new[] { testOutputFilePath },
				runContinuous: false,
				monitor: killer));

			var stormUITask = Task.Factory.StartNew(() => stormRunner.RunUI(false, killer));

			try
			{
				ConditionAwaiter.WaitForLogSnippet(testOutputFilePath, "test!!!");
			}
			finally
			{
				killer.KillAll();
				Task.WaitAll(zooKeeperTask, stormNimbusTask, stormSupervisorTask, testJarTask, stormUITask);
			}
		}

		private static string CompileTestJar(string testJarDirectory, string stormDirectory)
		{
			return TestJavaRunner.CompileJarFromResource(
				testClass: typeof(StormEndToEndTests),
				targetDirectory: testJarDirectory,
				javaResourceName: "SimpleTopology.java",
				libDirectory: Path.Combine(stormDirectory, "lib"),
				targetJarName: "TestJar.jar");
		}

		private static StormRunner SetupStorm(string stormDirectory)
		{
			var stormConfig = new StormConfig(
				nimbusHost: "localhost",
				zooKeeperServers: new[] { "localhost" },
				stormLocalDirectory: Path.Combine(stormDirectory, "storm-local"));
			var stormRunner = new StormRunner(
				resourceFileDirectory: ResourcePaths.StormResourcesPath,
				stormHomeDirectory: stormDirectory,
				javaHome: TestJavaRunner.JavaHome,
				logsDirectory: Path.Combine(stormDirectory, "logs"),
				config: stormConfig);
			stormRunner.Setup();
			return stormRunner;
		}

		private static Task RunZooKeeper(string zooKeeperDirectory, ProcessMonitor monitor)
		{
			var zooKeeperRunner = new ZooKeeperNodeRunner(
					resourceFileDirectory: ResourcePaths.ZooKeeperResourcesPath,
					config: new ZooKeeperConfig(Path.Combine(zooKeeperDirectory, "data")),
					configsDirectory: Path.Combine(zooKeeperDirectory, "conf"),
					logsDirectory: Path.Combine(zooKeeperDirectory, "log"),
					jarsDirectory: Path.Combine(zooKeeperDirectory, "lib"),
					javaHome: TestJavaRunner.JavaHome);
			zooKeeperRunner.Setup();
			var zooKeeperTask = Task.Factory.StartNew(() => zooKeeperRunner.Run(false, monitor));
			return zooKeeperTask;
		}
	}
}
