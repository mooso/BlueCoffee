using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// The base class for a typical Azure Shark/Spark node.
	/// </summary>
	public abstract class SharkSparkNodeBase : NodeWithJavaBase
	{
		private const string SparkDirectory = "Spark";

		/// <summary>
		/// The resource directories to download.
		/// </summary>
		protected override IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				return new[] { SparkDirectory }.Concat(base.ResourceDirectoriesToDownload);
			}
		}

		/// <summary>
		/// The directory where Spark resources are installed.
		/// </summary>
		protected string SparkResourceDirectory
		{
			get
			{
				return GetResourcesDirectory(SparkDirectory);
			}
		}

		/// <summary>
		/// The memory bound on each standalone executor.
		/// </summary>
		protected virtual int ExecutorMemoryMb
		{
			get
			{
				// By default the Worker in Spark uses (MachineMemory - 1024) MB for its total memory allocation.
				return Math.Max(512, (MachineTotalMemoryMb - 1024) / Environment.ProcessorCount);
			}
		}

		/// <summary>
		/// Other Spark properties than what's explicitly given.
		/// </summary>
		public virtual ImmutableDictionary<string, string> ExtraSparkProperties
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// The total memory on the machine in MB.
		/// </summary>
		protected static int MachineTotalMemoryMb
		{
			get
			{
				return (int)(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / (1024 * 1024));
			}
		}
	}
}
