using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Storm
{
	/// <summary>
	/// Runs Storm.
	/// </summary>
	public sealed class StormRunner
	{
		private readonly string _resourceFileDirectory;
		private readonly string _jarsDirectory;
		private readonly string _javaHome;
		private readonly string _logsDirectory;
		private readonly string _configDirectory;
		private readonly StormConfig _config;
		private readonly string _configFilePath;
		private readonly Log4jTraceLevel _traceLevel;
		private readonly string _loggingPropertiesFilePath;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="resourceFileDirectory">The directory that contains my resource files.</param>
		/// <param name="stormHomeDirectory">The directory to use as the storm home directory.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		/// <param name="logsDirectory">The directory to store logs.</param>
		/// <param name="config">The Storm configuration to use.</param>
		/// <param name="traceLevel">The trace level.</param>
		public StormRunner(string resourceFileDirectory, string stormHomeDirectory, string javaHome,
				string logsDirectory, StormConfig config, Log4jTraceLevel traceLevel = Log4jTraceLevel.INFO)
		{
			_resourceFileDirectory = resourceFileDirectory;
			_jarsDirectory = Path.Combine(stormHomeDirectory, "lib");
			_javaHome = javaHome;
			_logsDirectory = logsDirectory;
			_config = config;
			_configDirectory = Path.Combine(stormHomeDirectory, "conf");
			_configFilePath = Path.Combine(_configDirectory, "storm.yaml");
			_loggingPropertiesFilePath = Path.Combine(_configDirectory, "logging.properties");
			_traceLevel = traceLevel;
		}

		/// <summary>
		/// Setup Storm.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in
				new[] { _jarsDirectory, _logsDirectory, _configDirectory })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			WriteStormConfigFiles();
		}

		/// <summary>
		/// Run Storm Nimbus.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		public void RunNimbus(bool runContinuous = true)
		{
			const string className = "backtype.storm.daemon.nimbus";
			RunClass(runContinuous, className);
		}

		/// <summary>
		/// Run Storm Supervisor.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		public void RunSupervisor(bool runContinuous = true)
		{
			const string className = "backtype.storm.daemon.supervisor";
			RunClass(runContinuous, className);
		}

		private void RunClass(bool runContinuous, string className)
		{
			var runner = new JavaRunner(_javaHome);
			var classPathEntries = new[] { _configDirectory }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory));
			runner.RunClass(className,
					"",
					classPathEntries,
					maxMemoryMb: _config.MaxNodeMemoryMb,
					extraJavaOptions: new[]
					{
						"-XX:+UseParNewGC",
						"-XX:+UseConcMarkSweepGC",
						"-XX:CMSInitiatingOccupancyFraction=75",
						"-XX:+UseCMSInitiatingOccupancyOnly",
					},
					defines: new Dictionary<string, string>
					{
						{
							"log4j.configuration",
							"file:\"" + _loggingPropertiesFilePath + "\""
						},
						{ "storm.conf.file", Path.GetFileName(_configFilePath) },
					},
					runContinuous: runContinuous);
		}

		private void WriteStormConfigFiles()
		{
			using (var writer = File.CreateText(_configFilePath))
			{
				_config.WriteToYamlFile(writer);
			}
			CreateLog4jConfig().ToPropertiesFile().WriteToFile(_loggingPropertiesFilePath);
		}

		private Log4jConfig CreateLog4jConfig()
		{
			var layout = LayoutDefinition.PatternLayout("[%d{ISO8601}][%-5p][%-25c] %m%n");

			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender("console",
					layout: layout);
			var fileAppender = AppenderDefinitionFactory.DailyRollingFileAppender("file",
					Path.Combine(_logsDirectory, "storm.log"),
					layout: layout);

			var rootLogger = new RootLoggerDefinition(_traceLevel, consoleAppender, fileAppender);

			return new Log4jConfig(rootLogger, new ChildLoggerDefinition[] { });
		}

		private void ExtractResourceArchive(string resourceName, string targetDirectory)
		{
			using (var rawStream = File.OpenRead(Path.Combine(_resourceFileDirectory, resourceName + ".zip")))
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
