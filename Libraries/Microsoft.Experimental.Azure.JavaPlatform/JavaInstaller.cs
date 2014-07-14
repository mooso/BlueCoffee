using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// This class lays down the Open JDK binaries that can be used to run Java programs.
	/// </summary>
	public sealed class JavaInstaller
	{
		private readonly string _installDirectory;

		/// <summary>
		/// Creates a new installer.
		/// </summary>
		/// <param name="installDirectory">The directory in which to lay down the bits.</param>
		public JavaInstaller(string installDirectory)
		{
			_installDirectory = installDirectory;
		}

		/// <summary>
		/// Extract the Open JDK binaries into the destination directory.
		/// </summary>
		public void Setup()
		{
			using (var rawStream = typeof(JavaInstaller).Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.JavaPlatform.Resources.openjdk7.zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(_installDirectory);
			}
		}

		/// <summary>
		/// The JAVA_HOME directory to use when running Java programs after Setup().
		/// </summary>
		public string JavaHome
		{
			get { return Path.Combine(_installDirectory, "java"); }
		}
	}
}
