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
		private const string JavaHome = @"C:\Program Files\Java\jdk1.7.0_21";

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
			if (!Directory.Exists(testJarDirectory))
			{
				Directory.CreateDirectory(testJarDirectory);
			}
			// Write out the java file
			var javaFileName = "SimpleTopology.java";
			var jarFileName = "TestJar.jar";
			WriteResourceToFile(testJarDirectory, javaFileName);
			// Compile it
			RunTool(Path.Combine(JavaHome, "bin", "javac.exe"), String.Format(
				"-cp \"{0}\" -sourcepath \"{1}\" -d \"{1}\" {2}",
				Path.Combine(stormDirectory, "lib", "*"), testJarDirectory, Path.Combine(testJarDirectory, javaFileName)));
			// Jar it
			RunTool(Path.Combine(JavaHome, "bin", "jar.exe"), String.Format(
				"cf \"{0}\" -C \"{1}\" .",
				Path.Combine(testJarDirectory, jarFileName), testJarDirectory));
			return Path.Combine(testJarDirectory, jarFileName);
		}

		private static void WriteResourceToFile(string outputDirectory, string fileName)
		{
			var javaFilePath = Path.Combine(outputDirectory, fileName);
			using (var resourceStream =
				typeof(StormEndToEndTests).Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.Storm.Tests." + fileName))
			using (var javaFileStream = File.OpenWrite(javaFilePath))
			{
				var buffer = new byte[1024];
				int numBytesRead;
				while ((numBytesRead = resourceStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					javaFileStream.Write(buffer, 0, numBytesRead);
				}
			}
		}

		private static void RunTool(string toolPath, string args)
		{
			Trace.TraceInformation("Starting: {0} {1}", toolPath, args);
			var processStartInfo = new ProcessStartInfo(toolPath, args)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
			};
			var stdOut = new StringBuilder();
			var stdErr = new StringBuilder();
			int exitCode;
			using (var process = new Process() { StartInfo = processStartInfo })
			{
				process.OutputDataReceived += (source, eventArgs) => stdOut.AppendLine(eventArgs.Data);
				process.ErrorDataReceived += (source, eventArgs) => stdErr.AppendLine(eventArgs.Data);
				process.Start();
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();
				if (!process.WaitForExit(10 * 1000))
				{
					process.Kill();
					Assert.Fail("Timed out waiting for {0} to exit. Std Out:\n{1}\n Std Err:\n{2}",
						toolPath, stdOut.ToString(), stdErr.ToString());
				}
				exitCode = process.ExitCode;
			}
			Assert.AreEqual(0, exitCode,
				"Process {0} exited with non-zero exit code. Std Out:\n{1}\n Std Err:\n{2}",
					toolPath, stdOut.ToString(), stdErr.ToString());
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
				javaHome: JavaHome,
				logsDirectory: Path.Combine(stormDirectory, "logs"),
				config: stormConfig);
			stormRunner.Setup();
			return stormRunner;
		}

		private static Task RunZooKeeper(string zooKeeperDirectory, ProcessMonitor monitor)
		{
			var zooKeeperRunner = new ZooKeeperNodeRunner(
					resourceFileDirectory: ResourcePaths.ZooKeeperResourcesPath,
					dataDirectory: Path.Combine(zooKeeperDirectory, "data"),
					configsDirectory: Path.Combine(zooKeeperDirectory, "conf"),
					logsDirectory: Path.Combine(zooKeeperDirectory, "log"),
					jarsDirectory: Path.Combine(zooKeeperDirectory, "lib"),
					javaHome: JavaHome);
			zooKeeperRunner.Setup();
			var zooKeeperTask = Task.Factory.StartNew(() => zooKeeperRunner.Run(false, monitor));
			return zooKeeperTask;
		}
	}
}
