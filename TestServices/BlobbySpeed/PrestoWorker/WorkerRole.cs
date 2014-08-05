using PrestoCommon;

namespace PrestoWorker
{
	public class WorkerRole : PrestoWithHiveNodeBase
	{
		protected override bool IsCoordinator { get { return false; } }
	}
}
