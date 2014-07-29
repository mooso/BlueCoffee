using SparkCommon;

namespace SparkMaster
{
	public class WorkerRole : SparkNodeBase
	{
		protected override bool IsMaster { get { return true; } }
	}
}
