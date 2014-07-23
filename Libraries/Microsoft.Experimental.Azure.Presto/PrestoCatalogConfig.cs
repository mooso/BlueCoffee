using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Presto
{
	/// <summary>
	/// A configuration for a Presto Catalog.
	/// </summary>
	public abstract class PrestoCatalogConfig
	{
		/// <summary>
		/// The connector name for this catalog.
		/// </summary>
		public abstract string ConnectorName { get; }
		
		/// <summary>
		/// The name without extension of the properties file representing this catalog (e.g. "cassandra")
		/// </summary>
		public abstract string CatalogPropertiesFileName { get; }

		/// <summary>
		/// Gets the catalog-specific properties for the properties for this catalog.
		/// </summary>
		/// <returns>The catalog-specific properties.</returns>
		public abstract IEnumerable<KeyValuePair<string, string>> GetCatalogSpecificProperties();

		/// <summary>
		/// Creates the properties file for this catalog.
		/// </summary>
		/// <returns>The properties file.</returns>
		public PropertiesFile ToPropertiesFile()
		{
			return new PropertiesFile(
				new[] { new KeyValuePair<string, string>("connector.name", ConnectorName) }
				.Concat(GetCatalogSpecificProperties()));
		}
	}
}
