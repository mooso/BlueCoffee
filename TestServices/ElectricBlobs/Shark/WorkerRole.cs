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
using Microsoft.Experimental.Azure.Spark;
using System.Collections.Immutable;
using System.IO;
using Microsoft.Experimental.Azure.CommonTestUtilities;

namespace Shark
{
	public class WorkerRole : SharkNodeBase
	{
		protected override ImmutableDictionary<string, string> GetHadoopConfigProperties()
		{
			return WasbConfiguration.GetWasbConfigKeys();
		}
	}
}
