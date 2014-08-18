using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ElasticSearch
{
	/// <summary>
	/// A runner class that can run an Elastic Search node in Azure (or any machine).
	/// </summary>
	public class ESNodeRunner
	{
		private readonly string _resourceFileDirectory;
		private readonly string _jarsDirectory;
		private readonly string _javaHome;
		private readonly string _logsDirectory;
		private readonly string _homeDirectory;
		private readonly string _configDirectory;
		private readonly ESConfig _config;
		private readonly string _log4jPropertiesPath;
		private readonly string _configFilePath;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="resourceFileDirectory">The directory that contains my resource files.</param>
		/// <param name="jarsDirectory">The directory to use for jar files.</param>
		/// <param name="javaHome">The directory where Java is isntalled.</param>
		/// <param name="logsDirctory">The directory to use for logs.</param>
		/// <param name="homeDirectory">The directory to use as home.</param>
		/// <param name="config">The Elastic Search configuration to use.</param>
		/// <param name="configDirectory">The directory to use for configuration. If null we'll create one under <paramref name="homeDirectory"/>.</param>
		public ESNodeRunner(string resourceFileDirectory, string jarsDirectory, string javaHome, string logsDirctory,
			string homeDirectory, ESConfig config, string configDirectory = null)
		{
			_resourceFileDirectory = resourceFileDirectory;
			_jarsDirectory = jarsDirectory;
			_javaHome = javaHome;
			_logsDirectory = logsDirctory;
			_config = config;
			_homeDirectory = homeDirectory;
			_configDirectory = configDirectory ?? Path.Combine(homeDirectory, "config");
			_log4jPropertiesPath = Path.Combine(_configDirectory, "logging.properties");
			_configFilePath = Path.Combine(_configDirectory, "elasticsearch.properties");
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
			WriteESConfigFile();
			WriteESLog4jFile();
		}

		/// <summary>
		/// Run Elastic Search.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		public void Run(bool runContinuous = true)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "org.elasticsearch.bootstrap.Elasticsearch";
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
					{ "elasticSearch", null },
					{ "es-foreground", "yes" },
					{ "es.path.home", _homeDirectory },
					{ "es.path.conf", _configDirectory },
				},
				runContinuous: runContinuous);
		}

		private void WriteESConfigFile()
		{
			_config.ToPropertiesFile().WriteToFile(_configFilePath);
		}

		private void WriteESLog4jFile()
		{
			var config = ESLog4jConfigFactory.CreateConfig(_logsDirectory);
			var originalPropertiesFile = config.ToPropertiesFile();
			var log4jPrefixRemoved = new PropertiesFile(originalPropertiesFile
				.ConfigEntries
				.Select(kv => new KeyValuePair<string, string>(kv.Key.Remove(0, "log4j.".Length), kv.Value)));
			log4jPrefixRemoved.WriteToFile(_log4jPropertiesPath);
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
