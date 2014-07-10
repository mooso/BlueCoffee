using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	public sealed class JavaInstaller
	{
		private readonly string _installDirectory;

		public JavaInstaller(string installDirectory)
		{
			_installDirectory = installDirectory;
		}

		public void Setup()
		{
			using (var rawStream = typeof(JavaInstaller).Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.JavaPlatform.Resources.openjdk7.zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(_installDirectory);
			}
		}

		public string JavaHome
		{
			get { return Path.Combine(_installDirectory, "java"); }
		}
	}
}
