using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public sealed class ZookeeperConfig
	{
		public const int DefaultPort = 2181;
		private readonly string _snapshotDirectory;
		private readonly int _port;

		public ZookeeperConfig(string snapshotDirectory, int port = DefaultPort)
		{
			_snapshotDirectory = snapshotDirectory;
			_port = port;
		}

		public PropertiesFile ToPropertiesFile()
		{
			return new PropertiesFile(new Dictionary<string, string>()
			{
				{ "dataDir", _snapshotDirectory.Replace('\\', '/') },
				{ "clientPort", _port.ToString() },
			});
		}

		public static string GetZookeeperConnectionString(IEnumerable<string> zookeeperHosts, int port = DefaultPort)
		{
			return String.Join(",", zookeeperHosts.Select(h => String.Join(":", h, port)));
		}
	}
}
