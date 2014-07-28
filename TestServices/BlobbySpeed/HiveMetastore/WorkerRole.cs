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
	public class WorkerRole : RoleEntryPoint
	{
		private JavaInstaller _javaInstaller;
		private HiveRunner _hiveRunner;
		private HiveSqlServerMetastoreConfig _metastoreConfig;

		public override void Run()
		{
			try
			{
				_hiveRunner.RunMetastore(_metastoreConfig);
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
				InstallJava();
				InstallHive();
			}
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
			return base.OnStart();
		}

		private void InstallJava()
		{
			_javaInstaller = new JavaInstaller(Path.Combine(InstallDirectory, "Java"));
			_javaInstaller.Setup();
		}

		private void InstallHive()
		{
			_hiveRunner = new HiveRunner(
				jarsDirectory: Path.Combine(InstallDirectory, "jars"),
				javaHome: _javaInstaller.JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"));
			_hiveRunner.Setup();
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
