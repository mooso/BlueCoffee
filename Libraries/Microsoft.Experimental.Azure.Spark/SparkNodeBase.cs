using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// The base class for a typical Azure Spark node.
	/// </summary>
	public abstract class SparkNodeBase : SharkSparkNodeBase
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
			AddExtraJarsIfNeeded(Path.Combine(InstallDirectory, "Spark", "lib"));
		}

		/// <summary>
		/// true if this is a master node, false if it's a worker node.
		/// </summary>
		protected abstract bool IsMaster { get; }

		/// <summary>
		/// Add any extra Jar files Spark needs.
		/// </summary>
		/// <param name="jarsDirectory">The jars directory.</param>
		protected virtual void AddExtraJarsIfNeeded(string jarsDirectory)
		{ }

		/// <summary>
		/// Configure the Hadoop-side properties of Spark, to e.g. give WASB keys.
		/// </summary>
		/// <returns>The properties.</returns>
		protected abstract ImmutableDictionary<string, string> GetHadoopConfigProperties();

		/// <summary>
		/// The memory bound on the node.
		/// </summary>
		protected virtual int MaxNodeMemoryMb
		{
			get
			{
				return MachineTotalMemoryMb - 1024;
			}
		}

		/// <summary>
		/// The memory bound on each standalone executor.
		/// </summary>
		protected virtual int ExecutorMemoryMb
		{
			get
			{
				return Math.Min(512, MachineTotalMemoryMb / Environment.ProcessorCount);
			}
		}
		/// <summary>
		/// Other Spark properties than what's explicitly given.
		/// </summary>
		public virtual ImmutableDictionary<string, string> ExtraSparkProperties
		{
			get
			{
				return null;
			}
		}

		private void InstallSpark()
		{
			var master = DiscoverMasterNode();
			Trace.TraceInformation("Master node we'll use: " + master);
			var config = new SparkConfig(
				masterAddress: master,
				masterPort: 8081,
				masterWebUIPort: 8080,
				hadoopConfigProperties: GetHadoopConfigProperties(),
				maxNodeMemoryMb: MaxNodeMemoryMb,
				executorMemoryMb : ExecutorMemoryMb,
				extraSparkProperties: ExtraSparkProperties);
			_sparkRunner = new SparkRunner(
				resourceFileDirectory: SparkResourceDirectory,
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
			if (RoleEnvironment.IsEmulated)
			{
				return "localhost";
			}
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

		/// <summary>
		/// The total memory on the machine in MB.
		/// </summary>
		protected static int MachineTotalMemoryMb
		{
			get
			{
				return (int)(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / (1024 * 1024));
			}
		}
	}
}
