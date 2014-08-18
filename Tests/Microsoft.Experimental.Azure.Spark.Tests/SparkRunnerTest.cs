using Microsoft.Experimental.Azure.CommonTestUtilities;
using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Spark.Tests
{
	[TestClass]
	public class SparkRunnerTest
	{
		private const string JavaHome = @"C:\Program Files\Java\jdk1.7.0_21";

		[TestMethod]
		[Ignore]
		public void EndToEndTest()
		{
			var tempDirectory = @"C:\SparkTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var runner = SetupSpark(tempDirectory);
			var killer = new ProcessKiller();
			var masterTask = Task.Factory.StartNew(() => runner.RunMaster(runContinuous: false, monitor: killer));
			var slaveTask = Task.Factory.StartNew(() => runner.RunSlave(runContinuous: false, monitor: killer));
			var output = runner.RunExample("SparkPi", "10");
			Trace.TraceInformation(output.StandardError);
			killer.KillAll();
			Task.WaitAll(masterTask, slaveTask);
			StringAssert.Contains(output.StandardOutput, "Pi is roughly 3.14");
		}

		private sealed class ProcessKiller : ProcessMonitor
		{
			private readonly List<Process> _processes = new List<Process>();

			public void KillAll()
			{
				_processes.ForEach(p => p.Kill());
			}

			public override void ProcessStarted(Process process)
			{
				_processes.Add(process);
			}
		}

		private static SparkRunner SetupSpark(string sparkRoot)
		{
			var config = new SparkConfig(
				masterAddress: "localhost",
				masterPort: 7234,
				masterWebUIPort: 7235);
			var runner = new SparkRunner(
				resourceFileDirectory: ResourcePaths.SparkResourcesPath,
				sparkHome: Path.Combine(sparkRoot, "spark"),
				javaHome: JavaHome,
				config: config);
			runner.Setup();
			return runner;
		}
	}
}
