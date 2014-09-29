using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.CommonTestUtilities
{
	public sealed class ProcessKiller : ProcessMonitor
	{
		private readonly List<Process> _processes = new List<Process>();
		private readonly object _listLock = new object();

		public void KillAll()
		{
			List<Process> copy;
			lock (_listLock)
			{
				copy = _processes.ToList();
			}
			Parallel.ForEach(copy, p => p.Kill());
		}

		public override void ProcessStarted(Process process)
		{
			lock (_listLock)
			{
				_processes.Add(process);
			}
			process.Disposed += (s, e) =>
			{
				lock (_listLock)
				{
					_processes.Remove(process);
				}
			};
		}
	}
}
