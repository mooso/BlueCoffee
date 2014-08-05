using PrestoCommon;

namespace PrestoWorker
{
	public class WorkerRole : PrestoWithCassandraNodeBase
	{
		protected override bool IsCoordinator { get { return false; } }
	}
}
