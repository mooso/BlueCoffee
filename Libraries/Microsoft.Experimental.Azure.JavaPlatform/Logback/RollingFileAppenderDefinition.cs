using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.JavaPlatform.Logback
{
	/// <summary>
	/// Definition for a rolling file appender.
	/// </summary>
	public sealed class RollingFileAppenderDefinition : AppenderDefinition
	{
		private readonly int _maxFileSizeMb;
		private readonly string _filePath;

		/// <summary>
		/// Creates a new definition.
		/// </summary>
		/// <param name="name">The name of the appender.</param>
		/// <param name="filePath">The path of the log file.</param>
		/// <param name="maxFileSizeMb">Maximum size before rolling.</param>
		/// <param name="pattern">Pattern to use for each log message.</param>
		public RollingFileAppenderDefinition(string name, string filePath,
			int maxFileSizeMb = 100, string pattern = null)
			: base(name, "ch.qos.logback.core.rolling.RollingFileAppender", pattern)
		{
			_filePath = filePath;
			_maxFileSizeMb = maxFileSizeMb;
		}

		/// <summary>
		/// The actual XML content of the appender.
		/// </summary>
		protected override IEnumerable<XElement> CreateXmlContent()
		{
			return new[]
			{
				new XElement("file", _filePath.Replace('\\', '/')),
				new XElement("rollingPolicy",
					new XAttribute("class", "ch.qos.logback.core.rolling.FixedWindowRollingPolicy"),
					new XElement("fileNamePattern", _filePath.Replace('\\', '/') + ".%i"),
					new XElement("minIndex", 1),
					new XElement("maxIndex", 9)),
				new XElement("triggeringPolicy",
					new XAttribute("class", "ch.qos.logback.core.rolling.SizeBasedTriggeringPolicy"),
					new XElement("maxFileSize", _maxFileSizeMb + "MB")),
			};
		}
	}
}
