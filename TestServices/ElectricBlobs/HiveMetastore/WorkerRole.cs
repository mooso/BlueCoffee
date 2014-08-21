using Microsoft.Experimental.Azure.Hive;
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
using System.IO;
using Microsoft.Experimental.Azure.CommonTestUtilities;

namespace HiveMetastore
{
	public class WorkerRole : HiveMetastoreNodeBase
	{
		protected override HiveMetastoreConfig GetMetastoreConfig()
		{
			return new HiveDerbyMetastoreConfig(Path.Combine(DataDirectory, "DerbyData"),
				extraProperties: WasbConfiguration.GetWasbConfigKeys());
		}
	}
}
