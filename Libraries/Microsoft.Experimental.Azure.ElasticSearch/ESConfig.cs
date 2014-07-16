using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ElasticSearch
{
	/// <summary>
	/// The configuration for the Elastic Search node.
	/// </summary>
	public sealed class ESConfig
	{
		private readonly string _clusterName;
		private readonly string _nodeName;
		private readonly bool? _canBeMasterNode;
		private readonly bool? _canBeDataNode;
		private readonly ImmutableList<string> _dataDirectories;
		private readonly int? _nodeCommunicationPort;
		private readonly int? _httpPort;
		private readonly int? _minimumMasterNodes;
		private readonly bool? _enableMulticastDiscovery;
		private readonly ImmutableList<string> _masterNodes;

		/// <summary>
		/// Creates a new config.
		/// </summary>
		/// <param name="clusterName">Cluster name identifies your cluster for auto-discovery.</param>
		/// <param name="nodeName">Node names are generated dynamically on startup, but you can tie this node to a specific name.</param>
		/// <param name="canBeMasterNode">Allow this node to be eligible as a master node (enabled by default).</param>
		/// <param name="canBeDataNode">Allow this node to store data (enabled by default).</param>
		/// <param name="dataDirectories">Paths to directories where to store index data allocated for this node.</param>
		/// <param name="nodeCommunicationPort">Set a custom port for the node to node communication (9300 by default).</param>
		/// <param name="httpPort">Set a custom port to listen for HTTP traffic.</param>
		/// <param name="minimumMasterNodes">Set to ensure a node sees N other master eligible nodes to be considered operational within the cluster.</param>
		/// <param name="enableMulticastDiscovery">Multicast discovery (enabled by default).</param>
		/// <param name="masterNodes">Initial list of master nodes in the cluster to perform discovery when new nodes (master or data) are started.</param>
		public ESConfig(string clusterName = null,
			string nodeName = null,
			bool? canBeMasterNode = null,
			bool? canBeDataNode = null,
			IEnumerable<string> dataDirectories = null,
			int? nodeCommunicationPort = null,
			int? httpPort = null,
			int? minimumMasterNodes = null,
			bool? enableMulticastDiscovery = null,
			IEnumerable<string> masterNodes = null
			)
		{
			_clusterName = clusterName;
			_nodeName = nodeName;
			_canBeMasterNode = canBeMasterNode;
			_canBeDataNode = canBeDataNode;
			_dataDirectories = (dataDirectories ?? Enumerable.Empty<string>()).ToImmutableList();
			_nodeCommunicationPort = nodeCommunicationPort;
			_httpPort = httpPort;
			_minimumMasterNodes = minimumMasterNodes;
			_enableMulticastDiscovery = enableMulticastDiscovery;
			_masterNodes = (masterNodes ?? Enumerable.Empty<string>()).ToImmutableList();
		}

		internal IEnumerable<string> AllDirectories
		{
			get
			{
				return _dataDirectories;
			}
		}

		/// <summary>
		/// Creates the properties file.
		/// </summary>
		/// <returns>The properties file.</returns>
		public PropertiesFile ToPropertiesFile()
		{
			return new PropertiesFile(new[]
				{
					ToKeyValue("cluster.name", _clusterName),
					ToKeyValue("node.name", _nodeName),
					ToKeyValue("node.master", _canBeMasterNode),
					ToKeyValue("node.data", _canBeDataNode),
					ToKeyValue("path.data", _dataDirectories.Count != 0 ? String.Join(",", _dataDirectories) : null),
					ToKeyValue("transport.tcp.port", _nodeCommunicationPort),
					ToKeyValue("http.port", _httpPort),
					ToKeyValue("discovery.zen.minimum_master_nodes", _minimumMasterNodes),
					ToKeyValue("discovery.zen.ping.multicast.enabled", _enableMulticastDiscovery),
					ToKeyValue("discovery.zen.ping.unicast.hosts", MasterNodeListString())
				}.Where(kv => kv.HasValue).Select(kv => kv.Value));
		}

		private string MasterNodeListString()
		{
			if (_masterNodes == null || _masterNodes.Count == 0)
			{
				return null;
			}
			return String.Join(",", _masterNodes);
		}

		private static KeyValuePair<string, string>? ToKeyValue<T>(string name, T value)
			where T : struct
		{
			return ToKeyValue(name, value.ToString());
		}

		private static KeyValuePair<string, string>? ToKeyValue<T>(string name, T? value)
			where T: struct
		{
			return ToKeyValue(name, value.HasValue ? value.Value.ToString() : null);
		}

		private static KeyValuePair<string, string>? ToKeyValue(string name, string value)
		{
			if (value != null)
			{
				return new KeyValuePair<string, string>(name, value);
			}
			else
			{
				return null;
			}
		}
	}
}
