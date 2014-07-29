using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// The output from a Console application.
	/// </summary>
	public sealed class ProcessOutput
	{
		private readonly string _standardOutput;
		private readonly string _standardError;

		/// <summary>
		/// Constructs the output.
		/// </summary>
		/// <param name="standardOutput">The standard output from the process.</param>
		/// <param name="standardError">The standard error from the process.</param>
		public ProcessOutput(string standardOutput, string standardError)
		{
			_standardOutput = standardOutput;
			_standardError = standardError;
		}

		/// <summary>
		/// The standard output from the process.
		/// </summary>
		public string StandardOutput { get { return _standardOutput; } }

		/// <summary>
		/// The standard error from the process.
		/// </summary>
		public string StandardError { get { return _standardError; } }
	}
}
