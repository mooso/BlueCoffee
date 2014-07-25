using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// Configuration for a Hive server.
	/// </summary>
	public sealed class HiveServerConfig : HiveConfig
	{
		private readonly int _port;
		private readonly string _metastoreUris;

		/// <summary>
		/// Creates the config.
		/// </summary>
		/// <param name="port">The port to listen on.</param>
		/// <param name="metastoreUris">The URI of the metastore service to connect to.</param>
		public HiveServerConfig(int port, string metastoreUris)
		{
			_port = port;
			_metastoreUris = metastoreUris;
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
					{ "hive.server2.thrift.port", _port.ToString() },
				};
			}
		}
	}
}
