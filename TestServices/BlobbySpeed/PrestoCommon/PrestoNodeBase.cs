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
	public abstract class PrestoNodeBase : NodeWithJavaBase
	{
		private PrestoNodeRunner _prestoRunner;

		protected override void GuardedRun()
		{
			_prestoRunner.Run();
		}

		protected override void PostJavaInstallInitialize()
		{
			InstallPresto();
		}

		protected abstract bool IsCoordinator { get; }

		private void InstallPresto()
		{
			var coordinator = RoleEnvironment.Roles["PrestoCoordinator"].Instances
				.Select(GetIPAddress)
				.First();
			Trace.TraceInformation("Coordinator node we'll use: " + coordinator);
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
			var config = new PrestoConfig(
				environmentName: "azurecluster",
				nodeId: RoleEnvironment.CurrentRoleInstance.Id,
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				pluginConfigDirectory: Path.Combine(InstallDirectory, "etc"),
				pluginInstallDirectory: Path.Combine(InstallDirectory, "plugin"),
				discoveryServerUri: "http://" + coordinator + ":8080",
				catalogs: new[] { hiveCatalogConfig },
				isCoodrinator: IsCoordinator,
				isWorker: !IsCoordinator,
				isDiscoveryServer: IsCoordinator,
				maxNodeMemoryMb: 3 * 1024,
				maxTaskMemoryMb: (int)(2.5 * 1024));
			_prestoRunner = new PrestoNodeRunner(
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "Logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"),
				config: config);
			_prestoRunner.Setup();
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

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}

		private static IEnumerable<string> ReadWasbAccountsFile()
		{
			using (Stream resourceStream =
				typeof(PrestoNodeBase).Assembly.GetManifestResourceStream("PrestoCommon.WasbAccounts.txt"))
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
