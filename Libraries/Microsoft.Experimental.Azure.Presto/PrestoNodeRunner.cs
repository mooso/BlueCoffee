using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Presto
{
	/// <summary>
	/// Runs a Presto node.
	/// </summary>
	public sealed class PrestoNodeRunner
	{
		private readonly string _jarsDirectory;
		private readonly string _javaHome;
		private readonly string _logsDirectory;
		private readonly string _configDirectory;
		private readonly PrestoConfig _config;
		private readonly string _configFilePath;
		private readonly Log4jTraceLevel? _traceLevel;
		private readonly string _loggingPropertiesFilePath;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="jarsDirectory">The directory to use for jar files.</param>
		/// <param name="javaHome">The directory where Java is isntalled.</param>
		/// <param name="logsDirctory">The directory to use for logs.</param>
		/// <param name="config">The Presto configuration to use.</param>
		/// <param name="configDirectory">The directory to use for configuration.</param>
		/// <param name="traceLevel">The trace level.</param>
		public PrestoNodeRunner(string jarsDirectory, string javaHome, string logsDirctory,
			PrestoConfig config, string configDirectory,
			Log4jTraceLevel? traceLevel = null)
		{
			_jarsDirectory = jarsDirectory;
			_javaHome = javaHome;
			_logsDirectory = logsDirctory;
			_config = config;
			_configDirectory = configDirectory;
			_configFilePath = Path.Combine(_configDirectory, "config.properties");
			_loggingPropertiesFilePath = Path.Combine(_configDirectory, "logging.properties");
			_traceLevel = traceLevel;
		}

		/// <summary>
		/// Setup Elastic Search.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in
				new[] { _jarsDirectory, _logsDirectory, _configDirectory }
				.Concat(_config.AllDirectories))
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			WritePrestoConfigFiles();
		}

		/// <summary>
		/// Run Elastic Search.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		public void Run(bool runContinuous = true)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "com.facebook.presto.server.PrestoServer";
			var classPathEntries = JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory);
			runner.RunClass(className,
				"",
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
					{ "log.output-file", Path.Combine(_logsDirectory, "Presto.log").Replace('\\', '/') },
					{ "config", _configFilePath.Replace('\\', '/') },
				}
				.Concat(_config.GetNodeProperties())
				.Concat(GetLogFileProperties()),
				runContinuous: runContinuous);
		}

		private IEnumerable<KeyValuePair<string, string>> GetLogFileProperties()
		{
			return _traceLevel.HasValue ?
				new[]
				{
					new KeyValuePair<string, string>(
						"log.levels-file", _loggingPropertiesFilePath.Replace('\\', '/'))
				} :
				Enumerable.Empty<KeyValuePair<string, string>>();
		}

		private void WritePrestoConfigFiles()
		{
			_config.CreateConfigPropertiesFile().WriteToFile(_configFilePath);
			_config.WriteAllCatalogConfigFiles();
			if (_traceLevel.HasValue)
			{
				File.WriteAllText(_loggingPropertiesFilePath, "com.facebook.presto=" + _traceLevel.Value, Encoding.ASCII);
			}
		}

		private void ExtractResourceArchive(string resourceName, string targetDirectory)
		{
			using (var rawStream = GetType().Assembly.GetManifestResourceStream(
				"Microsoft.Experimental.Azure.Presto.Resources." + resourceName + ".zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(targetDirectory);
			}
		}

		private void ExtractJars()
		{
			ExtractResourceArchive("Jars", _jarsDirectory);
			ExtractResourceArchive("CassandraPlugin", Path.Combine(_config.PluginInstallDirectory, "cassandra"));
		}
	}
}
