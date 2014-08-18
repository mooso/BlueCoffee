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
		public const string RootLibrariesPath = @"C:\BlueCoffee\Libraries"; // FIX THIS (somehow)
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
	}
}
