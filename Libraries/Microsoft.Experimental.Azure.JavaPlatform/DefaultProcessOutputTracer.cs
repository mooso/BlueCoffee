using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// The default output tracer class (traces to <see cref="System.Diagnostics.Trace"/>).
	/// </summary>
	public sealed class DefaultProcessOutputTracer : ProcessOutputTracer
	{
		private readonly string _prefix;

		/// <summary>
		/// Creates the output tracer.
		/// </summary>
		/// <param name="prefix">An optional prefix to use for traced messages.</param>
		public DefaultProcessOutputTracer(string prefix = null)
		{
			_prefix = prefix ?? "";
		}

		/// <summary>
		/// Trace this line as Information.
		/// </summary>
		/// <param name="outputLine">The line to trace.</param>
		public override void TraceStandardOut(string outputLine)
		{
			Trace.TraceInformation(_prefix + outputLine);
		}

		/// <summary>
		/// Trace this line as Warning.
		/// </summary>
		/// <param name="outputLine">The line to trace.</param>
		public override void TraceStandardError(string outputLine)
		{
			Trace.TraceWarning(_prefix + outputLine);
		}
	}
}
