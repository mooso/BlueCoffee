using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Cassandra.Tests
{
	[TestClass]
	public class CassandraConfigTest
	{
		[TestMethod]
		public void TestSimpleConfig()
		{
			var config = new CassandraConfig(
				clusterName: "Test Cluster",
				clusterNodes: new[] { "127.0.0.1", "1.2.3.4" },
				dataDirectories: new[] { @"c:\d1", @"c:\d2" },
				commitLogDirectory: @"C:\logs",
				savedCachesDirectory: @"C:\SavedCaches");
			string configOutput;
			using (var writer = new StringWriter())
			{
				config.WriteToYamlFile(writer);
				writer.Flush();
				configOutput = writer.ToString();
			}
			Trace.WriteLine(configOutput);
			StringAssert.Contains(configOutput, "cluster_name: Test Cluster");
		}
	}
}
