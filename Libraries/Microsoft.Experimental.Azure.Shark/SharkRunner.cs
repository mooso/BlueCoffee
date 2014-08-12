using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Shark
{
	/// <summary>
	/// Runs a Shark server.
	/// </summary>
	public sealed class SharkRunner
	{
		private readonly string _jarsDirectory;
		private readonly string _confDirectory;
		private readonly string _logsDirectory;
		private readonly string _javaHome;
		private readonly string _sharkHome;
		private readonly string _fakeHadoopHome;
		private const string _log4jPropertiesFileName = "log4j.properties";
		private readonly SharkConfig _config;
		private readonly Log4jTraceLevel _traceLevel;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="sharkHome">The directory to use for Shark home.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="traceLevel">The trace level to use.</param>
		public SharkRunner(string sharkHome, string javaHome, SharkConfig config,
			Log4jTraceLevel traceLevel = Log4jTraceLevel.INFO)
		{
			_sharkHome = sharkHome;
			_javaHome = javaHome;
			_config = config;
			_traceLevel = traceLevel;
			_jarsDirectory = Path.Combine(_sharkHome, "lib");
			_confDirectory = Path.Combine(_sharkHome, "conf");
			_logsDirectory = Path.Combine(_sharkHome, "logs");
			_fakeHadoopHome = Path.Combine(_sharkHome, "hadoop");
		}

		/// <summary>
		/// Setup Elastic Search.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in
				new[] { _jarsDirectory, _confDirectory, _fakeHadoopHome, _logsDirectory })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			_config.GetHiveConfigXml().Save(Path.Combine(_confDirectory, "hive-site.xml"));
			CreateLog4jConfig().ToPropertiesFile().WriteToFile(Path.Combine(_confDirectory, _log4jPropertiesFileName));
		}

		/// <summary>
		/// Run Shark server 2.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		/// <param name="monitor">Optional process monitor.</param>
		public void RunSharkServer2(bool runContinuous = true, ProcessMonitor monitor = null)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "shark.SharkServer2";
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
				},
				defines: new Dictionary<string, string>
				{
					{
						"log4j.configuration",
						"file:\"" +
							Path.Combine(_confDirectory, _log4jPropertiesFileName) +
							"\""
					},
					{ "hadoop.home.dir", _fakeHadoopHome },
				},
				runContinuous: runContinuous,
				monitor: monitor,
				environmentVariables: new Dictionary<string, string>()
				{
					{ "HIVE_SERVER2_THRIFT_PORT", _config.ServerPort.ToString() },
					{ "SPARK_HOME", _config.SparkHome },
					{ "MASTER", _config.SparkMaster },
				});
		}

		/// <summary>
		/// Runs beeline as a console application.
		/// </summary>
		/// <returns>The beeline process.</returns>
		public Process RunBeeline(string serverAddress = "localhost")
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "org.apache.hive.beeline.BeeLine";
			return runner.RunClassAsConsole(className,
				String.Format("-u jdbc:hive2://{0}:{1}", serverAddress, _config.ServerPort),
				ClassPath(),
				maxMemoryMb: _config.MaxMemoryMb,
				defines: new Dictionary<string, string>
				{
				},
				environmentVariables: new Dictionary<string, string>()
				{
				});
		}

		private IEnumerable<string> ClassPath()
		{
			return new[] { _confDirectory }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory))
				.Concat(Directory.EnumerateFiles(Path.Combine(_config.SparkHome, "lib")).Where(p => Path.GetFileName(p).StartsWith("spark")));
		}

		private Log4jConfig CreateLog4jConfig()
		{
			var layout = LayoutDefinition.PatternLayout("[%d{ISO8601}][%-5p][%-25c] %m%n");

			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender("console",
				layout: layout);
			var fileAppender = AppenderDefinitionFactory.DailyRollingFileAppender("file",
				Path.Combine(_logsDirectory, "SharkLog.log"),
				layout: layout);

			var rootLogger = new RootLoggerDefinition(_traceLevel, consoleAppender, fileAppender);

			return new Log4jConfig(rootLogger, Enumerable.Empty<ChildLoggerDefinition>());
		}

		private static void ExtractResourceArchive(string resourceName, string targetDirectory)
		{
			using (var rawStream = typeof(SharkRunner).Assembly.GetManifestResourceStream(
				"Microsoft.Experimental.Azure.Shark.Resources." + resourceName + ".zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				foreach (var entry in archive.Entries)
				{
					var targetFile = Path.Combine(targetDirectory, entry.FullName);
					if (!File.Exists(targetFile))
					{
						entry.ExtractToFile(targetFile);
					}
				}
			}
		}

		private void ExtractJars()
		{
			ExtractResourceArchive("Jars", _jarsDirectory);
			var binDirectory = Directory.CreateDirectory(Path.Combine(_fakeHadoopHome, "bin"));
			const string winutils = "winutils.exe";
			File.Move(Path.Combine(_jarsDirectory, winutils), Path.Combine(binDirectory.FullName, winutils));
		}
	}
}
