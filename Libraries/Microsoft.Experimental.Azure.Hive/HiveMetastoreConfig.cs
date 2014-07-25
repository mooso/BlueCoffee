using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// Configuration for a Hive metastore.
	/// </summary>
	public abstract class HiveMetastoreConfig : HiveConfig
	{
		private readonly int _port;
		
		/// <summary>
		/// Creates the config.
		/// </summary>
		/// <param name="port">The port.</param>
		protected HiveMetastoreConfig(int port)
		{
			_port = port;
		}

		/// <summary>
		/// The port.
		/// </summary>
		public int Port { get { return _port; } }
	}
}
