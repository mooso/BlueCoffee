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

		public CassandraConfig(string clusterName,
			IEnumerable<string> clusterNodes,
			IEnumerable<string> dataDirectories,
			string commitLogDirectory, string savedCachesDirectory,
			int storagePort = 7000,
			int rpcPort = 9160)
		{
			_clusterName = clusterName;
			_clusterNodes = clusterNodes.ToImmutableList();
			_dataDirectories = dataDirectories.ToImmutableList();
			_commitLogDirectory = commitLogDirectory;
			_savedCachesDirectory = savedCachesDirectory;
			_storagePort = storagePort;
			_rpcPort = rpcPort;
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
