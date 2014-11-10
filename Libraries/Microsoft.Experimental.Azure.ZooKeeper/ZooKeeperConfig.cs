using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ZooKeeper
{
	/// <summary>
	/// ZooKeeper configuration.
	/// </summary>
	public sealed class ZooKeeperConfig
	{
		/// <summary>
		/// The default TCP port used for communicating with ZooKeeper.
		/// </summary>
		public const int DefaultClientPort = 2181;
		private readonly string _snapshotDirectory;
		private readonly int _clientPort;
		private readonly ImmutableList<ZooKeeperQuorumPeer> _allNodes;
		private readonly int _myId;
		private readonly int _tickTime;
		private readonly int _syncLimit;
		private readonly int _initLimit;

		/// <summary>
		/// Creates the configuration.
		/// </summary>
		/// <param name="snapshotDirectory">Directory for snapshots.</param>
		/// <param name="clientPort">The TCP port to use for clients communicating with the server.</param>
		/// <param name="allNodes">The ZK nodes in the cluster, ordered by ID. Can be null for single-node clusters.</param>
		/// <param name="myId">This node's ID (1-based).</param>
		/// <param name="tickTime">
		/// The length of a single tick, which is the basic time unit used by ZooKeeper, as measured in milliseconds.
		/// It is used to regulate heartbeats, and timeouts. For example, the minimum session timeout will be two ticks.
		/// </param>
		/// <param name="initLimit">
		/// Amount of time, in ticks (see tickTime), to allow followers to connect and sync to a leader.
		/// Increase this value as needed, if the amount of data managed by ZooKeeper is large.
		/// </param>
		/// <param name="syncLimit">
		/// Amount of time, in ticks (see tickTime), to allow followers to sync with ZooKeeper.
		/// If followers fall too far behind a leader, they will be dropped.
		/// </param>
		public ZooKeeperConfig(string snapshotDirectory,
			int clientPort = DefaultClientPort,
			IEnumerable<ZooKeeperQuorumPeer> allNodes = null, int myId = 1,
			int tickTime = 2000, int initLimit = 5, int syncLimit = 2)
		{
			_snapshotDirectory = snapshotDirectory;
			_clientPort = clientPort;
			_allNodes = (allNodes ?? Enumerable.Empty<ZooKeeperQuorumPeer>()).ToImmutableList();
			_myId = myId;
			_tickTime = tickTime;
			_initLimit = initLimit;
			_syncLimit = syncLimit;
		}

		/// <summary>
		/// Directory for snapshots (data directory).
		/// </summary>
		public string SnapshotDirectory { get { return _snapshotDirectory; } }

		/// <summary>
		/// The TCP port to use for communicating with the server.
		/// </summary>
		public int ClientPort { get { return _clientPort; } }

		/// <summary>
		/// The ZK nodes in the cluster, ordered by ID. Empty for single-node clusters.
		/// </summary>
		public IEnumerable<ZooKeeperQuorumPeer> AllNodes { get { return _allNodes; } }

		/// <summary>
		/// This node's ID in the cluster (1-based).
		/// </summary>
		public int MyId { get { return _myId; } }

		/// <summary>
		/// Represent this configuration as a properties file.
		/// </summary>
		/// <returns>The properties file to use.</returns>
		public PropertiesFile ToPropertiesFile()
		{
			return new PropertiesFile(new Dictionary<string, string>()
			{
				{ "dataDir", _snapshotDirectory.Replace('\\', '/') },
				{ "clientPort", _clientPort.ToString() },
			}.Concat(CreateQuorumPeerProperties()));
		}

		private IEnumerable<KeyValuePair<string, string>> CreateQuorumPeerProperties()
		{
			if (_allNodes.Count == 0)
			{
				return new Dictionary<string, string>();
			}
			return _allNodes
				.Select((n, i) => new KeyValuePair<string, string>("server." + (i + 1), n.ToString()))
				.Concat(new Dictionary<string, string>()
				{
					{ "tickTime", _tickTime.ToString() },
					{ "initLimit", _initLimit.ToString() },
					{ "syncLimit", _syncLimit.ToString() },
				});
		}
	}
}
