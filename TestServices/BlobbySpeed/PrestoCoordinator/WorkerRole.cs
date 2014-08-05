using PrestoCommon;

namespace PrestoCoordinator
{
	public class WorkerRole : PrestoWithHiveNodeBase
	{
		protected override bool IsCoordinator { get { return true; } }
	}
}
