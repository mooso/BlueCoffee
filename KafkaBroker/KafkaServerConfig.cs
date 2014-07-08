using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaBroker
{
	sealed class KafkaServerConfig
	{
		private readonly ImmutableDictionary<string, string> _configEntries;
		public const int DefaultPort = 9092;

		private KafkaServerConfig(IDictionary<string, string> configEntires)
		{
			_configEntries = configEntires.ToImmutableDictionary();
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
			return new KafkaServerConfig(_configEntries.SetItem(configName, value));
		}

		public void WriteToFile(string configFilePath)
		{
			File.WriteAllText(configFilePath,
				String.Join("\n", _configEntries.Select((k, v) => String.Format(CultureInfo.InvariantCulture, "{0}={1}", k, v))),
				Encoding.ASCII);
		}
	}
}
