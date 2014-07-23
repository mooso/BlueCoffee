using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Presto.Tests
{
	[TestClass]
	public class PrestoNodeRunnerTest
	{
		[TestMethod]
		public void EndToEndTest()
		{
			var tempDirectory = @"C:\PrestoTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var config = new PrestoConfig(
				nodeId: "testnode",
				dataDirectory: Path.Combine(tempDirectory, "data"),
				pluginConfigDirectory: Path.Combine(tempDirectory, "etc"),
				pluginInstallDirectory: Path.Combine(tempDirectory, "plugin"),
				discoveryServerUri: "http://localhost:8080",
				catalogs: Enumerable.Empty<PrestoCatalogConfig>());
			var runner = new PrestoNodeRunner(
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
