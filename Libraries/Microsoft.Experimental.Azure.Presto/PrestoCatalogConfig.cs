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
		private readonly string _catalogName;

		/// <summary>
		/// Creates this config.
		/// </summary>
		/// <param name="catalogName">The name of the catalog.</param>
		protected PrestoCatalogConfig(string catalogName)
		{
			_catalogName = catalogName;
		}

		/// <summary>
		/// The connector name for this catalog.
		/// </summary>
		public abstract string ConnectorName { get; }
		
		/// <summary>
		/// The name of the catalog.
		/// </summary>
		public string CatalogName
		{
			get { return _catalogName; }
		}

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
