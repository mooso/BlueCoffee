using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Logback;
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
		private readonly string _stormHomeDirectory;
		private readonly string _jarsDirectory;
		private readonly string _javaHome;
		private readonly string _logsDirectory;
		private readonly string _configDirectory;
		private readonly string _uiHtmlDirectory;
		private readonly StormConfig _config;
		private readonly string _configFilePath;
		private readonly LogbackTraceLevel _traceLevel;
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
				string logsDirectory, StormConfig config, LogbackTraceLevel traceLevel = LogbackTraceLevel.INFO)
		{
			_resourceFileDirectory = resourceFileDirectory;
			_stormHomeDirectory = stormHomeDirectory;
			_jarsDirectory = Path.Combine(stormHomeDirectory, "lib");
			_uiHtmlDirectory = Path.Combine(stormHomeDirectory, "public");
			_javaHome = javaHome;
			_logsDirectory = logsDirectory;
			_config = config;
			_configDirectory = Path.Combine(stormHomeDirectory, "conf");
			_configFilePath = Path.Combine(_configDirectory, "storm.yaml");
			_loggingPropertiesFilePath = Path.Combine(_configDirectory, "logging.xml");
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

		/// <summary>
		/// Run Storm UI.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		public void RunUI(bool runContinuous = true)
		{
			const string className = "backtype.storm.ui.core";
			RunClass(runContinuous, className);
		}

		/// <summary>
		/// Runs the given jar on Storm.
		/// </summary>
		/// <param name="className">The class name within the jar file to run.</param>
		/// <param name="jarPath">The fully qualified path to the jar file.</param>
		/// <param name="arguments">Optional arguments to the class.</param>
		/// <param name="runContinuous">If set, this method will keep restarting the class whenver it exits and will never return.</param>
		public void RunJar(string className, string jarPath, IEnumerable<string> arguments = null,
			bool runContinuous = true)
		{
			RunClass(runContinuous: runContinuous,
				className: className,
				extraDefines: new Dictionary<string, string>()
				{
					{ "storm.jar", jarPath },
				},
				extraClassPathEntries: new[] { jarPath },
				arguments: arguments);
		}

		private void RunClass(bool runContinuous, string className,
			IEnumerable<KeyValuePair<string, string>> extraDefines = null,
			IEnumerable<string> extraClassPathEntries = null,
			IEnumerable<string> arguments = null)
		{
			var runner = new JavaRunner(_javaHome);
			var classPathEntries = new[] { Path.Combine(_stormHomeDirectory), _configDirectory }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory))
				.Concat(extraClassPathEntries ?? Enumerable.Empty<string>());
			runner.RunClass(className,
					String.Join(" ", arguments ?? Enumerable.Empty<string>()),
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
							"logback.configurationFile",
							"\"" + _loggingPropertiesFilePath + "\""
						},
						{ "storm.conf.file", Path.GetFileName(_configFilePath) },
						{ "storm.home", _stormHomeDirectory.Replace('\\', '/') },
					}.Concat(extraDefines ?? Enumerable.Empty<KeyValuePair<string, string>>()),
					runContinuous: runContinuous,
					environmentVariables: new Dictionary<string, string>()
					{
						{ "JAVA_HOME", _javaHome },
					});
		}

		private void WriteStormConfigFiles()
		{
			using (var writer = File.CreateText(_configFilePath))
			{
				_config.WriteToYamlFile(writer);
			}
			CreateLogbackConfig().ToXDocument().Save(_loggingPropertiesFilePath);
		}

		private LogbackConfig CreateLogbackConfig()
		{
			var fileAppender = new RollingFileAppenderDefinition("main", Path.Combine(_logsDirectory, "Storm.log"));
			var consoleAppender = new ConsoleAppenderDefinition();
			return new LogbackConfig(new RootLoggerDefinition(_traceLevel, fileAppender, consoleAppender),
				new ChildLoggerDefinition[] {});
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
			ExtractResourceArchive("UI", _uiHtmlDirectory);
		}
	}
}
