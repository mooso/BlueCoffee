using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public sealed class ZookeeperConfig : PropertiesFileConfig
	{
		public const int DefaultPort = 2181;

		private ZookeeperConfig(IDictionary<string, string> configEntries)
			: base(configEntries)
		{
		}

		public static ZookeeperConfig Default(string snapshotDirectory)
		{
			return new ZookeeperConfig(new Dictionary<string, string>()
			{
				{ "dataDir", snapshotDirectory },
				{ "clientPort", DefaultPort.ToString() },
			});
		}

		public static string GetZookeeperConnectionString(IEnumerable<string> zookeeperHosts, int port = DefaultPort)
		{
			return String.Join(",", zookeeperHosts.Select(h => String.Join(":", h, port)));
		}

		public ZookeeperConfig WithConfig(string configName, string value)
		{
			return new ZookeeperConfig(ConfigEntries.SetItem(configName, value));
		}
	}
}
