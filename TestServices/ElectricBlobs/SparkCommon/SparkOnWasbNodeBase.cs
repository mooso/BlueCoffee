using Microsoft.Experimental.Azure.CommonTestUtilities;
using Microsoft.Experimental.Azure.Spark;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace SparkCommon
{
	public abstract class SparkOnWasbNodeBase : SparkNodeBase
	{
		protected override ImmutableDictionary<string, string> GetHadoopConfigProperties()
		{
			return WasbConfiguration.GetWasbConfigKeys();
		}
	}
}
