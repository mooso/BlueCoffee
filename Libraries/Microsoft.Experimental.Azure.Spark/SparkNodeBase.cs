using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// The base class for a typical Azure Elastic Search node.
	/// </summary>
	public abstract class SparkNodeBase : NodeWithJavaBase
	{
		private SparkRunner _sparkRunner;

		/// <summary>
		/// Overrides the Run method to run Spark.
		/// </summary>
		protected sealed override void GuardedRun()
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

		/// <summary>
		/// Overrides initialization to setup Spark.
		/// </summary>
		protected sealed override void PostJavaInstallInitialize()
		{
			InstallSpark();
		}

		/// <summary>
		/// true if this is a master node, false if it's a worker node.
		/// </summary>
		protected abstract bool IsMaster { get; }

		/// <summary>
		/// Configure the Hadoop-side properties of Spark, to e.g. give WASB keys.
		/// </summary>
		/// <returns>The properties.</returns>
		protected abstract ImmutableDictionary<string, string> GetHadoopConfigProperties();

		private void InstallSpark()
		{
			var master = DiscoverMasterNode();
			Trace.TraceInformation("Master node we'll use: " + master);
			var config = new SparkConfig(
				masterAddress: master,
				masterPort: 8081,
				masterWebUIPort: 8080,
				hadoopConfigProperties: GetHadoopConfigProperties());
			_sparkRunner = new SparkRunner(
				sparkHome: Path.Combine(InstallDirectory, "Spark"),
				javaHome: JavaHome,
				config: config);
			_sparkRunner.Setup();
		}


		/// <summary>
		/// Get the IP addresse for all the Spark master node.
		/// </summary>
		/// <returns>Default implementation returns the first instance in the "SparkMaster" role.</returns>
		protected virtual string DiscoverMasterNode()
		{
			return RoleEnvironment.Roles["SparkMaster"].Instances
				.Select(GetIPAddress)
				.First();
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
