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
				extraProperties: GetWasbConfigKeys());
		}

		private IEnumerable<string> ReadMetastoreFile()
		{
			return ReadResourceFile("SqlMetastore");
		}

		private ImmutableDictionary<string, string> GetWasbConfigKeys()
		{
			var wasbAccountsInfo = ReadWasbAccountsFile().ToList();
			if ((wasbAccountsInfo.Count % 2) != 0)
			{
				throw new InvalidOperationException("Invalid WASB accounts info file.");
			}
			var wasbConfigKeys = ImmutableDictionary<string, string>.Empty;
			for (int i = 0; i < wasbAccountsInfo.Count; i += 2)
			{
				wasbConfigKeys = wasbConfigKeys.Add(
					"fs.azure.account.key." + wasbAccountsInfo[i] + ".blob.core.windows.net",
					wasbAccountsInfo[i + 1]);
			}
			return wasbConfigKeys;
		}

		private IEnumerable<string> ReadWasbAccountsFile()
		{
			return ReadResourceFile("WasbAccounts");
		}

		private IEnumerable<string> ReadResourceFile(string name)
		{
			using (Stream resourceStream =
				GetType().Assembly.GetManifestResourceStream("HiveMetastore." + name + ".txt"))
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
