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
	public abstract class SparkNodeBase : RoleEntryPoint
	{
		private JavaInstaller _javaInstaller;
		private SparkRunner _sparkRunner;

		public override void Run()
		{
			try
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
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
		}

		public override bool OnStart()
		{
			try
			{
				InstallJava();
				InstallSpark();
			}
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
			return base.OnStart();
		}

		protected abstract bool IsMaster { get; }

		private void InstallJava()
		{
			_javaInstaller = new JavaInstaller(Path.Combine(InstallDirectory, "Java"));
			_javaInstaller.Setup();
		}

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
				javaHome: _javaInstaller.JavaHome,
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

		private static string GetIPAddress(RoleInstance i)
		{
			return i.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString();
		}

		private static string InstallDirectory
		{
			get { return RoleEnvironment.GetLocalResource("InstallDir").RootPath; }
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}

		private void UploadExceptionToBlob(Exception ex)
		{
			var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
			var container = storageAccount
					.CreateCloudBlobClient()
					.GetContainerReference("logs");
			container.CreateIfNotExists();
			container
					.GetBlockBlobReference("Exception from " + RoleEnvironment.CurrentRoleInstance.Id + " on " + DateTime.Now)
					.UploadText(ex.ToString());
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
