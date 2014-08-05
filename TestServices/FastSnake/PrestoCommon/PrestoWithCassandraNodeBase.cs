using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Presto;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrestoCommon
{
	public abstract class PrestoWithCassandraNodeBase : PrestoNodeBase
	{
		protected override IEnumerable<PrestoCatalogConfig> ConfigurePrestoCatalogs()
		{
			var cassandraNodes = RoleEnvironment.Roles["CassandraNode"].Instances
				.Select(GetIPAddress);
			Trace.TraceInformation("Cassandra nodes we'll use: " + String.Join(",", cassandraNodes));
			return new PrestoCatalogConfig[] { new PrestoCassandraCatalogConfig(cassandraNodes) };
		}
	}
}
