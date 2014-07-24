using PrestoCommon;

namespace PrestoCoordinator
{
	public class WorkerRole : PrestoNodeBase
	{
		protected override bool IsCoordinator { get { return true; } }
	}
}
