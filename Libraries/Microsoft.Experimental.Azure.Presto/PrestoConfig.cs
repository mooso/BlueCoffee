using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Presto
{
	/// <summary>
	/// Configuration for the Presto node.
	/// </summary>
	public sealed class PrestoConfig
	{
		private readonly string _environmentName;
		private readonly string _nodeId;
		private readonly string _dataDirectory;
		private readonly string _pluginConfigDirectory;
		private readonly string _pluginInstallDirectory;
		private readonly bool _isCoordinator;
		private readonly bool _isWorker;
		private readonly bool _isDiscoveryServer;
		private readonly int _httpPort;
		private readonly int _maxTaskMemoryMb;
		private readonly string _discoveryServerUri;
		private readonly ImmutableList<PrestoCatalogConfig> _catalogs;

		/// <summary>
		/// Creates a new configuration.
		/// </summary>
		/// <param name="nodeId">The unique node ID.</param>
		/// <param name="dataDirectory">The directory where Presto's data will be stored.</param>
		/// <param name="pluginConfigDirectory">The plugin configuration directory.</param>
		/// <param name="pluginInstallDirectory">The plugin installation directory.</param>
		/// <param name="discoveryServerUri">The URI (http://example.net:8080) for the discovery server.</param>
		/// <param name="catalogs">The list of catalogs to use for data.</param>
		/// <param name="environmentName">The environment name shared by all the Presto nodes in this cluster.</param>
		/// <param name="isCoodrinator">Whether this will be a coordinator node.</param>
		/// <param name="isWorker">Whether this will be a worker node (can be paired with being a coordinator).</param>
		/// <param name="isDiscoveryServer">Whether this will be a discovery server node.</param>
		/// <param name="httpPort">The HTTP port exposed for node communication.</param>
		/// <param name="maxTaskMemoryMb">Maximum amount of memory used by a single task.</param>
		public PrestoConfig(string nodeId,
			string dataDirectory, string pluginConfigDirectory, string pluginInstallDirectory,
			string discoveryServerUri,
			IEnumerable<PrestoCatalogConfig> catalogs,
			string environmentName = "presto", bool isCoodrinator = true,
			bool isWorker = true, bool isDiscoveryServer = true,
			int httpPort = 8080, int maxTaskMemoryMb = 1024)
		{
			_environmentName = environmentName;
			_nodeId = nodeId;
			_dataDirectory = dataDirectory;
			_isCoordinator = isCoodrinator;
			_isWorker = isWorker;
			_isDiscoveryServer = isDiscoveryServer;
			_httpPort = httpPort;
			_maxTaskMemoryMb = maxTaskMemoryMb;
			_discoveryServerUri = discoveryServerUri;
			_catalogs = catalogs.ToImmutableList();
			_pluginConfigDirectory = pluginConfigDirectory;
			_pluginInstallDirectory = pluginInstallDirectory;
		}

		internal IEnumerable<string> AllDirectories
		{
			get
			{
				return new[] { _dataDirectory, _pluginConfigDirectory, _pluginInstallDirectory };
			}
		}

		/// <summary>
		/// The plug-in install directory.
		/// </summary>
		public string PluginInstallDirectory { get { return _pluginInstallDirectory; } }

		/// <summary>
		/// Gets the node properties to be passed as system properties to the Java program.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<string, string>> GetNodeProperties()
		{
			return new Dictionary<string, string>()
				{
					{ "node.environment", _environmentName },
					{ "node.id", _nodeId },
					{ "node.data", _dataDirectory.Replace('\\', '/') },
				};
		}

		/// <summary>
		/// Creates the config.properties file with values from this configuration.
		/// </summary>
		/// <returns></returns>
		public PropertiesFile CreateConfigPropertiesFile()
		{
			return new PropertiesFile(new Dictionary<string, string>()
				{
					{ "coordinator", _isCoordinator.ToString() },
					{ "node-scheduler.include-coordinator", _isWorker.ToString() },
					{ "discovery-server.enabled", _isDiscoveryServer.ToString() },
					{ "http-server.http.port", _httpPort.ToString() },
					{ "task.max-memory", _maxTaskMemoryMb + "MB" },
					{ "discovery.uri", _discoveryServerUri.TrimEnd('/') },
					{ "plugin.config-dir", _pluginConfigDirectory.Replace('\\', '/') },
					{ "plugin.dir", _pluginInstallDirectory.Replace('\\', '/') },
				});
		}

		/// <summary>
		/// The list of catalogs.
		/// </summary>
		public IEnumerable<PrestoCatalogConfig> Catalogs { get { return _catalogs; } }

		/// <summary>
		/// Writes all the catalog config files into the plugin config directory.
		/// </summary>
		public void WriteAllCatalogConfigFiles()
		{
			if (!Directory.Exists(_pluginConfigDirectory))
			{
				Directory.CreateDirectory(_pluginConfigDirectory);
			}
			foreach (var catalog in _catalogs)
			{
				catalog.ToPropertiesFile().WriteToFile(
					Path.Combine(_pluginConfigDirectory,
					catalog.CatalogName + ".properties"));
			}
		}
	}
}
