using SparkCommon;

namespace SparkSlave
{
	public class WorkerRole : SparkOnWasbNodeBase
	{
		protected override bool IsMaster { get { return false; } }
	}
}
