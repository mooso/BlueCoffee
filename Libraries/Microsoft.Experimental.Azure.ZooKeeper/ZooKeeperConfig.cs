using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ZooKeeper
{
	public sealed class ZooKeeperConfig
	{
		public const int DefaultPort = 2181;
		private readonly string _snapshotDirectory;
		private readonly int _port;

		public ZooKeeperConfig(string snapshotDirectory, int port = DefaultPort)
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
	}
}
