using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// Traces output from a process to a string.
	/// </summary>
	public sealed class StringProcessOutputTracer : ProcessOutputTracer
	{
		private readonly StringBuilder _standardOutputBuilder = new StringBuilder();
		private readonly StringBuilder _standardErrorBuilder = new StringBuilder();

		/// <summary>
		/// Traces a standard output line.
		/// </summary>
		/// <param name="outputLine"></param>
		public override void TraceStandardOut(string outputLine)
		{
			_standardOutputBuilder.AppendLine(outputLine);
		}

		/// <summary>
		/// Traces a standard error line.
		/// </summary>
		/// <param name="outputLine"></param>
		public override void TraceStandardError(string outputLine)
		{
			_standardErrorBuilder.AppendLine(outputLine);
		}

		/// <summary>
		/// Gets the output emitted by the process up to this point.
		/// </summary>
		/// <returns></returns>
		public ProcessOutput GetOutputSoFar()
		{
			return new ProcessOutput(
				standardOutput: _standardOutputBuilder.ToString(),
				standardError: _standardErrorBuilder.ToString());
		}
	}
}
