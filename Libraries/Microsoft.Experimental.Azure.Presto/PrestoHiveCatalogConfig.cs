using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.Presto
{
	/// <summary>
	/// Config for a Hive catalog for Presto.
	/// </summary>
	public sealed class PrestoHiveCatalogConfig : PrestoCatalogConfig
	{
		private readonly string _metastoreUri;
		private readonly ImmutableList<KeyValuePair<string, string>> _hiveConfigurationProperties;
		private string _hiveConfigFile;

		/// <summary>
		/// Creates the config.
		/// </summary>
		/// <param name="metastoreUri">The URI for the metastore service.</param>
		/// <param name="catalogName">The name of the catalog.</param>
		/// <param name="hiveConfigurationProperties">Any extra Hive/Hadoop configuration properties.</param>
		public PrestoHiveCatalogConfig(string metastoreUri,
			IEnumerable<KeyValuePair<string, string>> hiveConfigurationProperties = null,
			string catalogName = "hive")
			: base(catalogName)
		{
			_metastoreUri = metastoreUri;
			_hiveConfigurationProperties =
				(hiveConfigurationProperties ?? Enumerable.Empty<KeyValuePair<string, string>>())
				.ToImmutableList();
		}

		/// <summary>
		/// Name of the connector.
		/// </summary>
		public override string ConnectorName
		{
			get { return "hive-hadoop2"; }
		}

		private static XElement PropertyElement(KeyValuePair<string, string> nameValue)
		{
			return new XElement("property",
				new XElement("name", nameValue.Key),
				new XElement("value", nameValue.Value));
		}

		/// <summary>
		/// The catalog-specific properties.
		/// </summary>
		/// <param name="configDirectory">The config directory - will write hive-site.xml there.</param>
		/// <returns>The properties</returns>
		protected override IEnumerable<KeyValuePair<string, string>> GetCatalogSpecificProperties(string configDirectory)
		{
			var properties = new Dictionary<string, string>()
			{
				{ "hive.metastore.uri", _metastoreUri },
			};
			if (_hiveConfigurationProperties.Count > 0)
			{
				var xml = new XDocument(
					new XElement("configuration",
						_hiveConfigurationProperties.Select(PropertyElement)
					)
				);
				_hiveConfigFile = Path.Combine(configDirectory, "hive-site.xml");
				xml.Save(_hiveConfigFile);
				properties.Add("hive.config.resources", _hiveConfigFile.Replace('\\', '/'));
			}
			return properties;
		}
	}
}
