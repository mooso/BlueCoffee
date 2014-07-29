using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// This class can be used to run Java programs and tools.
	/// </summary>
	public class JavaRunner
	{
		private readonly string _javaHome;
		private readonly string _javaExePath;

		/// <summary>
		/// Creates a new runner.
		/// </summary>
		/// <param name="javaHome">The JAVA_HOME to use.</param>
		/// <seealso cref="Microsoft.Experimental.Azure.JavaPlatform.JavaInstaller"/>
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

		/// <summary>
		/// Helper method to get all the jar files in a given list of directories as classpath entries.
		/// </summary>
		/// <param name="directoryList">The directories to search for jar files.</param>
		/// <returns>The classpath entry list.</returns>
		public static IEnumerable<String> GetClassPathForJarsInDirectories(params string[] directoryList)
		{
			return directoryList.SelectMany(d => Directory.EnumerateFiles(d, "*.jar"));
		}

		/// <summary>
		/// Runs a Java class as a separate program.
		/// </summary>
		/// <param name="className">The fully qualified name of the Java class to run.</param>
		/// <param name="arguments">The command-line arguments given to the Java class.</param>
		/// <param name="classPathEntries">The class path to use.</param>
		/// <param name="maxMemoryMb">Maximum memory allowed for the Java virtual machine.</param>
		/// <param name="server">If set, we use the server flag for the Java virtual machine.</param>
		/// <param name="defines">List of system properties to define.</param>
		/// <param name="extraJavaOptions">Any other command-line arguments to give to the Java virtual machine.</param>
		/// <param name="tracer">The output tracer to use to trace the output of the program as it's running.</param>
		/// <param name="runContinuous">If set, we will restart the program any time it exits.</param>
		/// <param name="monitor">If given, this monitor will be notified when the Java process is started.</param>
		/// <param name="environmentVariables">The environment variables to use for the Java process.</param>
		/// <returns></returns>
		public int RunClass(string className, string arguments, IEnumerable<string> classPathEntries, int maxMemoryMb = 512, bool server = true,
			IEnumerable<KeyValuePair<string, string>> defines = null,
			IEnumerable<string> extraJavaOptions = null,
			ProcessOutputTracer tracer = null, bool runContinuous = true,
			ProcessMonitor monitor = null,
			IEnumerable<KeyValuePair<string, string>> environmentVariables = null)
		{
			var simpleClassName = className.Split('.').Last();
			tracer = tracer ?? new DefaultProcessOutputTracer(simpleClassName + ": ");

			var javaToolArgumentList = new List<string>();
			javaToolArgumentList.Add("-cp " + String.Join(";", classPathEntries));
			if (defines != null)
			{
				javaToolArgumentList.Add(String.Join(" ", defines.Select(FormatDefineString)));
			}
			javaToolArgumentList.Add(String.Format(CultureInfo.InvariantCulture, "-Xmx{0}M", maxMemoryMb));
			if (extraJavaOptions != null)
			{
				javaToolArgumentList.AddRange(extraJavaOptions);
			}
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
			if (environmentVariables != null)
			{
				foreach (var variable in environmentVariables)
				{
					if (javaStartInfo.EnvironmentVariables.ContainsKey(variable.Key))
					{
						javaStartInfo.EnvironmentVariables[variable.Key] = variable.Value;
					}
					else
					{
						javaStartInfo.EnvironmentVariables.Add(variable.Key, variable.Value);
					}
				}
			}
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
					if (monitor != null)
					{
						monitor.ProcessStarted(javaProcess);
					}
					javaProcess.WaitForExit(Int32.MaxValue);
					if (!runContinuous)
					{
						return javaProcess.ExitCode;
					}
					Trace.TraceInformation("Class " + className + " exited with code " + javaProcess.ExitCode + ". Restarting...");
				}
			}
		}

		private static string FormatDefineString(KeyValuePair<string, string> kv)
		{
			if (kv.Value == null)
			{
				return String.Format(CultureInfo.InvariantCulture, "-D{0}", kv.Key);
			}
			else
			{
				return String.Format(CultureInfo.InvariantCulture, "-D{0}={1}", kv.Key, kv.Value);
			}
		}
	}
}
