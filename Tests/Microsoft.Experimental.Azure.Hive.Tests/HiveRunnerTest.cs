using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive.Tests
{
	[TestClass]
	public class HiveRunnerTest
	{
		private const string JavaHome = @"C:\Program Files\Java\jdk1.7.0_21";

		[TestMethod]
		[Ignore]
		public void EndToEndTest()
		{
			var tempDirectory = @"C:\HiveTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var runner = SetupHive(tempDirectory);
			runner.RunMetastore(runContinuous: false);
		}

		private static HiveRunner SetupHive(string prestoRoot)
		{
			var config = new HiveConfig(
				derbyDataDirectory: Path.Combine(prestoRoot, "metastore"));
			var runner = new HiveRunner(
				jarsDirectory: Path.Combine(prestoRoot, "jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(prestoRoot, "logs"),
				configDirectory: Path.Combine(prestoRoot, "conf"),
				config: config);
			runner.Setup();
			return runner;
		}
	}
}
