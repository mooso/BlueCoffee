using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// Configuration for Shark.
	/// </summary>
	public sealed class SharkConfig
	{
		private readonly int _serverPort;
		private readonly FakeHiveConfig _hiveConfig;
		private readonly int _maxMemoryMb;
		private readonly string _sparkMaster;
		private readonly int _executorMemoryMb;
		private readonly ImmutableDictionary<string, string> _extraSparkProperties;

		/// <summary>
		/// Creates a new config.
		/// </summary>
		/// <param name="serverPort">The port for the Shark server to listen on.</param>
		/// <param name="metastoreUris">The URI of the metastore service to connect to.</param>
		/// <param name="maxMemoryMb">The maximum memory of the Shark server.</param>
		/// <param name="sparkMaster">The Spark master URI.</param>
		/// <param name="extraHiveConfig">Optional extra configuration parameters for Hive.</param>
		/// <param name="executorMemoryMb">The memory bound on each standalone executor launched by Shark.</param>
		/// <param name="extraSparkProperties">Other Spark properties than what's explicitly given in the parameters above.</param>
		public SharkConfig(int serverPort, string metastoreUris, string sparkMaster, int maxMemoryMb = 1024,
			ImmutableDictionary<string, string> extraHiveConfig = null, int executorMemoryMb = 1024,
			ImmutableDictionary<string, string> extraSparkProperties = null)
		{
			_serverPort = serverPort;
			_hiveConfig = new FakeHiveConfig(metastoreUris, extraHiveConfig ?? ImmutableDictionary<string, string>.Empty);
			_maxMemoryMb = maxMemoryMb;
			_sparkMaster = sparkMaster;
			_executorMemoryMb = executorMemoryMb;
			_extraSparkProperties = extraSparkProperties ?? ImmutableDictionary<string, string>.Empty;
		}

		/// <summary>
		/// The port Shark server will listen on.
		/// </summary>
		public int ServerPort { get { return _serverPort; } }

		/// <summary>
		/// The maximum memory of the Shark server.
		/// </summary>
		public int MaxMemoryMb { get { return _maxMemoryMb; } }

		/// <summary>
		/// The memory bound on each standalone executor launched by Shark.
		/// </summary>
		public int ExecutorMemoryMb { get { return _executorMemoryMb; } }

		/// <summary>
		/// Other Spark properties than what's explicitly given.
		/// </summary>
		public ImmutableDictionary<string, string> ExtraSparkProperties { get { return _extraSparkProperties; } }

		/// <summary>
		/// The Spark master URI;
		/// </summary>
		public string SparkMaster { get { return _sparkMaster; } }

		internal XDocument GetHiveConfigXml()
		{
			return _hiveConfig.ToXml();
		}

		private sealed class FakeHiveConfig : HadoopStyleXmlConfig
		{
			private readonly string _metastoreUris;
			private readonly ImmutableDictionary<string, string> _extraHiveConfig;

			public FakeHiveConfig(string metastoreUris, ImmutableDictionary<string, string> extraHiveConfig)
			{
				_metastoreUris = metastoreUris;
				_extraHiveConfig = extraHiveConfig;
			}

			/// <summary>
			/// The configuration properties.
			/// </summary>
			protected override IEnumerable<KeyValuePair<string, string>> ConfigurationProperties
			{
				get
				{
					return new Dictionary<string, string>()
					{
						{ "hive.metastore.uris", _metastoreUris },
					}.Concat(ExtraHiveConfigWithFixedProperties());
				}
			}

			private ImmutableDictionary<string, string> ExtraHiveConfigWithFixedProperties()
			{
				// In the Hive jars we're using, the some defaults are hard-coded to /tmp instead of
				// using ${system.java.io.tmpdir} as the newer Hive code does. Fix that.
				const string localScratchDirKey = "hive.exec.local.scratchdir";
				const string queryLogKey = "hive.querylog.location";
				var ret = _extraHiveConfig;
				ret = AddIfNotExists(ret, localScratchDirKey, @"${java.io.tmpdir}/${user.name}/HiveSratch");
				ret = AddIfNotExists(ret, queryLogKey, @"${java.io.tmpdir}/${user.name}/QueryLog");
				// Hive needs to know that we're not executing locally, or else it's going to write
				// results for queries into files on the local file system, and on a cluster it would
				// execute queries but not return any results (that was a fun bug to debug)
				const string mapredFrameworkKey = "mapreduce.framework.name";
				ret = AddIfNotExists(ret, mapredFrameworkKey, "classic"); // Anything other than "local" would work really I think
				return ret;
			}

			private static ImmutableDictionary<string, string> AddIfNotExists(
				ImmutableDictionary<string, string> dict, string key, string value)
			{
				return dict.ContainsKey(key) ? dict : dict.Add(key, value);
			}
		}
	}
}
