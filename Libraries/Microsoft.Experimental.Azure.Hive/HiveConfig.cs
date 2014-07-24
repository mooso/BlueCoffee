using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// Hive configuration.
	/// </summary>
	public sealed class HiveConfig
	{
		private readonly string _derbyDataDirectory;

		/// <summary>
		/// Creates the Hive configuration.
		/// </summary>
		/// <param name="derbyDataDirectory">The local directory where the Derby DB files for the metastore will be stored.</param>
		public HiveConfig(string derbyDataDirectory)
		{
			_derbyDataDirectory = derbyDataDirectory;
		}

		/// <summary>
		/// Create the XML representation of this configuration.
		/// </summary>
		/// <returns>The XML representation.</returns>
		public XDocument ToXml()
		{
			return new XDocument(
				new XElement("configuration",
					PropertyElement("hive.metastore.warehouse.dir", _derbyDataDirectory)
				)
			);
		}

		private static XElement PropertyElement(string name, string value)
		{
			return new XElement("property",
				new XElement("name", name),
				new XElement("value", value));
		}
	}
}
