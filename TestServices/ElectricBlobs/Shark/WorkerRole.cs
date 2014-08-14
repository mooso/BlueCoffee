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

namespace Shark
{
	public class WorkerRole : SharkNodeBase
	{
		protected override string DiscoverMasterNode()
		{
			return GetIPAddress(RoleEnvironment.Roles["Spark"].Instances.Single(i => i.Id.EndsWith("_0")));
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
				typeof(WorkerRole).Assembly.GetManifestResourceStream("Shark.WasbAccounts.txt"))
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
