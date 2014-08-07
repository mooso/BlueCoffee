using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// Configuration for a Hive metastore.
	/// </summary>
	public abstract class HiveMetastoreConfig : HadoopStyleXmlConfig
	{
		private readonly int _port;
		private readonly ImmutableDictionary<string, string> _extraProperties;
		
		/// <summary>
		/// Creates the config.
		/// </summary>
		/// <param name="port">The port.</param>
		/// <param name="extraProperties">Any extra configuration properties to set.</param>
		protected HiveMetastoreConfig(int port, ImmutableDictionary<string, string> extraProperties)
		{
			_port = port;
			_extraProperties = extraProperties ?? ImmutableDictionary<string, string>.Empty;
		}

		/// <summary>
		/// The configuration properties.
		/// </summary>
		protected sealed override IEnumerable<KeyValuePair<string, string>> ConfigurationProperties
		{
			get
			{
				return _extraProperties.Concat(MetastoreSpecificProperties);
			}
		}

		/// <summary>
		/// Any meta-store specific properties.
		/// </summary>
		protected virtual IEnumerable<KeyValuePair<string, string>> MetastoreSpecificProperties
		{
			get
			{
				return Enumerable.Empty<KeyValuePair<string, string>>();
			}
		}

		/// <summary>
		/// The port.
		/// </summary>
		public int Port { get { return _port; } }
	}
}
