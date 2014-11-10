using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ZooKeeper
{
	/// <summary>
	/// Description of a ZK node attempting to join a cluster.
	/// </summary>
	public sealed class ZooKeeperQuorumPeer
	{
		/// <summary>
		/// Default leader election port.
		/// </summary>
		public const int DefaultLeaderElectionPort = 3888;
		/// <summary>
		/// Default quorum peer port.
		/// </summary>
		public const int DefaultQuorumPeerPort = 2888;
		private readonly string _hostName;
		private readonly int _quorumPeerPort;
		private readonly int _leaderElectionPort;

		/// <summary>
		/// Create a new description.
		/// </summary>
		/// <param name="hostName">The host name.</param>
		/// <param name="quorumPeerPort">The main port it uses to communicate among its peers.</param>
		/// <param name="leaderElectionPort">The port it uses to elect leaders.</param>
		public ZooKeeperQuorumPeer(string hostName,
			int quorumPeerPort = DefaultQuorumPeerPort,
			int leaderElectionPort = DefaultLeaderElectionPort)
		{
			_hostName = hostName;
			_quorumPeerPort = quorumPeerPort;
			_leaderElectionPort = leaderElectionPort;
		}

		/// <summary>
		/// The host name.
		/// </summary>
		public string HostName { get { return _hostName; } }
		/// <summary>
		/// The main port it uses to communicate among its peers.
		/// </summary>
		public int QuorumPeerPort { get { return _quorumPeerPort; } }
		/// <summary>
		/// The port it uses to elect leaders.
		/// </summary>
		public int LeaderElectionPort { get { return _leaderElectionPort; } }

		/// <summary>
		/// The string representation of this as it would appear in the config file.
		/// </summary>
		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture,
				"{0}:{1}:{2}",
				_hostName, _quorumPeerPort, _leaderElectionPort);
		}
	}
}
