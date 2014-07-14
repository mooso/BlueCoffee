using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// An abstract definition for a class that can take output lines from a process and log it somewhere useful.
	/// </summary>
	public abstract class ProcessOutputTracer
	{
		/// <summary>
		/// Trace the given line that the underlying process output in its stdout.
		/// </summary>
		/// <param name="outputLine">The output line.</param>
		public abstract void TraceStandardOut(string outputLine);

		/// <summary>
		/// Trace the given line that the underlying process output in its stderr.
		/// </summary>
		/// <param name="outputLine">The output line.</param>
		public abstract void TraceStandardError(string outputLine);
	}
}
