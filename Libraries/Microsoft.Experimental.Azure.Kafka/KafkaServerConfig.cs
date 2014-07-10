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
	public sealed class KafkaServerConfig
	{
		public const int DefaultPort = 9092;
		private readonly int _port;
		private readonly int _brokerId;
		private readonly string _logFileDirectory;
		private readonly string _zooKeeperConnectionString;

		public KafkaServerConfig(int brokerId, string logFileDirectory, string zooKeeperConnectionString, int port = DefaultPort)
		{
			_port = port;
			_brokerId = brokerId;
			_logFileDirectory = logFileDirectory;
			_zooKeeperConnectionString = zooKeeperConnectionString;
		}

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
