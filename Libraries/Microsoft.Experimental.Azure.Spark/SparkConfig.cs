using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		/// <summary>
		/// Creates the config.
		/// </summary>
		/// <param name="masterAddress">The IP/address of the master node.</param>
		/// <param name="masterPort">The port for the master node.</param>
		/// <param name="masterWebUIPort">The port for the web UI on the master node.</param>
		public SparkConfig(string masterAddress, int masterPort, int masterWebUIPort)
		{
			_masterAddress = masterAddress;
			_masterPort = masterPort;
			_masterWebUIPort = masterWebUIPort;
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
	}
}
