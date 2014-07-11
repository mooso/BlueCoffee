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
	public class CassandraNodeRunner
	{
		private readonly string _jarsDirectory;
		private readonly string _javaHome;
		private readonly string _logsDirectory;
		private readonly string _configDirectory;
		private readonly CassandraConfig _config;
		private readonly string _log4jPropertiesPath;
		private readonly string _configFilePath;

		public CassandraNodeRunner(string jarsDirectory, string javaHome, string logsDirctory,
			string configDirectory, CassandraConfig config)
		{
			_jarsDirectory = jarsDirectory;
			_javaHome = javaHome;
			_logsDirectory = logsDirctory;
			_config = config;
			_configDirectory = configDirectory;
			_log4jPropertiesPath = Path.Combine(_configDirectory, "log4j.properties");
			_configFilePath = Path.Combine(_configDirectory, "cassandra.yaml");
		}

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
				defines: new Dictionary<string, string>
				{
					{ "log4j.configuration", "file:\"" + _log4jPropertiesPath + "\"" },
				},
				runContinuous: runContinuous);
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
			using (var rawStream = GetType().Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.Cassandra.Resources.Jars.zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(_jarsDirectory);
			}
		}
	}
}
