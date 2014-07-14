using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Microsoft.Experimental.Azure.Cassandra
{
	public sealed class CassandraConfig
	{
		private readonly string _clusterName;
		private readonly ImmutableList<string> _clusterNodes;
		private readonly ImmutableList<string> _dataDirectories;
		private readonly string _commitLogDirectory;
		private readonly string _savedCachesDirectory;
		private readonly int _storagePort;
		private readonly int _rpcPort;
		private readonly int? _nativeTransportPort;
		private readonly TimeSpan? _ringDelay;

		public CassandraConfig(string clusterName,
			IEnumerable<string> clusterNodes,
			IEnumerable<string> dataDirectories,
			string commitLogDirectory, string savedCachesDirectory,
			TimeSpan? ringDelay = null,
			int storagePort = 7000,
			int rpcPort = 9160,
			int? nativeTransportPort = 9042)
		{
			_clusterName = clusterName;
			_clusterNodes = clusterNodes.ToImmutableList();
			_dataDirectories = dataDirectories.ToImmutableList();
			_commitLogDirectory = commitLogDirectory;
			_savedCachesDirectory = savedCachesDirectory;
			_storagePort = storagePort;
			_rpcPort = rpcPort;
			_nativeTransportPort = nativeTransportPort;
			_ringDelay = ringDelay;
		}

		/// <summary>
		/// Delay after which Cassandra assumes the ring has stabilized (and will error out if it can't gossip with any of the nodes)
		/// Defaul is 30 seconds in Cassandra if not specified.
		/// </summary>
		public TimeSpan? RingDelay
		{
			get { return _ringDelay; }
		}

		public IEnumerable<string> AllDirectories
		{
			get
			{
				return new[] { _commitLogDirectory, _savedCachesDirectory }
					.Concat(_dataDirectories);
			}
		}

		public void WriteToYamlFile(string filePath)
		{
			using (var writer = new StreamWriter(filePath, append: false, encoding: Encoding.ASCII))
			{
				WriteToYamlFile(writer);
			}
		}

		public void WriteToYamlFile(TextWriter writer)
		{
			var serializer = new Serializer();
			serializer.Serialize(writer, new
			{
				cluster_name = _clusterName,
				seed_provider = new object[]
				{
					new Dictionary<string,object>()
					{
						{ "class_name", "org.apache.cassandra.locator.SimpleSeedProvider" },
						{ "parameters", new object[]
							{
								new Dictionary<string, string>()
								{
									{ "seeds",  String.Join(",", _clusterNodes) },
								}
							}
						},
					}
				},
				data_file_directories = _dataDirectories.Select(d => d.Replace('\\', '/')).ToList(),
				commitlog_directory = _commitLogDirectory.Replace('\\', '/'),
				saved_caches_directory = _savedCachesDirectory.Replace('\\', '/'),
				storage_port = _storagePort,
				rpc_port = _rpcPort,
				start_native_transport = _nativeTransportPort.HasValue,
				native_transport_port = _nativeTransportPort ?? 0,
				// Hard-coded for now, we can make configurable if needed
				commitlog_sync = "periodic",
				commitlog_sync_period_in_ms = 10000,
				partitioner = "org.apache.cassandra.dht.Murmur3Partitioner",
				endpoint_snitch = "SimpleSnitch",
				listen_address = "localhost",
			});
		}
	}
}
