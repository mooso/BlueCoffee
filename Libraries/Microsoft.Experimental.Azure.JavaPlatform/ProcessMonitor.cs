using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// A monitor class that watches processes being created.
	/// </summary>
	public abstract class ProcessMonitor
	{
		/// <summary>
		/// Notifies this monitor that a process has been started.
		/// </summary>
		/// <param name="process">The process started.</param>
		public abstract void ProcessStarted(Process process);
	}
}
