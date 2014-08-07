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
		private readonly string _fakeHadoopHome;
		private const string _log4jPropertiesFileName = "log4j.properties";
		private const string _configFileName = "hive-site.xml";

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="jarsDirectory">The directory to use for jar files.</param>
		/// <param name="javaHome">The directory where Java is isntalled.</param>
		/// <param name="logsDirctory">The directory to use for logs.</param>
		/// <param name="configDirectory">The directory to use for configuration.</param>
		public HiveRunner(string jarsDirectory, string javaHome, string logsDirctory,
			string configDirectory)
		{
			_jarsDirectory = jarsDirectory;
			_javaHome = javaHome;
			_logsDirectory = logsDirctory;
			_configDirectory = configDirectory;
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
		}

		/// <summary>
		/// Run Hive metastore.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <param name="runContinuous">If set, this method will keep restarting the metastore whenver it exits and will never return.</param>
		/// <param name="monitor">Optional process monitor.</param>
		public void RunMetastore(HiveMetastoreConfig config, bool runContinuous = true, ProcessMonitor monitor = null)
		{
			const string className = "org.apache.hadoop.hive.metastore.HiveMetaStore";
			const string fileName = "hive-metastore";
			WriteHiveConfigFile(fileName, config);
			WriteHiveLog4jFile(fileName);
			RunClass(
				runContinuous: runContinuous,
				className: className,
				arguments: "-p " + config.Port,
				configSubdirectory: fileName,
				monitor: monitor);
		}

		/// <summary>
		/// Runs Hive server 2.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <param name="runContinuous">If set, this method will keep restarting the metastore whenver it exits and will never return.</param>
		/// <param name="monitor">Optional process monitor.</param>
		public void RunHiveServer(HiveServerConfig config, bool runContinuous = true, ProcessMonitor monitor = null)
		{
			const string className = "org.apache.hive.service.server.HiveServer2";
			const string fileName = "hive-server2";
			WriteHiveConfigFile(fileName, config);
			WriteHiveLog4jFile(fileName);
			RunClass(
				runContinuous: runContinuous,
				className: className,
				arguments: "",
				configSubdirectory: fileName,
				monitor: monitor);
		}

		private void RunClass(bool runContinuous, string className, string arguments, string configSubdirectory, ProcessMonitor monitor)
		{
			var runner = new JavaRunner(_javaHome);
			var classPathEntries = new[] { Path.Combine(_configDirectory, configSubdirectory) }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory));
			runner.RunClass(className,
				arguments,
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
						{
							"log4j.configuration",
							"file:\"" +
								Path.Combine(_configDirectory, configSubdirectory, _log4jPropertiesFileName) +
								"\""
						},
						{ "hadoop.home.dir", _fakeHadoopHome },
				},
				runContinuous: runContinuous,
				monitor: monitor);
		}

		private void WriteHiveConfigFile(string configSubdirectory, HadoopStyleXmlConfig config)
		{
			var directory = EnsureConfigSubdirectoryExists(configSubdirectory);
			config.ToXml().Save(Path.Combine(directory, _configFileName));
		}

		private string EnsureConfigSubdirectoryExists(string configSubdirectory)
		{
			var directory = Path.Combine(_configDirectory, configSubdirectory);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			return directory;
		}

		private void WriteHiveLog4jFile(string configSubdirectory)
		{
			var config = HiveLog4jConfigFactory.CreateConfig(Path.Combine(_logsDirectory, configSubdirectory + ".log"));
			var directory = EnsureConfigSubdirectoryExists(configSubdirectory);
			config.ToPropertiesFile().WriteToFile(Path.Combine(directory, _log4jPropertiesFileName));
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
