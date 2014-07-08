using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public abstract class ProcessOutputTracer
	{
		public abstract void TraceStandardOut(string outputLine);

		public abstract void TraceStandardError(string outputLine);
	}
}
