using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// A hive service configuration.
	/// </summary>
	public abstract class HiveConfig
	{
		/// <summary>
		/// Create the XML representation of this configuration.
		/// </summary>
		/// <returns>The XML representation.</returns>
		public XDocument ToXml()
		{
			return new XDocument(
				new XElement("configuration",
					ConfigurationProperties.Select(PropertyElement)
				)
			);
		}

		/// <summary>
		/// The configuration properties.
		/// </summary>
		protected abstract IEnumerable<KeyValuePair<string, string>> ConfigurationProperties
		{ get; }

		private static XElement PropertyElement(KeyValuePair<string, string> nameValue)
		{
			return new XElement("property",
				new XElement("name", nameValue.Key),
				new XElement("value", nameValue.Value));
		}
	}
}
