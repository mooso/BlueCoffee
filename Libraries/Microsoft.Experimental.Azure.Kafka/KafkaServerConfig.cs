using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Kafka
{
	/// <summary>
	/// Kafka broker configuration.
	/// </summary>
	public sealed class KafkaServerConfig
	{
		/// <summary>
		/// The default TCP port exposed for the broker.
		/// </summary>
		public const int DefaultPort = 9092;
		private readonly int _port;
		private readonly int _brokerId;
		private readonly string _logFileDirectory;
		private readonly string _zooKeeperConnectionString;

		/// <summary>
		/// Create the configuration.
		/// </summary>
		/// <param name="brokerId">The unique broker ID.</param>
		/// <param name="logFileDirectory">The directory to use for logs.</param>
		/// <param name="zooKeeperConnectionString">The connection string for the ZooKeeper cluster.</param>
		/// <param name="port">The port to expose for the broker.</param>
		public KafkaServerConfig(int brokerId, string logFileDirectory, string zooKeeperConnectionString, int port = DefaultPort)
		{
			_port = port;
			_brokerId = brokerId;
			_logFileDirectory = logFileDirectory;
			_zooKeeperConnectionString = zooKeeperConnectionString;
		}

		/// <summary>
		/// Represent this configuration as a properties file.
		/// </summary>
		/// <returns>The properties file to use.</returns>
		public PropertiesFile ToPropertiesFile()
		{
			return new PropertiesFile(new Dictionary<string, string>()
			{
				{ "broker.id", _brokerId.ToString() },
				{ "port", _port.ToString() },
				{ "log.dirs", _logFileDirectory.Replace('\\', '/') },
				{ "zookeeper.connect", _zooKeeperConnectionString },
			});
		}
	}
}
