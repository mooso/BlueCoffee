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

namespace Microsoft.Experimental.Azure.Kafka
{
	/// <summary>
	/// A runner class that can run an Apache Kafka broker on an Azure compute node (or any machine).
	/// </summary>
	public class KafkaBrokerRunner
	{
		private readonly string _dataDirectory;
		private readonly string _configsDirectory;
		private readonly string _logsDirectory;
		private readonly string _jarsDirectory;
		private readonly string _kafkaServerPropertiesPath;
		private readonly string _kafkaLog4jPropertiesPath;
		private readonly ImmutableList<string> _zooKeeperHosts;
		private readonly int _zooKeeperPort;
		private readonly int _brokerId;
		private readonly string _javaHome;

		/// <summary>
		/// Creates a new runner.
		/// </summary>
		/// <param name="dataDirectory">The directory to use for Kafka data.</param>
		/// <param name="configsDirectory">The directory to use for Kafka configuration.</param>
		/// <param name="logsDirectory">The directory to use for logs.</param>
		/// <param name="jarsDirectory">The directory to use for jar files.</param>
		/// <param name="zooKeeperHosts">A list of addresses of ZooKeeper cluster hosts.</param>
		/// <param name="zooKeeperPort">The TCP port to use to communicate to the ZooKeeper nodes.</param>
		/// <param name="brokerId">A unique ID of this broker in the cluster.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		public KafkaBrokerRunner(string dataDirectory, string configsDirectory, string logsDirectory, string jarsDirectory,
			IEnumerable<string> zooKeeperHosts, int zooKeeperPort, int brokerId, string javaHome)
		{
			_dataDirectory = dataDirectory;
			_configsDirectory = configsDirectory;
			_logsDirectory = logsDirectory;
			_jarsDirectory = jarsDirectory;
			_zooKeeperHosts = zooKeeperHosts.ToImmutableList();
			_zooKeeperPort = zooKeeperPort;
			_brokerId = brokerId;
			_javaHome = javaHome;
			_kafkaServerPropertiesPath = Path.Combine(_configsDirectory, "server.properties");
			_kafkaLog4jPropertiesPath = Path.Combine(_configsDirectory, "log4j.properties");
		}

		/// <summary>
		/// Setup Kafka.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in new[] { _dataDirectory, _configsDirectory, _logsDirectory, _jarsDirectory })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			WriteKafkaServerConfigFile();
			WriteKafkaLog4jFile();
		}

		/// <summary>
		/// Run the broker.
		/// </summary>
		/// <remarks>
		/// This method never returns (just runs continuously or throws).
		/// </remarks>
		public void Run()
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "kafka.Kafka";
			var classPathEntries = JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory);
			runner.RunClass(className,
				_kafkaServerPropertiesPath,
				classPathEntries,
				defines: new Dictionary<string, string>
					{
						{ "log4j.configuration", "file:\"" + _kafkaLog4jPropertiesPath + "\"" }
					});
		}

		private string GetZookeeperConnectionString()
		{
			return String.Join(",", _zooKeeperHosts.Select(h => String.Join(":", h, _zooKeeperPort)));
		}

		private void WriteKafkaServerConfigFile()
		{
			var zookeeperConnectionString = GetZookeeperConnectionString();
			Trace.TraceInformation("Zookeeper connection string: " + zookeeperConnectionString);
			var config = new KafkaServerConfig(_brokerId, _dataDirectory, zookeeperConnectionString);
			config.ToPropertiesFile().WriteToFile(_kafkaServerPropertiesPath);
		}

		private void WriteKafkaLog4jFile()
		{
			var config = KafkaLog4jConfigFactory.CreateConfig(_logsDirectory);
			config.ToPropertiesFile().WriteToFile(_kafkaLog4jPropertiesPath);
		}

		private void ExtractJars()
		{
			using (var rawStream = typeof(KafkaBrokerRunner).Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.Kafka.Resources.Jars.zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(_jarsDirectory);
			}
		}
	}
}
