using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
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
		private readonly string _binDirectory;
		private readonly string _hadoopConfDirectory;
		private readonly string _logsDirectory;
		private readonly string _javaHome;
		private readonly string _sparkHome;
		private readonly string _fakeHadoopHome;
		private readonly Log4jTraceLevel _traceLevel;
		private const string _log4jPropertiesFileName = "log4j.properties";
		private readonly SparkConfig _config;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="sparkHome">The directory to use for Spark home.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="traceLevel">The trace level to use.</param>
		public SparkRunner(string sparkHome, string javaHome, SparkConfig config,
			Log4jTraceLevel traceLevel = Log4jTraceLevel.INFO)
		{
			_sparkHome = sparkHome;
			_javaHome = javaHome;
			_config = config;
			_traceLevel = traceLevel;
			_jarsDirectory = Path.Combine(_sparkHome, "lib");
			_binDirectory = Path.Combine(_sparkHome, "bin");
			_logsDirectory = Path.Combine(_sparkHome, "logs");
			_fakeHadoopHome = Path.Combine(_sparkHome, "hadoop");
			_hadoopConfDirectory = Path.Combine(_fakeHadoopHome, "conf");
		}

		/// <summary>
		/// Setup Elastic Search.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in
				new[] { _jarsDirectory, _binDirectory, _hadoopConfDirectory, _logsDirectory })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			_config.WriteHadoopCoreSiteXml(_hadoopConfDirectory);
			CreateLog4jConfig().ToPropertiesFile().WriteToFile(Path.Combine(_hadoopConfDirectory, _log4jPropertiesFileName));
		}

		/// <summary>
		/// Run Spark Master.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		/// <param name="monitor">Optional process monitor.</param>
		public void RunMaster(bool runContinuous = true, ProcessMonitor monitor = null)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "org.apache.spark.deploy.master.Master";
			runner.RunClass(className,
				String.Format("--ip {0} --port {1} --webui-port {2}",
					_config.MasterAddress, _config.MasterPort, _config.MasterWebUIPort),
				ClassPath(),
				extraJavaOptions: new[]
				{
					"-XX:+UseParNewGC",
					"-XX:+UseConcMarkSweepGC",
					"-XX:CMSInitiatingOccupancyFraction=75",
					"-XX:+UseCMSInitiatingOccupancyOnly",
				},
				defines: SparkDefines(),
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
			var runner = new JavaRunner(_javaHome);
			const string className = "org.apache.spark.deploy.worker.Worker";
			runner.RunClass(className,
				_config.SparkMasterUri,
				ClassPath(),
				extraJavaOptions: new[]
				{
					"-XX:+UseParNewGC",
					"-XX:+UseConcMarkSweepGC",
					"-XX:CMSInitiatingOccupancyFraction=75",
					"-XX:+UseCMSInitiatingOccupancyOnly",
				},
				defines: SparkDefines(),
				runContinuous: runContinuous,
				monitor: monitor,
				environmentVariables: SparkEnvironmentVariables());
		}

		/// <summary>
		/// Runs a Spark example.
		/// </summary>
		/// <param name="exampleName">The name of the example (e.g. SparkPi).</param>
		/// <param name="exampleArgs">Arguments given to the example.</param>
		/// <returns>The stdout/stderr from running the example.</returns>
		public ProcessOutput RunExample(string exampleName, string exampleArgs)
		{
			var runner = new JavaRunner(_javaHome);
			const string submitterClassName = "org.apache.spark.deploy.SparkSubmit";
			var exampleClassName = "org.apache.spark.examples." + exampleName;
			const string exampleJarName = "spark-examples-1.0.1-hadoop2.2.0.jar";
			var exampleJarPath = Path.Combine(_jarsDirectory, exampleJarName);
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

		private Log4jConfig CreateLog4jConfig()
		{
			var layout = LayoutDefinition.PatternLayout("[%d{ISO8601}][%-5p][%-25c] %m%n");

			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender("console",
				layout: layout);
			var fileAppender = AppenderDefinitionFactory.DailyRollingFileAppender("file",
				Path.Combine(_logsDirectory, "SparkLog.log"),
				layout: layout);

			var rootLogger = new RootLoggerDefinition(_traceLevel, consoleAppender, fileAppender);

			return new Log4jConfig(rootLogger, Enumerable.Empty<ChildLoggerDefinition>());
		}

		private Dictionary<string, string> SparkDefines()
		{
			return new Dictionary<string, string>
				{
					{
						"log4j.configuration",
						"file:\"" +
							Path.Combine(_hadoopConfDirectory, _log4jPropertiesFileName) +
							"\""
					},
				};
		}

		private IEnumerable<string> ClassPath()
		{
			return new[] { _hadoopConfDirectory }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory));
		}

		private Dictionary<string, string> SparkEnvironmentVariables()
		{
			return new Dictionary<string, string>()
				{
					{ "SPARK_HOME", _sparkHome },
					{ "HADOOP_HOME", _fakeHadoopHome },
					{ "JAVA_HOME", _javaHome },
					{ "HADOOP_CONF_DIR", _hadoopConfDirectory },
					{ "SPARK_JAVA_OPTS", String.Format("\"-Dhadoop.home.dir={0}\"", _fakeHadoopHome.Replace('\\', '/')) },
				};
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
			const string classPathScriptName = "compute-classpath.cmd";
			File.Move(
				Path.Combine(_jarsDirectory, classPathScriptName),
				Path.Combine(_binDirectory, classPathScriptName));
			File.WriteAllBytes(Path.Combine(_sparkHome, "RELEASE"), new byte[] { });
			var binDirectory = Directory.CreateDirectory(Path.Combine(_fakeHadoopHome, "bin"));
			const string winutils = "winutils.exe";
			File.Move(Path.Combine(_jarsDirectory, winutils), Path.Combine(binDirectory.FullName, winutils));
		}
	}
}
