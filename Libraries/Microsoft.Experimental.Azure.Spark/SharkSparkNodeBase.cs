using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
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
	}
}
