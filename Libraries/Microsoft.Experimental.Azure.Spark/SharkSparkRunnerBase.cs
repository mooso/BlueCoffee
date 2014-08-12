using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Spark
{
	/// <summary>
	/// Base class for Shark and Spark runners.
	/// </summary>
	public abstract class SharkSparkRunnerBase
	{
		private readonly string _jarsDirectory;
		private readonly string _binDirectory;
		private readonly string _confDirectory;
		private readonly string _logsDirectory;
		private readonly string _javaHome;
		private readonly string _homeDirectory;
		private readonly string _fakeHadoopHome;
		private const string _log4jPropertiesFileName = "log4j.properties";
		private readonly Log4jTraceLevel _traceLevel;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="homeDirectory">The directory to use for home.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		/// <param name="traceLevel">The trace level to use.</param>
		protected SharkSparkRunnerBase(string homeDirectory, string javaHome,
			Log4jTraceLevel traceLevel = Log4jTraceLevel.INFO)
		{
			_homeDirectory = homeDirectory;
			_javaHome = javaHome;
			_traceLevel = traceLevel;
			_jarsDirectory = Path.Combine(_homeDirectory, "lib");
			_binDirectory = Path.Combine(_homeDirectory, "bin");
			_confDirectory = Path.Combine(_homeDirectory, "conf");
			_logsDirectory = Path.Combine(_homeDirectory, "logs");
			_fakeHadoopHome = Path.Combine(_homeDirectory, "hadoop");
		}

		/// <summary>
		/// Setup Shark or Spark.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in
				new[] { _jarsDirectory, _binDirectory, _confDirectory, _fakeHadoopHome, _logsDirectory })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			WriteConfig();
			CreateLog4jConfig().ToPropertiesFile().WriteToFile(Path.Combine(_confDirectory, _log4jPropertiesFileName));
		}

		/// <summary>
		/// Write the Shark/Spark-specific configuration.
		/// </summary>
		protected abstract void WriteConfig();

		/// <summary>
		/// The log file name.
		/// </summary>
		protected abstract string LogFileName { get; }

		/// <summary>
		/// Creates a new JavaRunner.
		/// </summary>
		/// <returns>The JavaRunner.</returns>
		protected JavaRunner CreateJavaRunner()
		{
			return new JavaRunner(_javaHome);
		}

		/// <summary>
		/// The Java system properties that indicate the Log4j file and the Hadoop home directory.
		/// </summary>
		protected ImmutableDictionary<string, string> HadoopHomeAndLog4jDefines
		{
			get
			{
				return new Dictionary<string, string>
				{
					{
						"log4j.configuration",
						"file:\"" +
							Path.Combine(_confDirectory, _log4jPropertiesFileName) +
							"\""
					},
					{ "hadoop.home.dir", _fakeHadoopHome },
				}.ToImmutableDictionary();
			}
		}

		/// <summary>
		/// The home directory.
		/// </summary>
		protected string HomeDirectory { get { return _homeDirectory; } }

		/// <summary>
		/// The jars directory.
		/// </summary>
		protected string JarsDirectory { get { return _jarsDirectory; } }

		/// <summary>
		/// The fake Hadoop home under which winutils.exe lives
		/// </summary>
		protected string FakeHadoopHome { get { return _fakeHadoopHome; } }

		/// <summary>
		/// JavaHome.
		/// </summary>
		protected string JavaHome { get { return _javaHome; } }

		/// <summary>
		/// The configuration directory.
		/// </summary>
		protected string ConfDirectory { get { return _confDirectory; } }

		/// <summary>
		/// The common class path.
		/// </summary>
		/// <returns>The class path.</returns>
		protected IEnumerable<string> ClassPath()
		{
			return new[] { _confDirectory }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory));
		}

		private Log4jConfig CreateLog4jConfig()
		{
			var layout = LayoutDefinition.PatternLayout("[%d{ISO8601}][%-5p][%-25c] %m%n");

			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender("console",
				layout: layout);
			var fileAppender = AppenderDefinitionFactory.DailyRollingFileAppender("file",
				Path.Combine(_logsDirectory, LogFileName),
				layout: layout);

			var rootLogger = new RootLoggerDefinition(_traceLevel, consoleAppender, fileAppender);

			return new Log4jConfig(rootLogger, Enumerable.Empty<ChildLoggerDefinition>());
		}

		private static void ExtractResourceArchive(string resourceName, string targetDirectory)
		{
			using (var rawStream = typeof(SharkRunner).Assembly.GetManifestResourceStream(
				"Microsoft.Experimental.Azure.Spark.Resources." + resourceName + ".zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				foreach (var entry in archive.Entries)
				{
					var targetFile = Path.Combine(targetDirectory, entry.FullName);
					if (!File.Exists(targetFile))
					{
						entry.ExtractToFile(targetFile);
					}
				}
			}
		}

		private void ExtractJars()
		{
			ExtractResourceArchive("Jars", _jarsDirectory);
			const string classPathScriptName = "compute-classpath.cmd";
			File.Move(
				Path.Combine(_jarsDirectory, classPathScriptName),
				Path.Combine(_binDirectory, classPathScriptName));
			File.WriteAllBytes(Path.Combine(_homeDirectory, "RELEASE"), new byte[] { });
			var binDirectory = Directory.CreateDirectory(Path.Combine(_fakeHadoopHome, "bin"));
			const string winutils = "winutils.exe";
			File.Move(Path.Combine(_jarsDirectory, winutils), Path.Combine(binDirectory.FullName, winutils));
		}
	}
}
