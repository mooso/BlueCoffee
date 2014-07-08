using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public class JavaRunner
	{
		private readonly string _javaHome;
		private readonly string _javaExePath;

		public JavaRunner(string javaHome)
		{
			if (String.IsNullOrEmpty(javaHome))
			{
				throw new ArgumentNullException("javaHome");
			}
			_javaHome = javaHome;
			_javaExePath = Path.Combine(javaHome, "bin", "java.exe");
			if (!File.Exists(_javaExePath))
			{
				throw new ArgumentException("Java.exe not found in: " + _javaExePath);
			}
		}

		public static IEnumerable<String> GetClassPathForJarsInDirectories(params string[] directoryList)
		{
			return directoryList.SelectMany(d => Directory.EnumerateFiles(d, "*.jar"));
		}

		public int RunClass(string className, string arguments, IEnumerable<string> classPathEntries, int maxMemoryMb = 512, bool server = true, IDictionary<string, string> defines = null,
			ProcessOutputTracer tracer = null, bool runContinuous = true)
		{
			var simpleClassName = className.Split('.').Last();
			tracer = tracer ?? new DefaultProcessOutputTracer(simpleClassName + ": ");

			var javaToolArgumentList = new List<string>();
			javaToolArgumentList.Add("-cp " + String.Join(";", classPathEntries));
			if (defines != null)
			{
				javaToolArgumentList.Add(String.Join(" ", defines.Select((k, v) => String.Format(CultureInfo.InvariantCulture, "-D{0}={1}", k, v))));
			}
			javaToolArgumentList.Add(String.Format(CultureInfo.InvariantCulture, "-Xmx{0}M", maxMemoryMb));
			if (server)
			{
				javaToolArgumentList.Add("-server");
			}
			javaToolArgumentList.Add(className);
			javaToolArgumentList.Add(arguments);

			var javaStartInfo = new ProcessStartInfo()
			{
				Arguments = String.Join(" ", javaToolArgumentList),
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				FileName = _javaExePath
			};
			while (true)
			{
				Trace.TraceInformation("About to run: " + javaStartInfo.FileName + " " + javaStartInfo.Arguments);
				using (Process javaProcess = new Process() { StartInfo = javaStartInfo })
				{
					javaProcess.OutputDataReceived += (source, eventArgs) => tracer.TraceStandardOut(eventArgs.Data);
					javaProcess.ErrorDataReceived += (source, eventArgs) => tracer.TraceStandardError(eventArgs.Data);
					javaProcess.Start();
					javaProcess.BeginOutputReadLine();
					javaProcess.BeginErrorReadLine();
					javaProcess.WaitForExit(Int32.MaxValue);
					if (!runContinuous)
					{
						return javaProcess.ExitCode;
					}
					Trace.TraceInformation("Class " + className + " exited with code " + javaProcess.ExitCode + ". Restarting...");
				}
			}
		}
	}
}
