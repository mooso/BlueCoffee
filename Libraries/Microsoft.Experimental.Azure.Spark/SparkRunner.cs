using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// Runs a Spark master or slave node.
	/// </summary>
	public sealed class SparkRunner
	{
		private readonly string _jarsDirectory;
		private readonly string _javaHome;
		private readonly SparkConfig _config;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="jarsDirectory">The directory to use for jar files.</param>
		/// <param name="javaHome">The directory where Java is isntalled.</param>
		/// <param name="config">The configuration.</param>
		public SparkRunner(string jarsDirectory, string javaHome, SparkConfig config)
		{
			_jarsDirectory = jarsDirectory;
			_javaHome = javaHome;
			_config = config;
		}

		/// <summary>
		/// Setup Elastic Search.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in
				new[] { _jarsDirectory })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
		}

		/// <summary>
		/// Run Spark Master.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		public void RunMaster(bool runContinuous = true)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "org.apache.spark.deploy.master.Master";
			var classPathEntries = JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory);
			runner.RunClass(className,
				String.Format("--ip {0} --port {1} --webui-port {2}",
					_config.MasterAddress, _config.MasterPort, _config.MasterWebUIPort),
				classPathEntries,
				extraJavaOptions: new[]
				{
					"-XX:+UseParNewGC",
					"-XX:+UseConcMarkSweepGC",
					"-XX:CMSInitiatingOccupancyFraction=75",
					"-XX:+UseCMSInitiatingOccupancyOnly",
				},
				defines: new Dictionary<string, string>
				{
				},
				runContinuous: runContinuous);
		}

		/// <summary>
		/// Run Spark Slave.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		public void RunSlave(bool runContinuous = true)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "org.apache.spark.deploy.worker.Worker";
			var classPathEntries = JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory);
			runner.RunClass(className,
				_config.SparkMasterUri,
				classPathEntries,
				extraJavaOptions: new[]
				{
					"-XX:+UseParNewGC",
					"-XX:+UseConcMarkSweepGC",
					"-XX:CMSInitiatingOccupancyFraction=75",
					"-XX:+UseCMSInitiatingOccupancyOnly",
				},
				defines: new Dictionary<string, string>
				{
				},
				runContinuous: runContinuous);
		}

		private void ExtractResourceArchive(string resourceName, string targetDirectory)
		{
			using (var rawStream = GetType().Assembly.GetManifestResourceStream(
				"Microsoft.Experimental.Azure.Spark.Resources." + resourceName + ".zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(targetDirectory);
			}
		}

		private void ExtractJars()
		{
			ExtractResourceArchive("Jars", _jarsDirectory);
		}
	}
}
