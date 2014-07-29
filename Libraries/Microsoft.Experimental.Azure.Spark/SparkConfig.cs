using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// Configuration for the Spark cluster.
	/// </summary>
	public sealed class SparkConfig
	{
		private readonly string _masterAddress;
		private readonly int _masterPort;
		private readonly int _masterWebUIPort;
		private readonly ImmutableDictionary<string, string> _hadoopConfigProperties;

		/// <summary>
		/// Creates the config.
		/// </summary>
		/// <param name="masterAddress">The IP/address of the master node.</param>
		/// <param name="masterPort">The port for the master node.</param>
		/// <param name="masterWebUIPort">The port for the web UI on the master node.</param>
		/// <param name="hadoopConfigProperties">Optional extra config properties for the Hadoop side of the world.</param>
		public SparkConfig(string masterAddress, int masterPort, int masterWebUIPort,
			ImmutableDictionary<string, string> hadoopConfigProperties = null)
		{
			_masterAddress = masterAddress;
			_masterPort = masterPort;
			_masterWebUIPort = masterWebUIPort;
			_hadoopConfigProperties = (hadoopConfigProperties ?? ImmutableDictionary<string, string>.Empty)
				.SetItem("fs.azure.skip.metrics", "true");
;
		}

		/// <summary>
		/// The IP/address of the master node.
		/// </summary>
		public string MasterAddress { get { return _masterAddress; } }

		/// <summary>
		/// The port the master node listens to.
		/// </summary>
		public int MasterPort { get { return _masterPort; } }

		/// <summary>
		/// The URI for the Spark master node.
		/// </summary>
		public string SparkMasterUri
		{
			get
			{
				return String.Format("spark://{0}:{1}", _masterAddress, _masterPort);
			}
		}

		/// <summary>
		/// The port for the web UI on the master node.
		/// </summary>
		public int MasterWebUIPort { get { return _masterWebUIPort; } }

		internal void WriteHadoopCoreSiteXml(string hadoopConfDirectory)
		{
			var doc = new XDocument(
				new XElement("configuration",
					_hadoopConfigProperties.Select(PropertyElement)
				)
			);
			doc.Save(Path.Combine(hadoopConfDirectory, "core-site.xml"));
		}

		private static XElement PropertyElement(KeyValuePair<string, string> nameValue)
		{
			return new XElement("property",
				new XElement("name", nameValue.Key),
				new XElement("value", nameValue.Value));
		}
	}
}
