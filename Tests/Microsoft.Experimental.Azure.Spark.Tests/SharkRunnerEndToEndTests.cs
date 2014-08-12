using Microsoft.Experimental.Azure.Hive;
using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Spark;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Spark.Tests
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
				derbyDataDirectory: Path.Combine(hiveRoot, "metastore"),
				extraProperties: WasbProperties());

			var hiveTask = Task.Factory.StartNew(() => hiveRunner.RunMetastore(metastoreConfig, runContinuous: false, monitor: killer));
			var sparkRunner = SetupSpark(sparkRoot);
			var masterTask = Task.Factory.StartNew(() => sparkRunner.RunMaster(runContinuous: false, monitor: killer));
			var slaveTask = Task.Factory.StartNew(() => sparkRunner.RunSlave(runContinuous: false, monitor: killer));
			var sharkRunner = SetupShark(sharkRoot);
			var sharkTask = Task.Factory.StartNew(() => sharkRunner.RunSharkServer2(runContinuous: false, monitor: killer));

			try
			{
				var sharkLogFile = Path.Combine(sharkRoot, "logs", "SharkLog.log");
				WaitForCondition(() => File.Exists(sharkLogFile), TimeSpan.FromSeconds(30));
				WaitForCondition(() => SharedRead(sharkLogFile).Contains("SharkServer2 started"), TimeSpan.FromSeconds(30));
				var dataFilePath = Path.Combine(tempDirectory, "TestData.txt");
				File.WriteAllText(dataFilePath, String.Join("\n", 501, 623, 713), Encoding.ASCII);
				var beelineOutput = sharkRunner.RunBeeline(new[]
				{
					"CREATE TABLE t(a int);",
					String.Format("LOAD DATA LOCAL INPATH 'file:///{0}' INTO TABLE t;", dataFilePath.Replace('\\', '/')),
					"SELECT * FROM t WHERE a > 700;",
				});
				Trace.WriteLine("Stderr: " + beelineOutput.StandardError);
				StringAssert.Contains(beelineOutput.StandardOutput, "713");
			}
			finally
			{
				killer.KillAll();
				Task.WaitAll(hiveTask, sharkTask, masterTask, slaveTask);
			}
		}

		private static string SharedRead(string sharkLogFile)
		{
			using (var fileStream = new FileStream(sharkLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var textReader = new StreamReader(fileStream))
			{
				return textReader.ReadToEnd();
			}
		}

		private sealed class ProcessKiller : ProcessMonitor
		{
			private readonly List<Process> _processes = new List<Process>();

			public void KillAll()
			{
				Parallel.ForEach(_processes, p => p.Kill());
			}

			public override void ProcessStarted(Process process)
			{
				_processes.Add(process);
				process.Disposed += (s, e) => _processes.Remove(process);
			}
		}

		private static void WaitForCondition(Func<bool> condition, TimeSpan timeout)
		{
			var timer = Stopwatch.StartNew();
			do
			{
				if (condition())
				{
					return;
				}
				Thread.Sleep(100);
			} while (timer.Elapsed < timeout);
			Assert.Fail("Timed out.");
		}

		private static ImmutableDictionary<string, string> WasbProperties()
		{
			return new Dictionary<string, string>()
				{
					{ "fs.azure.skip.metrics", "true" },
					// Add account keys here.
				}.ToImmutableDictionary();
		}

		private static SharkRunner SetupShark(string sharkRoot)
		{
			var config = new SharkConfig(
				serverPort: 9444,
				metastoreUris: "thrift://localhost:9083",
				sparkMaster: "spark://localhost:7234",
				extraHiveConfig: WasbProperties());
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
				masterWebUIPort: 7235,
				hadoopConfigProperties: WasbProperties());
			var runner = new SparkRunner(
				sparkHome: sparkRoot,
				javaHome: JavaHome,
				config: config);
			runner.Setup();
			return runner;
		}
	}
}
