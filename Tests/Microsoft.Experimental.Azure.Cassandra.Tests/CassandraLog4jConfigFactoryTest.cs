using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Cassandra.Tests
{
	[TestClass]
	public class CassandraLog4jConfigFactoryTest
	{
		[TestMethod]
		public void TestFileOutput()
		{
			var config = CassandraLog4jConfigFactory.CreateConfig(@"C:\MyLogs");
			Assert.IsNotNull(config);
			var propFile = config.ToPropertiesFile();
			var fileText = propFile.ConstructFileText();
			Trace.WriteLine(fileText);
			StringAssert.Contains(fileText, "C:/MyLogs/system.log");
		}
	}
}
