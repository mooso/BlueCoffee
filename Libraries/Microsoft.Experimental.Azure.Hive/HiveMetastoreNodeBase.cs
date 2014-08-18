using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// The base class for a typical Azure Hive Metastore node.
	/// </summary>
	public abstract class HiveMetastoreNodeBase : NodeWithJavaBase
	{
		private HiveRunner _hiveRunner;
		private const string HiveDirectory = "Hive";

		/// <summary>
		/// The resource directories to download.
		/// </summary>
		protected override IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { HiveDirectory }.Concat(base.ResourceDirectoriesToDownload);
			}
		}

		/// <summary>
		/// Gets the metastore configuration (e.g. a HiveSqlServerMetastoreConfig for SQL Server-backed metastore).
		/// </summary>
		/// <returns>The configuration.</returns>
		protected abstract HiveMetastoreConfig GetMetastoreConfig();

		/// <summary>
		/// Overrides the Run method to run Hive metastore.
		/// </summary>
		protected sealed override void GuardedRun()
		{
			_hiveRunner.RunMetastore(GetMetastoreConfig());
		}

		/// <summary>
		/// Overrides initialization to setup Hive.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallHive();
		}

		private void InstallHive()
		{
			_hiveRunner = new HiveRunner(
				resourceFileDirectory: GetResourcesDirectory(HiveDirectory),
				jarsDirectory: Path.Combine(InstallDirectory, "jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(DataDirectory, "logs"),
				configDirectory: Path.Combine(InstallDirectory, "conf"));
			_hiveRunner.Setup();
		}

		/// <summary>
		/// Gets the data directory - by default we look for a "DataDirectory" local resource.
		/// </summary>
		protected virtual string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDirectory").RootPath; }
		}
	}
}
