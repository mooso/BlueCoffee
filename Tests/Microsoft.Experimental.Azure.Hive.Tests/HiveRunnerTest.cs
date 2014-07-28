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
			const int metastorePort = 9123, hiveServerPort = 9124;
			var metastoreConfig = new HiveDerbyMetastoreConfig(Path.Combine(tempDirectory, "metastore"), metastorePort);
			var metastoreTask = Task.Factory.StartNew(() => runner.RunMetastore(metastoreConfig, runContinuous: false));
			var serverConfig = new HiveServerConfig(hiveServerPort, "thrift://localhost:" + metastorePort);
			var serverTask = Task.Factory.StartNew(() => runner.RunHiveServer(serverConfig, runContinuous: false));
			Task.WaitAll(serverTask, metastoreTask);
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
	}
}
