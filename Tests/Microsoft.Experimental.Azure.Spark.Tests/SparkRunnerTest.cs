using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
			var masterTask = Task.Factory.StartNew(() => runner.RunMaster(runContinuous: false));
			var slaveTask = Task.Factory.StartNew(() => runner.RunSlave(runContinuous: false));
			Task.WaitAll(masterTask, slaveTask);
		}

		private static SparkRunner SetupSpark(string sparkRoot)
		{
			var config = new SparkConfig(
				masterAddress: "localhost",
				masterPort: 7234,
				masterWebUIPort: 7235);
			var runner = new SparkRunner(
				jarsDirectory: Path.Combine(sparkRoot, "jars"),
				javaHome: JavaHome,
				config: config);
			runner.Setup();
			return runner;
		}
	}
}
