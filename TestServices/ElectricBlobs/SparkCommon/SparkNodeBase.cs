using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Spark;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkCommon
{
	public abstract class SparkNodeBase : NodeWithJavaBase
	{
		private SparkRunner _sparkRunner;

		protected override void GuardedRun()
		{
			if (IsMaster)
			{
				_sparkRunner.RunMaster();
			}
			else
			{
				_sparkRunner.RunSlave();
			}
		}

		protected override void PostJavaInstallInitialize()
		{
			InstallSpark();
		}

		protected abstract bool IsMaster { get; }

		private void InstallSpark()
		{
			var master = RoleEnvironment.Roles["SparkMaster"].Instances
				.Select(GetIPAddress)
				.First();
			Trace.TraceInformation("Master node we'll use: " + master);
			var config = new SparkConfig(
				masterAddress: master,
				masterPort: 8081,
				masterWebUIPort: 8080,
				hadoopConfigProperties: GetWasbConfigKeys().ToImmutableDictionary());
			_sparkRunner = new SparkRunner(
				sparkHome: Path.Combine(InstallDirectory, "Spark"),
				javaHome: JavaHome,
				config: config);
			_sparkRunner.Setup();
		}

		private Dictionary<string, string> GetWasbConfigKeys()
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
			return wasbConfigKeys;
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}

		private static IEnumerable<string> ReadWasbAccountsFile()
		{
			using (Stream resourceStream =
				typeof(SparkNodeBase).Assembly.GetManifestResourceStream("SparkCommon.WasbAccounts.txt"))
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
