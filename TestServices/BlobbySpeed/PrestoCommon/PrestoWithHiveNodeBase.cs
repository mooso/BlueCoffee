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
				}.Concat(GetWasbConfigKeys()));
			return new PrestoCatalogConfig[] { hiveCatalogConfig };
		}

		private List<KeyValuePair<string, string>> GetWasbConfigKeys()
		{
			var wasbAccountsInfo = ReadWasbAccountsFile().ToList();
			if ((wasbAccountsInfo.Count % 2) != 0)
			{
				throw new InvalidOperationException("Invalid WASB accounts info file.");
			}
			var wasbConfigKeys = new List<KeyValuePair<string, string>>();
			for (int i = 0; i < wasbAccountsInfo.Count; i += 2)
			{
				wasbConfigKeys.Add(new KeyValuePair<string, string>(
					"fs.azure.account.key." + wasbAccountsInfo[i] + ".blob.core.windows.net",
					wasbAccountsInfo[i + 1]));
			}
			return wasbConfigKeys;
		}

		private static IEnumerable<string> ReadWasbAccountsFile()
		{
			using (Stream resourceStream =
				typeof(PrestoWithHiveNodeBase).Assembly.GetManifestResourceStream("PrestoCommon.WasbAccounts.txt"))
			{
				StreamReader reader = new StreamReader(resourceStream);
				string currentLine;
				while ((currentLine = reader.ReadLine()) != null)
				{
					currentLine = currentLine.Trim();
					if (currentLine.StartsWith("#")) // Comment
					{
						continue;
					}
					if (currentLine == "")
					{
						continue;
					}
					yield return currentLine;
				}
				reader.Close();
			}
		}
	}
}
