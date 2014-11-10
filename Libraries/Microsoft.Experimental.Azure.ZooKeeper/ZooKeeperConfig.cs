using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ZooKeeper
{
	/// <summary>
	/// ZooKeeper configuration.
	/// </summary>
	public sealed class ZooKeeperConfig
	{
		/// <summary>
		/// The default TCP port used for communicating with ZooKeeper.
		/// </summary>
		public const int DefaultPort = 2181;
		private readonly string _snapshotDirectory;
		private readonly int _port;

		/// <summary>
		/// Creates the configuration.
		/// </summary>
		/// <param name="snapshotDirectory">Directory for snapshots.</param>
		/// <param name="port">The TCP port to use for communicating with the server.</param>
		public ZooKeeperConfig(string snapshotDirectory, int port = DefaultPort)
		{
			_snapshotDirectory = snapshotDirectory;
			_port = port;
		}

		/// <summary>
		/// Directory for snapshots (data directory).
		/// </summary>
		public string SnapshotDirectory { get { return _snapshotDirectory; } }

		/// <summary>
		/// The TCP port to use for communicating with the server.
		/// </summary>
		public int Port { get { return _port; } }

		/// <summary>
		/// Represent this configuration as a properties file.
		/// </summary>
		/// <returns>The properties file to use.</returns>
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
