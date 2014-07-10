using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	public static class JavaInstaller
	{
		public static void ExtractJdk(string destination)
		{
			using (var rawStream = typeof(JavaInstaller).Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.JavaPlatform.Resources.openjdk7.zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(destination);
			}
		}
	}
}
