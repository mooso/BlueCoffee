using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public sealed class DefaultProcessOutputTracer : ProcessOutputTracer
	{
		private readonly string _prefix;

		public DefaultProcessOutputTracer(string prefix = null)
		{
			_prefix = prefix ?? "";
		}

		public override void TraceStandardOut(string outputLine)
		{
			Trace.TraceInformation(_prefix + outputLine);
		}

		public override void TraceStandardError(string outputLine)
		{
			Trace.TraceWarning(_prefix + outputLine);
		}
	}
}
