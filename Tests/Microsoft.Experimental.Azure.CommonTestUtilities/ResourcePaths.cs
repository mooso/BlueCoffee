using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.CommonTestUtilities
{
	public static class ResourcePaths
	{
		public static readonly string RootLibrariesPath = Path.Combine(FindRootSolutionDirectory(), "Libraries");
		public static readonly string JavaPlatformResourcesPath =
			Path.Combine(RootLibrariesPath,
			"Microsoft.Experimental.Azure.JavaPlatform", "Resources");
		public static readonly string CassandraResourcesPath =
			Path.Combine(RootLibrariesPath,
			"Microsoft.Experimental.Azure.Cassandra", "Resources");
		public static readonly string ElasticSearchResourcesPath =
			Path.Combine(RootLibrariesPath,
			"Microsoft.Experimental.Azure.ElasticSearch", "Resources");
		public static readonly string HiveResourcesPath =
			Path.Combine(RootLibrariesPath,
			"Microsoft.Experimental.Azure.Hive", "Resources");
		public static readonly string PrestoResourcesPath =
			Path.Combine(RootLibrariesPath,
			"Microsoft.Experimental.Azure.Presto", "Resources");
		public static readonly string SparkResourcesPath =
			Path.Combine(RootLibrariesPath,
			"Microsoft.Experimental.Azure.Spark", "Resources");

		private static string FindRootSolutionDirectory()
		{
			var appDomainBase = AppDomain.CurrentDomain.BaseDirectory;
			for (var currentParent = Directory.GetParent(appDomainBase); currentParent != null; currentParent = currentParent.Parent)
			{
				if (currentParent.GetFiles("BlueCoffee.sln").Any())
				{
					return currentParent.FullName;
				}
			}
			throw new InvalidOperationException("Can't find the root of the solution.");
		}
	}
}
