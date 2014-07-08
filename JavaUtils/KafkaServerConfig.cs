using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public sealed class KafkaServerConfig : PropertiesFileConfig
	{
		public const int DefaultPort = 9092;

		private KafkaServerConfig(IDictionary<string, string> configEntries)
			: base(configEntries)
		{
		}

		public static KafkaServerConfig Default(int brokerId, string logFileDirectory, string zooKeeperConnectionString)
		{
			return new KafkaServerConfig(new Dictionary<string, string>()
			{
				{ "broker.id", brokerId.ToString() },
				{ "port", DefaultPort.ToString() },
				{ "log.dirs", logFileDirectory },
				{ "zookeeper.connect", zooKeeperConnectionString },
			});
		}

		public KafkaServerConfig WithConfig(string configName, string value)
		{
			return new KafkaServerConfig(ConfigEntries.SetItem(configName, value));
		}
	}
}
