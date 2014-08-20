using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Hive;
using System.IO;
using System.Collections.Immutable;
using Microsoft.Experimental.Azure.CommonTestUtilities;

namespace HiveMetastore
{
	public class WorkerRole : HiveMetastoreNodeBase
	{
		protected override HiveMetastoreConfig GetMetastoreConfig()
		{
			var metastoreConfigInfo = ReadMetastoreFile().ToList();
			if (metastoreConfigInfo.Count != 4)
			{
				throw new InvalidOperationException("Invalid metastore configuration.");
			}
			return new HiveSqlServerMetastoreConfig(
				serverUri: metastoreConfigInfo[0],
				databaseName: metastoreConfigInfo[1],
				userName: metastoreConfigInfo[2],
				password: metastoreConfigInfo[3],
				extraProperties: WasbConfiguration.GetWasbConfigKeys());
		}

		private IEnumerable<string> ReadMetastoreFile()
		{
			return TestConfigFile.ReadFile("SqlMetastore");
		}
	}
}
