using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public sealed class KafkaLog4jConfig
	{
		private readonly string _logDirectory;

		public KafkaLog4jConfig(string logDirectory)
		{
			_logDirectory = logDirectory;
		}

		public void WriteToFile(string configFilePath)
		{
			// TODO: actually construct the log4j file from first principles instead of using a sample file.
			string sampleLog4j;
			using (var rawStream = typeof(JavaInstaller).Assembly.GetManifestResourceStream("JavaUtils.Resources.log4j.properties"))
			using (var textStream = new StreamReader(rawStream, Encoding.ASCII))
			{
				sampleLog4j = textStream.ReadToEnd();
			}
			var finalLog4j = sampleLog4j.Replace("kafka.logs.dir=logs", "kafka.logs.dir=" + _logDirectory);
			File.WriteAllText(configFilePath, finalLog4j, Encoding.ASCII);
		}
	}
}
