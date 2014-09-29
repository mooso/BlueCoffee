using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.CommonTestUtilities
{
	public static class ConditionAwaiter
	{

		public static string SharedRead(string sharkLogFile)
		{
			using (var fileStream = new FileStream(sharkLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var textReader = new StreamReader(fileStream))
			{
				return textReader.ReadToEnd();
			}
		}

		public static void WaitForCondition(Func<bool> condition, TimeSpan? timeout = null)
		{
			var timer = Stopwatch.StartNew();
			do
			{
				if (condition())
				{
					return;
				}
				Thread.Sleep(100);
			} while (timer.Elapsed < (timeout ?? TimeSpan.FromSeconds(30)));
			Assert.Fail("Timed out.");
		}

		public static void WaitForLogSnippet(string logFilePath, string snippetToWaitFor, TimeSpan? timeout = null)
		{
			WaitForCondition(() => File.Exists(logFilePath), timeout);
			WaitForCondition(() => SharedRead(logFilePath).Contains(snippetToWaitFor), timeout);
		}
	}
}
