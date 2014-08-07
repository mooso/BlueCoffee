using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.Shark
{
	/// <summary>
	/// Configuration for Shark.
	/// </summary>
	public sealed class SharkConfig
	{
		private readonly int _serverPort;
		private readonly FakeHiveConfig _hiveConfig;
		private readonly int _maxMemoryMb;
		private readonly string _sparkHome;
		private readonly string _sparkMaster;

		/// <summary>
		/// Creates a new config.
		/// </summary>
		/// <param name="serverPort">The port for the Shark server to listen on.</param>
		/// <param name="metastoreUris">The URI of the metastore service to connect to.</param>
		/// <param name="maxMemoryMb">The maximum memory of the Shark server.</param>
		/// <param name="sparkHome">The home directory where Spark is installed.</param>
		/// <param name="sparkMaster">The Spark master URI.</param>
		/// <param name="extraHiveConfig">Optional extra configuration parameters for Hive.</param>
		public SharkConfig(int serverPort, string metastoreUris, string sparkHome, string sparkMaster, int maxMemoryMb = 1024,
			ImmutableDictionary<string, string> extraHiveConfig = null)
		{
			_serverPort = serverPort;
			_hiveConfig = new FakeHiveConfig(metastoreUris, extraHiveConfig ?? ImmutableDictionary<string, string>.Empty);
			_maxMemoryMb = maxMemoryMb;
			_sparkHome = sparkHome;
			_sparkMaster = sparkMaster;
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
		/// The Spark master URI;
		/// </summary>
		public string SparkMaster { get { return _sparkMaster; } }

		/// <summary>
		/// The home directory where Spark is installed.
		/// </summary>
		public string SparkHome { get { return _sparkHome; } }

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
					}.Concat(_extraHiveConfig);
				}
			}
		}
	}
}
