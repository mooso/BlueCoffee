using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Presto
{
	/// <summary>
	/// Catalog properties for a Cassandra catalog for Presto.
	/// </summary>
	public sealed class PrestoCassandraCatalogConfig : PrestoCatalogConfig
	{
		private readonly ImmutableList<string> _contactPoints;
		private readonly int _cassandraNativeProtocolPort;

		/// <summary>
		/// Creates a new config.
		/// </summary>
		/// <param name="contactPoints">The list of host names for the Cassandra servers to contact to discover topology.</param>
		/// <param name="cassandraNativeProtocolPort">The native protocol port on the Cassandra cluster.</param>
		public PrestoCassandraCatalogConfig(IEnumerable<string> contactPoints,
			int cassandraNativeProtocolPort = 9142)
		{
			_contactPoints = contactPoints.ToImmutableList();
			_cassandraNativeProtocolPort = cassandraNativeProtocolPort;
		}
	
		/// <summary>
		/// The name of the connector (cassandra).
		/// </summary>
		public override string ConnectorName
		{
			get { return "cassandra"; }
		}

		/// <summary>
		/// The name of the properties file (cassandra).
		/// </summary>
		public override string CatalogPropertiesFileName
		{
			get { return "cassandra"; }
		}

		/// <summary>
		/// Gets the Cassandra-specific properties.
		/// </summary>
		/// <returns>The properties.</returns>
		public override IEnumerable<KeyValuePair<string, string>> GetCatalogSpecificProperties()
		{
			return new Dictionary<string, string>()
			{
				{ "cassandra.contact-points", String.Join(",", _contactPoints) },
				{ "cassandra.native-protocol-port", _cassandraNativeProtocolPort.ToString() },
			};
		}
	}
}
