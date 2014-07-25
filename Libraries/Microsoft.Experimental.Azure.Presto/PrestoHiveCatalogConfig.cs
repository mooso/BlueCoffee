using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Presto
{
	/// <summary>
	/// Config for a Hive catalog for Presto.
	/// </summary>
	public sealed class PrestoHiveCatalogConfig : PrestoCatalogConfig
	{
		private readonly string _metastoreUri;

		/// <summary>
		/// Creates the config.
		/// </summary>
		/// <param name="metastoreUri">The URI for the metastore service.</param>
		/// <param name="catalogName">The name of the catalog.</param>
		public PrestoHiveCatalogConfig(string metastoreUri, string catalogName = "hive")
			: base(catalogName)
		{
			_metastoreUri = metastoreUri;
		}

		/// <summary>
		/// Name of the connector.
		/// </summary>
		public override string ConnectorName
		{
			get { return "hive-hadoop2"; }
		}

		/// <summary>
		/// The catalog-specific properties.
		/// </summary>
		/// <returns>The properties</returns>
		public override IEnumerable<KeyValuePair<string, string>> GetCatalogSpecificProperties()
		{
			return new Dictionary<string, string>()
			{
				{ "hive.metastore.uri", _metastoreUri },
			};
		}
	}
}
