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
	/// <summary>
	/// Configuration for a Cassandra node.
	/// </summary>
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

		/// <summary>
		/// Create a new configuration.
		/// </summary>
		/// <param name="clusterName">Name of the cluster.</param>
		/// <param name="clusterNodes">List of cluster nodes (not necessarily exhaustive, see Cassandra help for seeds for details).</param>
		/// <param name="dataDirectories">List of directories to use for data files.</param>
		/// <param name="commitLogDirectory">Directory to use for commit logs.</param>
		/// <param name="savedCachesDirectory">Directory to use for saved caches.</param>
		/// <param name="ringDelay">The time to wait while trying to reach other nodes in the cluster before giving up (Cassandra defaults to 30 seconds).</param>
		/// <param name="storagePort">The TCP port to expose for storage service communication (mainly inter-node communication).</param>
		/// <param name="rpcPort">The TCP port to expose for RPC.</param>
		/// <param name="nativeTransportPort">The TCP port to expose for native transport.</param>
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
		
		/// <summary>
		/// All the directories specified in this configuration (data, commit logs, saved caches).
		/// </summary>
		public IEnumerable<string> AllDirectories
		{
			get
			{
				return new[] { _commitLogDirectory, _savedCachesDirectory }
					.Concat(_dataDirectories);
			}
		}

		/// <summary>
		/// Write out this configuration to a YAML file for Cassandra.
		/// </summary>
		/// <param name="filePath">The path of the file.</param>
		public void WriteToYamlFile(string filePath)
		{
			using (var writer = new StreamWriter(filePath, append: false, encoding: Encoding.ASCII))
			{
				WriteToYamlFile(writer);
			}
		}

		/// <summary>
		/// Write out this configuration to a YAML file for Cassandra.
		/// </summary>
		/// <param name="writer">The writer for the file.</param>
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
				listen_address = _clusterNodes.Count == 1 ? "localhost" : null,
			});
		}
	}
}
