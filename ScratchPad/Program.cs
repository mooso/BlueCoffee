using JavaUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScratchPad
{
	class Program
	{
		static void Main(string[] args)
		{
			Trace.Listeners.Add(new ConsoleTraceListener());
			var javaHome = @"C:\TryExtractJdk\java";
			var jarsHome = @"C:\Kafka\kafka_2.9.2-0.8.1.1\libs";
			var logsDirectory = @"C:\Temp\KafkaLogs";
			var kafkaLog4jPropertiesPath = @"C:\Temp\log4j.properties";
			var config = new KafkaLog4jConfig(logsDirectory);
			config.WriteToFile(kafkaLog4jPropertiesPath);
			Directory.CreateDirectory(logsDirectory);
			var runner = new JavaRunner(javaHome);
			const string className = "kafka.Kafka";
			var classPathEntries = JavaRunner.GetClassPathForJarsInDirectories(jarsHome);
			runner.RunClass(className,
				@"C:\Kafka\kafka_2.9.2-0.8.1.1\config\server.properties",
				classPathEntries,
				defines: new Dictionary<string, string> { { "log4j.configuration", "file:\"" + kafkaLog4jPropertiesPath + "\"" } });
		}
	}
}
