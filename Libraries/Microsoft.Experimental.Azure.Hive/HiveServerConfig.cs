using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// Configuration for a Hive server.
	/// </summary>
	public sealed class HiveServerConfig : HadoopStyleXmlConfig
	{
		private readonly int _port;
		private readonly string _metastoreUris;
		private readonly ImmutableDictionary<string, string> _extraProperties;

		/// <summary>
		/// Creates the config.
		/// </summary>
		/// <param name="port">The port to listen on.</param>
		/// <param name="metastoreUris">The URI of the metastore service to connect to.</param>
		/// <param name="extraProperties">Any extra configuration properties to set.</param>
		public HiveServerConfig(int port, string metastoreUris, ImmutableDictionary<string, string> extraProperties = null)
		{
			_port = port;
			_metastoreUris = metastoreUris;
			_extraProperties = extraProperties ?? ImmutableDictionary<string, string>.Empty;
		}

		/// <summary>
		/// The configuration properties.
		/// </summary>
		protected override IEnumerable<KeyValuePair<string, string>> ConfigurationProperties
		{
			get
			{
				return _extraProperties.Concat(new Dictionary<string, string>()
				{
					{ "hive.metastore.uris", _metastoreUris },
					{ "hive.server2.thrift.port", _port.ToString() },
				});
			}
		}
	}
}
