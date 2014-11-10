using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ZooKeeper
{
	/// <summary>
	/// Factory class for a default ZooKeeper log4j configuration.
	/// </summary>
	public static class ZooKeeperLog4jConfigFactory
	{
		/// <summary>
		/// Creates the default configuration.
		/// </summary>
		/// <param name="logDirectory">Directory for the log files.</param>
		/// <returns>The configuration to use.</returns>
		public static Log4jConfig CreateConfig(string logDirectory)
		{
			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender();
			var fileAppender = AppenderDefinitionFactory.DailyRollingFileAppender("file", logDirectory.Replace('\\', '/') + "/zk.log");
			var rootLogger = new RootLoggerDefinition(Log4jTraceLevel.INFO, consoleAppender, fileAppender);
			return new Log4jConfig(rootLogger, new ChildLoggerDefinition[] {});
		}
	}
}
