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
		private readonly ImmutableList<string> _dataDirectories;
		private readonly string _commitLogDirectory;
		private readonly string _savedCachesDirectory;
		private readonly int _storagePort;
		private readonly int _rpcPort;

		public CassandraConfig(string clusterName, IEnumerable<string> dataDirectories,
			string commitLogDirectory, string savedCachesDirectory,
			int storagePort = 7000,
			int rpcPort = 9160)
		{
			_clusterName = clusterName;
			_dataDirectories = dataDirectories.ToImmutableList();
			_commitLogDirectory = commitLogDirectory;
			_savedCachesDirectory = savedCachesDirectory;
			_storagePort = storagePort;
			_rpcPort = rpcPort;
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
				data_file_directories = _dataDirectories.Select(d => d.Replace('\\', '/')).ToList(),
				commitlog_directory = _commitLogDirectory.Replace('\\', '/'),
				saved_caches_directory = _savedCachesDirectory.Replace('\\', '/'),
				storage_port = _storagePort,
				rpc_port = _rpcPort,
			});
		}
	}
}
