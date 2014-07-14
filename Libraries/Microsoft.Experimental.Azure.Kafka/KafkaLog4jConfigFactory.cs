using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Kafka
{
	/// <summary>
	/// Factory class for default Kafka broker log4j configuration.
	/// </summary>
	public static class KafkaLog4jConfigFactory
	{
		private const string LogDirectoryPropertyName = "kafka.logs.dir";

		/// <summary>
		/// Create the configuration.
		/// </summary>
		/// <param name="logDirectory">The directory to use for logs.</param>
		/// <returns></returns>
		public static Log4jConfig CreateConfig(string logDirectory)
		{
			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender();
			var kafkaAppender = QualifiedFileAppender("kafkaAppender", "server.log");
			var stateChangeAppender = QualifiedFileAppender("stateChangeAppender", "state-change.log");
			var requestAppender = QualifiedFileAppender("requestAppender", "kafka-request.log");
			var cleanerAppender = QualifiedFileAppender("cleanerAppender", "log-cleaner.log");
			var controllerAppender = QualifiedFileAppender("controllerAppender", "controller.log");
			var rootLogger = new RootLoggerDefinition(Log4jTraceLevel.INFO, consoleAppender);
			var childLoggers = new[]
				{
					new ChildLoggerDefinition("kafka", Log4jTraceLevel.INFO, kafkaAppender),
					new ChildLoggerDefinition("kafka.network.RequestChannel$", Log4jTraceLevel.WARN, requestAppender, false),
					new ChildLoggerDefinition("kafka.request.logger", Log4jTraceLevel.WARN, requestAppender, false),
					new ChildLoggerDefinition("kafka.controller", Log4jTraceLevel.TRACE, controllerAppender, false),
					new ChildLoggerDefinition("kafka.log.LogCleaner", Log4jTraceLevel.INFO, cleanerAppender, false),
					new ChildLoggerDefinition("kafka.state.change.logger", Log4jTraceLevel.TRACE, stateChangeAppender, false),
				};
			return new Log4jConfig(rootLogger, childLoggers, new Dictionary<string, string>()
				{
					{ LogDirectoryPropertyName, logDirectory.Replace('\\', '/') }
				});
		}

		private static AppenderDefinition QualifiedFileAppender(string name, string fileName)
		{
			return AppenderDefinitionFactory.DailyRollingFileAppender(name, "${" + LogDirectoryPropertyName + "}/" + fileName);
		}
	}
}
