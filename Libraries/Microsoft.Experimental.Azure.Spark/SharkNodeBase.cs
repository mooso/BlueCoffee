using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.Spark;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// The base class for a typical Azure Shark node.
	/// </summary>
	public abstract class SharkNodeBase : SharkSparkNodeBase
	{
		private SharkRunner _sharkRunner;

		/// <summary>
		/// Overrides the Run method to run Shark.
		/// </summary>
		protected sealed override void GuardedRun()
		{
			_sharkRunner.RunSharkServer2();
		}

		/// <summary>
		/// Overrides initialization to setup Shark.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallShark();
		}

		/// <summary>
		/// Configure the Hadoop-side properties of Shark, to e.g. give WASB keys.
		/// </summary>
		/// <returns>The properties.</returns>
		protected abstract ImmutableDictionary<string, string> GetHadoopConfigProperties();

		private void InstallShark()
		{
			var master = DiscoverMasterNode();
			Trace.TraceInformation("Master node we'll use: " + master);
			var metastore = DiscoverMetastoreNode();
			Trace.TraceInformation("Metastore node we'll use: " + master);
			var config = new SharkConfig(
				serverPort: 8082,
				metastoreUris: "thrift://" + metastore + ":9083",
				sparkMaster: "spark://" + master + ":8081",
				executorMemoryMb : ExecutorMemoryMb,
				extraSparkProperties: ExtraSparkProperties,
				extraHiveConfig:
					GetHadoopConfigProperties()
					.Add( "fs.azure.skip.metrics", "true")
					.Add("hive.exec.local.scratchdir", "C:/Resources/temp/HiveScratch")
					.Add("hive.querylog.location", Path.Combine(InstallDirectory, "Shark", "QueryHistory")));
			_sharkRunner = new SharkRunner(
				resourceFileDirectory: SparkResourceDirectory,
				sharkHome: Path.Combine(InstallDirectory, "Shark"),
				javaHome: JavaHome,
				config: config);
			_sharkRunner.Setup();
		}

		/// <summary>
		/// Get the IP address for the Spark master node.
		/// </summary>
		/// <returns>Default implementation returns the first instance in the "SparkMaster" role.</returns>
		protected virtual string DiscoverMasterNode()
		{
			if (RoleEnvironment.IsEmulated)
			{
				return "localhost";
			}
			return RoleEnvironment.Roles["SparkMaster"].Instances
				.Select(GetIPAddress)
				.First();
		}

		/// <summary>
		/// Get the IP address for the Hive metastore node.
		/// </summary>
		/// <returns>Default implementation returns the first instance in the "HiveMetastore" role.</returns>
		protected virtual string DiscoverMetastoreNode()
		{
			if (RoleEnvironment.IsEmulated)
			{
				return "localhost";
			}
			return RoleEnvironment.Roles["HiveMetastore"].Instances
				.Select(GetIPAddress)
				.First();
		}
	}
}
