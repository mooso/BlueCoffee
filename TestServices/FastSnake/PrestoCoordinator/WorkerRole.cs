using PrestoCommon;

namespace PrestoCoordinator
{
	public class WorkerRole : PrestoWithCassandraNodeBase
	{
		protected override bool IsCoordinator { get { return true; } }
	}
}
