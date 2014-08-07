using Microsoft.Experimental.Azure.Hive;
using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Spark;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Shark.Tests
{
	[TestClass]
	public class SharkNodeRunnerTest
	{
		private const string JavaHome = @"C:\Program Files\Java\jdk1.7.0_21";

		[TestMethod]
		[Ignore]
		public void EndToEndTest()
		{
			var tempDirectory = @"C:\SharkTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var hiveRoot = Path.Combine(tempDirectory, "HiveRoot");
			var sharkRoot = Path.Combine(tempDirectory, "Shark");
			var sparkRoot = Path.Combine(tempDirectory, "Spark");
			var killer = new ProcessKiller();
			var hiveRunner = SetupHive(hiveRoot);
			var metastoreConfig = new HiveDerbyMetastoreConfig(
				derbyDataDirectory: Path.Combine(hiveRoot, "metastore"));
			var hiveTask = Task.Factory.StartNew(() => hiveRunner.RunMetastore(metastoreConfig, runContinuous: false, monitor: killer));
			var sparkRunner = SetupSpark(sparkRoot);
			var masterTask = Task.Factory.StartNew(() => sparkRunner.RunMaster(runContinuous: false, monitor: killer));
			var slaveTask = Task.Factory.StartNew(() => sparkRunner.RunSlave(runContinuous: false, monitor: killer));
			var sharkRunner = SetupShark(sharkRoot, sparkRoot);
			var sharkTask = Task.Factory.StartNew(() => sharkRunner.RunSharkServer2(runContinuous: false, monitor: killer));

			var sharkCliStartInfo = new ProcessStartInfo(
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SharkCliShell.exe"),
				String.Format("\"{0}\" \"{1}\" \"{2}\"", sharkRoot, sparkRoot, JavaHome)
			);

			using (Process cliProcess = Process.Start(sharkCliStartInfo))
			{
				cliProcess.WaitForExit();
			}
			killer.KillAll();
			Task.WaitAll(hiveTask, sharkTask, masterTask, slaveTask);
		}

		private sealed class ProcessKiller : ProcessMonitor
		{
			private readonly List<Process> _processes = new List<Process>();

			public void KillAll()
			{
				_processes.ForEach(p =>
				{
					if (!p.WaitForExit(0))
					{
						p.Kill();
					}
				});
			}

			public override void ProcessStarted(Process process)
			{
				_processes.Add(process);
			}
		}

		private static SharkRunner SetupShark(string sharkRoot, string sparkRoot)
		{
			var config = new SharkConfig(
				serverPort: 9444,
				metastoreUris: "thrift://localhost:9083",
				sparkHome: sparkRoot,
				sparkMaster: "spark://localhost:7234");
			var runner = new SharkRunner(
				sharkHome: sharkRoot,
				javaHome: JavaHome,
				config: config);
			runner.Setup();
			return runner;
		}

		private static HiveRunner SetupHive(string hiveRoot)
		{
			var runner = new HiveRunner(
				jarsDirectory: Path.Combine(hiveRoot, "jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(hiveRoot, "logs"),
				configDirectory: Path.Combine(hiveRoot, "conf"));
			runner.Setup();
			return runner;
		}

		private static SparkRunner SetupSpark(string sparkRoot)
		{
			var config = new SparkConfig(
				masterAddress: "localhost",
				masterPort: 7234,
				masterWebUIPort: 7235);
			var runner = new SparkRunner(
				sparkHome: Path.Combine(sparkRoot, "spark"),
				javaHome: JavaHome,
				config: config);
			runner.Setup();
			return runner;
		}
	}
}
