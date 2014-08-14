using Microsoft.Experimental.Azure.Spark;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Spark
{
	public class WorkerRole : SparkNodeBase
	{
		protected override bool IsMaster
		{
			get { return IsMasterInstance(RoleEnvironment.CurrentRoleInstance); }
		}

		protected override string DiscoverMasterNode()
		{
			return GetIPAddress(RoleEnvironment.CurrentRoleInstance.Role.Instances.Single(IsMasterInstance));
		}

		private static bool IsMasterInstance(RoleInstance instance)
		{
			return instance.Id.EndsWith("_0");
		}

		protected override ImmutableDictionary<string, string> GetHadoopConfigProperties()
		{
			var wasbAccountsInfo = ReadWasbAccountsFile().ToList();
			if ((wasbAccountsInfo.Count % 2) != 0)
			{
				throw new InvalidOperationException("Invalid WASB accounts info file.");
			}
			var wasbConfigKeys = new Dictionary<string, string>();
			for (int i = 0; i < wasbAccountsInfo.Count; i += 2)
			{
				wasbConfigKeys.Add(
					"fs.azure.account.key." + wasbAccountsInfo[i] + ".blob.core.windows.net",
					wasbAccountsInfo[i + 1]);
			}
			return wasbConfigKeys.ToImmutableDictionary();
		}

		private static IEnumerable<string> ReadWasbAccountsFile()
		{
			using (Stream resourceStream =
				typeof(WorkerRole).Assembly.GetManifestResourceStream("Spark.WasbAccounts.txt"))
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
