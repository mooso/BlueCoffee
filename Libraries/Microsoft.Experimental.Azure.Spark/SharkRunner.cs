using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// Runs a Shark server.
	/// </summary>
	public sealed class SharkRunner : SharkSparkRunnerBase
	{
		private readonly SharkConfig _config;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="resourceFileDirectory">The directory that contains my resource files.</param>
		/// <param name="sharkHome">The directory to use for Shark home.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="traceLevel">The trace level to use.</param>
		public SharkRunner(string resourceFileDirectory, string sharkHome, string javaHome, SharkConfig config,
			Log4jTraceLevel traceLevel = Log4jTraceLevel.INFO)
			: base(resourceFileDirectory, sharkHome, javaHome, traceLevel)
		{
			_config = config;
		}

		/// <summary>
		/// Write the Shark-specific configuration.
		/// </summary>
		protected override void WriteConfig()
		{
			var configXml = _config.GetHiveConfigXml();
			configXml.Save(Path.Combine(ConfDirectory, "hive-site.xml"));
		}

		/// <summary>
		/// The log file name.
		/// </summary>
		protected override string LogFileName
		{
			get { return "SharkLog.log"; }
		}

		/// <summary>
		/// Run Shark server 2.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		/// <param name="monitor">Optional process monitor.</param>
		/// <param name="debugPort">If given, we'll start in debug mode so Eclipse cat attach at the given port.</param>
		public void RunSharkServer2(bool runContinuous = true, ProcessMonitor monitor = null, int? debugPort = null)
		{
			var runner = CreateJavaRunner();
			const string className = "org.apache.spark.sql.hive.thriftserver.HiveThriftServer2";
			runner.RunClass(className,
				"",
				ClassPath(),
				maxMemoryMb: _config.MaxMemoryMb,
				extraJavaOptions: new[]
				{
					"-XX:+UseParNewGC",
					"-XX:+UseConcMarkSweepGC",
					"-XX:CMSInitiatingOccupancyFraction=75",
					"-XX:+UseCMSInitiatingOccupancyOnly",
				}.Concat(debugPort.HasValue ? JavaRunner.GetStandardDebugArguments(debugPort.Value) : Enumerable.Empty<string>()),
				defines: SharkServerDefines,
				runContinuous: runContinuous,
				monitor: monitor,
				environmentVariables: new Dictionary<string, string>()
				{
					{ "HIVE_SERVER2_THRIFT_PORT", _config.ServerPort.ToString() },
				});
		}

		/// <summary>
		/// Runs beeline.
		/// </summary>
		/// <param name="commands">The SQL commands to execute.</param>
		/// <param name="serverAddress">The server IP address.</param>
		/// <returns>The process output.</returns>
		public ProcessOutput RunBeeline(IEnumerable<string> commands, string serverAddress = "localhost")
		{
			var runner = CreateJavaRunner();
			const string className = "org.apache.hive.beeline.BeeLine";
			var tracer = new StringProcessOutputTracer();
			var exitCode = runner.RunClass(className,
				String.Format("-u jdbc:hive2://{0}:{1} {2}", serverAddress, _config.ServerPort,
					String.Join(" ", commands.Select(c => String.Format("-e \"{0}\"", c)))),
				ClassPath(),
				maxMemoryMb: _config.MaxMemoryMb,
				defines: HadoopHomeAndLog4jDefines,
				environmentVariables: new Dictionary<string, string>()
				{
				},
				tracer: tracer,
				runContinuous: false);
			return tracer.GetOutputSoFar();
		}

		private ImmutableDictionary<string, string> SharkServerDefines
		{
			get
			{
				return HadoopHomeAndLog4jDefines
					.Add("spark.executor.memory", _config.ExecutorMemoryMb + "m")
					.Add("spark.master", _config.SparkMaster)
					.AddRange(_config.ExtraSparkProperties);
			}
		}
	}
}
