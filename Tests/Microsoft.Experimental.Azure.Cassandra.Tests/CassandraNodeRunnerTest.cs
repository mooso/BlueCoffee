using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Cassandra.Tests
{
	[TestClass]
	public class CassandraNodeRunnerTest
	{
		[TestMethod]
		[Ignore]
		public void EndToEndTest()
		{
			var tempDirectory = @"C:\CassandraTestOutput";
			Directory.Delete(tempDirectory, recursive: true);
			var config = new CassandraConfig(
				clusterName: "Test cluster",
				clusterNodes: new[] { "127.0.0.1" },
				dataDirectories: new[] { Path.Combine(tempDirectory, "D1"), Path.Combine(tempDirectory, "D2") },
				commitLogDirectory: Path.Combine(tempDirectory, "commitlog"),
				savedCachesDirectory: Path.Combine(tempDirectory, "savedcaches"));
			var runner = new CassandraNodeRunner(
				jarsDirectory: Path.Combine(tempDirectory, "jars"),
				javaHome: @"C:\Program Files\Java\jdk1.7.0_21",
				logsDirctory: Path.Combine(tempDirectory, "logs"),
				configDirectory: Path.Combine(tempDirectory, "conf"),
				config: config);
			runner.Setup();
			runner.Run(runContinuous: false);
		}
	}
}
