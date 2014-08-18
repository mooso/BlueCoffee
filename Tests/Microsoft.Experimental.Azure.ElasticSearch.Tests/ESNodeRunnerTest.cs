using Microsoft.Experimental.Azure.CommonTestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ElasticSearch.Tests
{
	[TestClass]
	public class ESNodeRunnerTest
	{
		[TestMethod]
		[Ignore]
		public void EndToEndTest()
		{
			var tempDirectory = @"C:\ESTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var config = new ESConfig(
				clusterName: "Test cluster",
				enableMulticastDiscovery: false);
			var runner = new ESNodeRunner(
				resourceFileDirectory: ResourcePaths.ElasticSearchResourcesPath,
				jarsDirectory: Path.Combine(tempDirectory, "jars"),
				javaHome: @"C:\Program Files\Java\jdk1.7.0_21",
				logsDirctory: Path.Combine(tempDirectory, "logs"),
				configDirectory: Path.Combine(tempDirectory, "conf"),
				homeDirectory: tempDirectory,
				config: config);
			runner.Setup();
			runner.Run(runContinuous: false);
		}
	}
}
