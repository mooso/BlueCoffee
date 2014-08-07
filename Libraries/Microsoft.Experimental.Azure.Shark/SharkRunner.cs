using Microsoft.Experimental.Azure.JavaPlatform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Shark
{
	/// <summary>
	/// Runs a Shark server.
	/// </summary>
	public sealed class SharkRunner
	{
		private readonly string _jarsDirectory;
		private readonly string _confDirectory;
		private readonly string _javaHome;
		private readonly string _sharkHome;
		private readonly string _fakeHadoopHome;
		private readonly SharkConfig _config;

		/// <summary>
		/// Create a new runner.
		/// </summary>
		/// <param name="sharkHome">The directory to use for Shark home.</param>
		/// <param name="javaHome">The directory where Java is installed.</param>
		/// <param name="config">The configuration.</param>
		public SharkRunner(string sharkHome, string javaHome, SharkConfig config)
		{
			_sharkHome = sharkHome;
			_javaHome = javaHome;
			_config = config;
			_jarsDirectory = Path.Combine(_sharkHome, "lib");
			_confDirectory = Path.Combine(_sharkHome, "conf");
			_fakeHadoopHome = Path.Combine(_sharkHome, "hadoop");
		}

		/// <summary>
		/// Setup Elastic Search.
		/// </summary>
		public void Setup()
		{
			foreach (var dir in
				new[] { _jarsDirectory, _confDirectory, _fakeHadoopHome })
			{
				Directory.CreateDirectory(dir);
			}
			ExtractJars();
			_config.GetHiveConfigXml().Save(Path.Combine(_confDirectory, "hive-site.xml"));
		}

		/// <summary>
		/// Run Shark server 2.
		/// </summary>
		/// <param name="runContinuous">If set, this method will keep restarting the node whenver it exits and will never return.</param>
		/// <param name="monitor">Optional process monitor.</param>
		public void RunSharkServer2(bool runContinuous = true, ProcessMonitor monitor = null)
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "shark.SharkServer2";
			runner.RunClass(className,
				"",
				ClassPath(),
				maxMemoryMb: _config.MaxMemoryMb,
				extraJavaOptions: new[]
				{
					"-XX:+UseParNewGC",
					"-XX:+UseConcMarkSweepGC",
					"-XX:CMSInitiatingOccupancyFraction=75",
					"-XX:+UseCMSInitiatingOccupancyOnly",
				},
				defines: new Dictionary<string, string>
				{
					{ "hadoop.home.dir", _fakeHadoopHome },
				},
				runContinuous: runContinuous,
				monitor: monitor,
				environmentVariables: new Dictionary<string, string>()
				{
					{ "HIVE_SERVER2_THRIFT_PORT", _config.ServerPort.ToString() },
					{ "SPARK_HOME", _config.SparkHome },
					{ "MASTER", _config.SparkMaster },
				});
		}

		/// <summary>
		/// Runs the Shark CLI as a console application.
		/// </summary>
		/// <returns>The Shark CLI process.</returns>
		public Process RunSharkCli()
		{
			var runner = new JavaRunner(_javaHome);
			const string className = "shark.SharkCliDriver";
			return runner.RunClassAsConsole(className,
				"",
				ClassPath(),
				maxMemoryMb: _config.MaxMemoryMb,
				defines: new Dictionary<string, string>
				{
					{ "hadoop.home.dir", _fakeHadoopHome },
				},
				environmentVariables: new Dictionary<string, string>()
				{
					{ "HIVE_SERVER2_THRIFT_PORT", _config.ServerPort.ToString() },
				});
		}

		private IEnumerable<string> ClassPath()
		{
			return new[] { _confDirectory }
				.Concat(JavaRunner.GetClassPathForJarsInDirectories(_jarsDirectory));
		}

		private void ExtractResourceArchive(string resourceName, string targetDirectory)
		{
			using (var rawStream = GetType().Assembly.GetManifestResourceStream(
				"Microsoft.Experimental.Azure.Shark.Resources." + resourceName + ".zip"))
			using (var archive = new ZipArchive(rawStream))
			{
				archive.ExtractToDirectory(targetDirectory);
			}
		}

		private void ExtractJars()
		{
			ExtractResourceArchive("Jars", _jarsDirectory);
			var binDirectory = Directory.CreateDirectory(Path.Combine(_fakeHadoopHome, "bin"));
			const string winutils = "winutils.exe";
			File.Move(Path.Combine(_jarsDirectory, winutils), Path.Combine(binDirectory.FullName, winutils));
		}
	}
}
