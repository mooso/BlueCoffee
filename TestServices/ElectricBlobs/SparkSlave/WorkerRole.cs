using SparkCommon;

namespace SparkSlave
{
	public class WorkerRole : SparkNodeBase
	{
		protected override bool IsMaster { get { return false; } }
	}
}
