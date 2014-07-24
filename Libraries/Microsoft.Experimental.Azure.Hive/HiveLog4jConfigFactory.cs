using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	internal static class HiveLog4jConfigFactory
	{
		/// <summary>
		/// Creates the default configuration.
		/// </summary>
		/// <param name="logDirectory">Directory for the log files.</param>
		/// <returns>The configuration to use.</returns>
		public static Log4jConfig CreateConfig(string logDirectory)
		{
			var layout = LayoutDefinition.PatternLayout("[%d{ISO8601}][%-5p][%-25c] %m%n");

			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender("console",
				layout: layout);
			var fileAppender = AppenderDefinitionFactory.DailyRollingFileAppender("file",
				Path.Combine(logDirectory, "Hive.log"),
				layout: layout);

			var rootLogger = new RootLoggerDefinition(Log4jTraceLevel.INFO, consoleAppender, fileAppender);

			return new Log4jConfig(rootLogger, Enumerable.Empty<ChildLoggerDefinition>());
		}
	}
}
