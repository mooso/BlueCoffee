using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.IO;
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
		/// <param name="configDirectory">The configuration directory, in case one needs to write extra config files there.</param>
		/// <returns>The catalog-specific properties.</returns>
		protected abstract IEnumerable<KeyValuePair<string, string>> GetCatalogSpecificProperties(string configDirectory);

		/// <summary>
		/// Creates the properties file for this catalog.
		/// </summary>
		public void CreatePropertiesFile(string configDirectory)
		{
			var propertiesFile = new PropertiesFile(
				new[] { new KeyValuePair<string, string>("connector.name", ConnectorName) }
				.Concat(GetCatalogSpecificProperties(configDirectory)));
			propertiesFile.WriteToFile(Path.Combine(configDirectory, CatalogName + ".properties"));
		}
	}
}
