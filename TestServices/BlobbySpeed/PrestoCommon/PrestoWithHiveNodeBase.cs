using Microsoft.Experimental.Azure.CommonTestUtilities;
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
	public abstract class PrestoWithHiveNodeBase : PrestoNodeBase
	{
		protected override IEnumerable<PrestoCatalogConfig> ConfigurePrestoCatalogs()
		{
			var hiveNode = RoleEnvironment.Roles["HiveMetastore"].Instances
				.Select(GetIPAddress)
				.First();
			Trace.TraceInformation("Hive node we'll use: " + hiveNode);
			var hiveCatalogConfig = new PrestoHiveCatalogConfig(
				metastoreUri: String.Format("thrift://{0}:9083", hiveNode),
				hiveConfigurationProperties: new Dictionary<string, string>()
				{
					{ "fs.azure.skip.metrics", "true" },
					{ "fs.azure.check.block.md5", "false" },
				}.Concat(WasbConfiguration.GetWasbConfigKeys()));
			return new PrestoCatalogConfig[] { hiveCatalogConfig };
		}
	}
}
