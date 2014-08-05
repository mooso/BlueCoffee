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

namespace HiveMetastore
{
	public class WorkerRole : NodeWithJavaBase
	{
		private HiveRunner _hiveRunner;
		private HiveSqlServerMetastoreConfig _metastoreConfig;

		protected override void GuardedRun()
		{
			_hiveRunner.RunMetastore(_metastoreConfig);
		}

		protected override void PostJavaInstallInitialize()
		{
			var metastoreConfigInfo = ReadMetastoreFile().ToList();
			if (metastoreConfigInfo.Count != 4)
			{
				throw new InvalidOperationException("Invalid metastore configuration.");
			}
			_metastoreConfig = new HiveSqlServerMetastoreConfig(
				serverUri: metastoreConfigInfo[0],
				databaseName: metastoreConfigInfo[1],
				userName: metastoreConfigInfo[2],
				password: metastoreConfigInfo[3]);
			InstallHive();
		}

		private void InstallHive()
		{
			_hiveRunner = new HiveRunner(
				jarsDirectory: Path.Combine(InstallDirectory, "jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"));
			_hiveRunner.Setup();
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}

		private IEnumerable<string> ReadMetastoreFile()
		{
			using (Stream resourceStream =
				GetType().Assembly.GetManifestResourceStream("HiveMetastore.SqlMetastore.txt"))
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
