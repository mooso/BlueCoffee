﻿using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ZooKeeper
{
	/// <summary>
	/// A runner class that can run ZooKeeper server in Azure (or any machine).
	/// </summary>
	public class ZooKeeperNodeRunner
	{
		private readonly string _javaHome;
		private readonly string _jarsDirectory;
		private readonly string _dataDirectory;
		private readonly string _configsDirectory;
		private readonly string _zooKeeperPropertiesPath;
		private readonly string _logsDirectory;
		private readonly string _zooKeeperLog4jPropertiesPath;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="dataDirectory">The directory to use for ZooKeeper data.</param>
		/// <param name="configsDirectory">The directory to use for ZooKeeper configuration.</param>
		/// <param name="logsDirectory">The directory to use for ZooKeeper logs.</param>
		/// <param name="jarsDirectory">The directory to use for jar files.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		public ZooKeeperNodeRunner(string dataDirectory, string configsDirectory, string logsDirectory, string jarsDirectory,
			string javaHome)
		{
			_dataDirectory = dataDirectory;
			_configsDirectory = configsDirectory;
			_logsDirectory = logsDirectory;
			_jarsDirectory = jarsDirectory;
			_javaHome = javaHome;
			_zooKeeperPropertiesPath = Path.Combine(_configsDirectory, "zookeeper.properties");
			_zooKeeperLog4jPropertiesPath = Path.Combine(_configsDirectory, "log4j.properties");
		}

		/// <summary>
		/// Run ZooKeeper.
		/// </summary>
		/// <remarks>
		/// This method never returns (just runs continuously or throws).
		/// </remarks>
		public void Run()
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "org.apache.zookeeper.server.quorum.QuorumPeerMain";
			var classPathEntries = JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory);
			runner.RunClass(className, _zooKeeperPropertiesPath, classPathEntries,
				defines: new Dictionary<string, string>
					{
						{ "log4j.configuration", "file:\"" + _zooKeeperLog4jPropertiesPath + "\"" }
					});
		}

		/// <summary>
		/// Setup ZooKeeper.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in new[] { _dataDirectory, _configsDirectory, _logsDirectory, _jarsDirectory })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			WriteZooKeeperServerConfigFile();
			WriteZooKeeperLog4jFile();
		}

		private void WriteZooKeeperServerConfigFile()
		{
			var config = new ZooKeeperConfig(_dataDirectory);
			config.ToPropertiesFile().WriteToFile(_zooKeeperPropertiesPath);
		}

		private void WriteZooKeeperLog4jFile()
		{
			var config = ZooKeeperLog4jConfigFactory.CreateConfig(_logsDirectory);
			config.ToPropertiesFile().WriteToFile(_zooKeeperLog4jPropertiesPath);
		}

		private void ExtractJars()
		{
			using (var rawStream = typeof(ZooKeeperNodeRunner).Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.ZooKeeper.Resources.Jars.zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(_jarsDirectory);
			}
		}
	}
}
