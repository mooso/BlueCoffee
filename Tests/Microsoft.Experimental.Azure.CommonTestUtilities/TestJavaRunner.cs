using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.CommonTestUtilities
{
	public static class TestJavaRunner
	{
		public const string JavaHome = @"C:\Program Files\Java\jdk1.7.0_21";

		public static string CompileJarFromResource(Type testClass,
			string targetDirectory,
			string javaResourceName,
			string libDirectory,
			string targetJarName)
		{
			if (!Directory.Exists(targetDirectory))
			{
				Directory.CreateDirectory(targetDirectory);
			}
			// Write out the java file
			WriteResourceToFile(targetDirectory, javaResourceName, testClass);
			// Compile it
			RunTool(Path.Combine(JavaHome, "bin", "javac.exe"), String.Format(
				"-cp \"{0}\" -sourcepath \"{1}\" -d \"{1}\" {2}",
				Path.Combine(libDirectory, "*"), targetDirectory, Path.Combine(targetDirectory, javaResourceName)));
			// Jar it
			RunTool(Path.Combine(JavaHome, "bin", "jar.exe"), String.Format(
				"cf \"{0}\" -C \"{1}\" .",
				Path.Combine(targetDirectory, targetJarName), targetDirectory));
			return Path.Combine(targetDirectory, targetJarName);
		}

		public static ProcessOutput RunJavaResourceFile(Type testClass,
			string javaResourceName,
			string libDirectory,
			string arguments = "")
		{
			var targetDirectory = Path.Combine(Path.GetTempPath(), "JavaRunTemp-" + Guid.NewGuid());
			if (!Directory.Exists(targetDirectory))
			{
				Directory.CreateDirectory(targetDirectory);
			}
			try
			{
				// Write out the java file
				WriteResourceToFile(targetDirectory, javaResourceName, testClass);
				// Compile it
				RunTool(Path.Combine(JavaHome, "bin", "javac.exe"), String.Format(
					"-cp \"{0}\" -sourcepath \"{1}\" -d \"{1}\" {2}",
					Path.Combine(libDirectory, "*"), targetDirectory, Path.Combine(targetDirectory, javaResourceName)));
				// Run it
				var runner = new JavaRunner(JavaHome);
				var tracer = new StringProcessOutputTracer();
				var exitCode = runner.RunClass(
					className: Path.GetFileNameWithoutExtension(javaResourceName),
					arguments: arguments,
					classPathEntries: new[] { Path.Combine(libDirectory, "*"), targetDirectory },
					tracer: tracer,
					runContinuous: false);
				return tracer.GetOutputSoFar();
			}
			finally
			{
				Directory.Delete(targetDirectory, recursive: true);
			}
		}

		public static void WriteResourceToFile(string outputDirectory, string fileName, Type testClass)
		{
			var javaFilePath = Path.Combine(outputDirectory, fileName);
			using (var resourceStream =
				testClass.Assembly.GetManifestResourceStream(testClass.Namespace + "." + fileName))
			using (var javaFileStream = File.OpenWrite(javaFilePath))
			{
				var buffer = new byte[1024];
				int numBytesRead;
				while ((numBytesRead = resourceStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					javaFileStream.Write(buffer, 0, numBytesRead);
				}
			}
		}

		public static void RunTool(string toolPath, string args)
		{
			Trace.TraceInformation("Starting: {0} {1}", toolPath, args);
			var processStartInfo = new ProcessStartInfo(toolPath, args)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
			};
			var stdOut = new StringBuilder();
			var stdErr = new StringBuilder();
			int exitCode;
			using (var process = new Process() { StartInfo = processStartInfo })
			{
				process.OutputDataReceived += (source, eventArgs) => stdOut.AppendLine(eventArgs.Data);
				process.ErrorDataReceived += (source, eventArgs) => stdErr.AppendLine(eventArgs.Data);
				process.Start();
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();
				if (!process.WaitForExit(10 * 1000))
				{
					process.Kill();
					Assert.Fail("Timed out waiting for {0} to exit. Std Out:\n{1}\n Std Err:\n{2}",
						toolPath, stdOut.ToString(), stdErr.ToString());
				}
				exitCode = process.ExitCode;
			}
			Assert.AreEqual(0, exitCode,
				"Process {0} exited with non-zero exit code. Std Out:\n{1}\n Std Err:\n{2}",
					toolPath, stdOut.ToString(), stdErr.ToString());
		}
	}
}
