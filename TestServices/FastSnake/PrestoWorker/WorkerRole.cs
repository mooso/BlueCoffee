using PrestoCommon;

namespace PrestoWorker
{
	public class WorkerRole : PrestoNodeBase
	{
		protected override bool IsCoordinator { get { return false; } }
	}
}
