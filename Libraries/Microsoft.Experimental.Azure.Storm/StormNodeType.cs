namespace Microsoft.Experimental.Azure.Storm
{
	/// <summary>
	/// The type of work done by this Storm Node in Azure.
	/// </summary>
	public enum StormNodeType
	{
		/// <summary>
		/// Nimbus: the coordinator.
		/// </summary>
		Nimbus,
		/// <summary>
		/// Supervisor: the worker.
		/// </summary>
		Supervisor,
		/// <summary>
		/// Nimbus with a UI web page server.
		/// </summary>
		NimbusWithUI,
		/// <summary>
		/// UI web page server.
		/// </summary>
		UI,
		/// <summary>
		/// A DRPC (distributed-RPC) server.
		/// </summary>
		Drpc,
		/// <summary>
		/// A Supervisor with DRPC server.
		/// </summary>
		SupervisorWithDrpc,
		/// <summary>
		/// Custom: no work is started by default other than laying down the Storm bits.
		/// </summary>
		Custom,
	}
}
