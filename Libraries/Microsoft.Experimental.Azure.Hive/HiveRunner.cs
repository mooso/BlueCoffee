using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// Runs Hive metastore.
	/// </summary>
	public sealed class HiveRunner
	{
		private readonly string _jarsDirectory;
		private readonly string _javaHome;
		private readonly string _logsDirectory;
		private readonly string _configDirectory;
		private readonly string _log4jPropertiesPath;
		private readonly string _configFilePath;
		private readonly HiveConfig _config;
		private readonly string _fakeHadoopHome;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="jarsDirectory">The directory to use for jar files.</param>
		/// <param name="javaHome">The directory where Java is isntalled.</param>
		/// <param name="logsDirctory">The directory to use for logs.</param>
		/// <param name="configDirectory">The directory to use for configuration.</param>
		/// <param name="config">The hive configuration.</param>
		public HiveRunner(string jarsDirectory, string javaHome, string logsDirctory,
			HiveConfig config, string configDirectory)
		{
			_jarsDirectory = jarsDirectory;
			_javaHome = javaHome;
			_logsDirectory = logsDirctory;
			_configDirectory = configDirectory;
			_config = config;
			_log4jPropertiesPath = Path.Combine(_configDirectory, "hive-log4j.properties");
			_configFilePath = Path.Combine(_configDirectory, "hive-site.xml");
			_fakeHadoopHome = Path.Combine(_jarsDirectory, "hadoop");
		}

		/// <summary>
		/// Setup Elastic Search.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in
				new[] { _jarsDirectory, _logsDirectory, _configDirectory })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			WriteHiveConfigFile();
			WriteHiveLog4jFile();
		}

		/// <summary>
		/// Run Hive metastore.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the metastore whenver it exits and will never return.</param>
		public void RunMetastore(bool runContinuous = true)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "org.apache.hadoop.hive.metastore.HiveMetaStore";
			var classPathEntries = new[] { _configDirectory }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory));
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
						{ "log4j.configuration", "file:\"" + _log4jPropertiesPath + "\"" },
						{ "hadoop.home.dir", _fakeHadoopHome },
				},
				runContinuous: runContinuous);
		}

		private void WriteHiveConfigFile()
		{
			_config.ToXml().Save(_configFilePath);
		}

		private void WriteHiveLog4jFile()
		{
			var config = HiveLog4jConfigFactory.CreateConfig(_logsDirectory);
			config.ToPropertiesFile().WriteToFile(_log4jPropertiesPath);
		}

		private void ExtractJars()
		{
			using (var rawStream = GetType().Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.Hive.Resources.Jars.zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(_jarsDirectory);
			}
			var binDirectory = Directory.CreateDirectory(Path.Combine(_fakeHadoopHome, "bin"));
			const string winutils = "winutils.exe";
			File.Copy(Path.Combine(_jarsDirectory, winutils), Path.Combine(binDirectory.FullName, winutils));
		}
	}
}
