using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	public sealed class SparkRunner : SharkSparkRunnerBase
	{
		private readonly SparkConfig _config;
		private const string _childTaskLog4jPropertiesFileName = "child-log4j.properties";
		private readonly Log4jTraceLevel _childTaskTraceLevel;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="resourceFileDirectory">The directory that contains my resource files.</param>
		/// <param name="sparkHome">The directory to use for Spark home.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="traceLevel">The trace level to use.</param>
		/// <param name="childTaskTraceLevel">The child task trace level to use.</param>
		public SparkRunner(string resourceFileDirectory, string sparkHome, string javaHome, SparkConfig config,
			Log4jTraceLevel traceLevel = Log4jTraceLevel.INFO,
			Log4jTraceLevel childTaskTraceLevel = Log4jTraceLevel.INFO)
			: base(resourceFileDirectory, sparkHome, javaHome, traceLevel)
		{
			_config = config;
			_childTaskTraceLevel = childTaskTraceLevel;
		}

		/// <summary>
		/// Write the Spark-specific configuration.
		/// </summary>
		protected override void WriteConfig()
		{
			_config.WriteHadoopCoreSiteXml(ConfDirectory);
			CreateChildTaskLog4jConfig().ToPropertiesFile().WriteToFile(Path.Combine(ConfDirectory, _childTaskLog4jPropertiesFileName));
		}

		private Log4jConfig CreateChildTaskLog4jConfig()
		{
			var layout = LayoutDefinition.PatternLayout("[%d{ISO8601}][%-5p][%-25c] %m%n");

			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender("console",
				layout: layout);

			var rootLogger = new RootLoggerDefinition(_childTaskTraceLevel, consoleAppender);

			return new Log4jConfig(rootLogger, Enumerable.Empty<ChildLoggerDefinition>());
		}

		/// <summary>
		/// The log file name.
		/// </summary>
		protected override string LogFileName
		{
			get { return "SparkLog.log"; }
		}

		/// <summary>
		/// Run Spark Master.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		/// <param name="monitor">Optional process monitor.</param>
		public void RunMaster(bool runContinuous = true, ProcessMonitor monitor = null)
		{
			var runner = CreateJavaRunner();
			const string className = "org.apache.spark.deploy.master.Master";
			runner.RunClass(className,
				String.Format("--ip {0} --port {1} --webui-port {2}",
					_config.MasterAddress, _config.MasterPort, _config.MasterWebUIPort),
				ClassPath(),
				maxMemoryMb: _config.MaxNodeMemoryMb,
				extraJavaOptions: new[]
				{
					"-XX:+UseParNewGC",
					"-XX:+UseConcMarkSweepGC",
					"-XX:CMSInitiatingOccupancyFraction=75",
					"-XX:+UseCMSInitiatingOccupancyOnly",
				},
				defines: SparkNodeDefines,
				runContinuous: runContinuous,
				monitor: monitor,
				environmentVariables: SparkEnvironmentVariables());
		}

		/// <summary>
		/// Run Spark Slave.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		/// <param name="monitor">Optional process monitor.</param>
		public void RunSlave(bool runContinuous = true, ProcessMonitor monitor = null)
		{
			var runner = CreateJavaRunner();
			const string className = "org.apache.spark.deploy.worker.Worker";
			runner.RunClass(className,
				_config.SparkMasterUri,
				ClassPath(),
				maxMemoryMb: _config.MaxNodeMemoryMb,
				extraJavaOptions: new[]
				{
					"-XX:+UseParNewGC",
					"-XX:+UseConcMarkSweepGC",
					"-XX:CMSInitiatingOccupancyFraction=75",
					"-XX:+UseCMSInitiatingOccupancyOnly",
				},
				defines: SparkNodeDefines,
				runContinuous: runContinuous,
				monitor: monitor,
				environmentVariables: SparkEnvironmentVariables());
		}

		private ImmutableDictionary<string, string> SparkNodeDefines
		{
			get
			{
				return HadoopHomeAndLog4jDefines
					.Add("spark.executor.memory", _config.ExecutorMemoryMb + "m")
					.AddRange(_config.ExtraSparkProperties);
			}
		}

		/// <summary>
		/// Runs a Spark example.
		/// </summary>
		/// <param name="exampleName">The name of the example (e.g. SparkPi).</param>
		/// <param name="exampleArgs">Arguments given to the example.</param>
		/// <returns>The stdout/stderr from running the example.</returns>
		public ProcessOutput RunExample(string exampleName, string exampleArgs)
		{
			var runner = CreateJavaRunner();
			const string submitterClassName = "org.apache.spark.deploy.SparkSubmit";
			var exampleClassName = "org.apache.spark.examples." + exampleName;
			const string exampleJarName = "spark-examples-1.1.0-SNAPSHOT-hadoop2.4.0.jar";
			var exampleJarPath = Path.Combine(JarsDirectory, exampleJarName);
			var processOutputTracer = new StringProcessOutputTracer();
			runner.RunClass(submitterClassName,
				String.Format("--master {0} --class {1} \"{2}\" {3}",
					_config.SparkMasterUri, exampleClassName, exampleJarPath, exampleArgs),
				ClassPath(),
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
				runContinuous: false,
				tracer: processOutputTracer,
				environmentVariables: SparkEnvironmentVariables());
			return processOutputTracer.GetOutputSoFar();
		}

		private Dictionary<string, string> SparkEnvironmentVariables()
		{
			return new Dictionary<string, string>()
				{
					{ "SPARK_HOME", HomeDirectory },
					{ "HADOOP_HOME", FakeHadoopHome },
					{ "JAVA_HOME", JavaHome },
					{ "HADOOP_CONF_DIR", ConfDirectory },
					{ "SPARK_JAVA_OPTS", String.Format("\"-Dhadoop.home.dir={0}\" \"-Dlog4j.configuration=file:{1}\"",
						FakeHadoopHome.Replace('\\', '/'),
						Path.Combine(ConfDirectory, _childTaskLog4jPropertiesFileName).Replace('\\', '/')) },
				};
		}
	}
}
