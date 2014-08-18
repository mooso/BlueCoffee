using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Cassandra
{
	/// <summary>
	/// A runner class that can run a Cassandra node in Azure (or any machine).
	/// </summary>
	public class CassandraNodeRunner
	{
		private readonly string _resourceFileDirectory;
		private readonly string _jarsDirectory;
		private readonly string _javaHome;
		private readonly string _logsDirectory;
		private readonly string _configDirectory;
		private readonly CassandraConfig _config;
		private readonly string _log4jPropertiesPath;
		private readonly string _configFilePath;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="resourceFileDirectory">The directory that contains my resource files.</param>
		/// <param name="jarsDirectory">The directory to use for jar files.</param>
		/// <param name="javaHome">The directory where Java is isntalled.</param>
		/// <param name="logsDirctory">The directory to use for logs.</param>
		/// <param name="configDirectory">The directory to use for configuration.</param>
		/// <param name="config">The Cassandra configuration to use.</param>
		public CassandraNodeRunner(string resourceFileDirectory, string jarsDirectory, string javaHome, string logsDirctory,
			string configDirectory, CassandraConfig config)
		{
			_resourceFileDirectory = resourceFileDirectory;
			_jarsDirectory = jarsDirectory;
			_javaHome = javaHome;
			_logsDirectory = logsDirctory;
			_config = config;
			_configDirectory = configDirectory;
			_log4jPropertiesPath = Path.Combine(_configDirectory, "log4j.properties");
			_configFilePath = Path.Combine(_configDirectory, "cassandra.yaml");
		}

		/// <summary>
		/// Setup Cassandra.
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
			WriteCassandraConfigFile();
			WriteCassandraLog4jFile();
		}

		/// <summary>
		/// Run Cassandra.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		public void Run(bool runContinuous = true)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "org.apache.cassandra.service.CassandraDaemon";
			var classPathEntries = new[] { _configDirectory }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory));
			runner.RunClass(className,
				_configFilePath,
				classPathEntries,
				extraJavaOptions: new[]
				{
					"-javaagent:\"" + Path.Combine(_jarsDirectory, "jamm-0.2.5.jar") + "\"",
				},
				defines: AddRingDelayOptionIfNeeded(new Dictionary<string, string>
				{
					{ "log4j.configuration", "file:\"" + _log4jPropertiesPath + "\"" },
				}),
				runContinuous: runContinuous);
		}

		private Dictionary<string, string> AddRingDelayOptionIfNeeded(Dictionary<string, string> defines)
		{
			if (_config.RingDelay.HasValue)
			{
				defines.Add("cassandra.ring_delay_ms",
					_config.RingDelay.Value.TotalMilliseconds.ToString("F0"));
			}
			return defines;
		}

		private void WriteCassandraConfigFile()
		{
			_config.WriteToYamlFile(_configFilePath);
		}

		private void WriteCassandraLog4jFile()
		{
			var config = CassandraLog4jConfigFactory.CreateConfig(_logsDirectory);
			config.ToPropertiesFile().WriteToFile(_log4jPropertiesPath);
		}

		private void ExtractJars()
		{
			using (var rawStream = File.OpenRead(Path.Combine(_resourceFileDirectory, "Jars.zip")))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(_jarsDirectory);
			}
		}
	}
}
