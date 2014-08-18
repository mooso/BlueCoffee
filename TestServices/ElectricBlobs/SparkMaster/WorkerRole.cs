using SparkCommon;

namespace SparkMaster
{
	public class WorkerRole : SparkOnWasbNodeBase
	{
		protected override bool IsMaster { get { return true; } }
	}
}
