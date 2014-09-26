using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Microsoft.Experimental.Azure.Storm
{
	/// <summary>
	/// Configuration for a Storm node.
	/// </summary>
	public sealed class StormConfig
	{
		private readonly int _maxNodeMemoryMb;
		private readonly string _nimbusHost;
		private readonly ImmutableList<string> _zooKeeperServers;
		private readonly int _zooKeeperPort;

		/// <summary>
		/// Creates a new configuration.
		/// </summary>
		/// <param name="nimbusHost">The host where Nimbus is running.</param>
		/// <param name="zooKeeperServers">The names of ZooKeeper hosts.</param>
		/// <param name="zooKeeperPort">The port ZooKeeper nodes are listening on.</param>
		/// <param name="maxNodeMemoryMb">Maximum amount of memory used by the Storm node.</param>
		public StormConfig(string nimbusHost,
				IEnumerable<string> zooKeeperServers, int zooKeeperPort = 2181,
				int maxNodeMemoryMb = 2048)
		{
			_nimbusHost = nimbusHost;
			_zooKeeperServers = zooKeeperServers.ToImmutableList();
			_zooKeeperPort = zooKeeperPort;
			_maxNodeMemoryMb = maxNodeMemoryMb;
		}

		/// <summary>
		/// The host where Nimbus is running.
		/// </summary>
		public string NimbusHost { get { return _nimbusHost; } }

		/// <summary>
		/// The names of the ZooKeeper hosts.
		/// </summary>
		public ImmutableList<string> ZooKeeperServers { get { return _zooKeeperServers; } }

		/// <summary>
		/// The port ZooKeeper nodes are listening on.
		/// </summary>
		public int ZooKeeperPort { get { return _zooKeeperPort; } }

		/// <summary>
		/// Maximum amount of memory used by the Storm node.
		/// </summary>
		public int MaxNodeMemoryMb { get { return _maxNodeMemoryMb; } }

		/// <summary>
		/// Write out this configuration to a YAML file for Storm.
		/// </summary>
		/// <param name="writer">The writer for the file.</param>
		public void WriteToYamlFile(TextWriter writer)
		{
			var serializer = new Serializer();
			serializer.Serialize(writer, new Dictionary<string, object>()
			{
				{ "storm.zookeeper.servers", _zooKeeperServers.ToArray() },
				{ "nimbus.host", _nimbusHost },
			});
		}
	}
}
