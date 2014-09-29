using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.JavaPlatform.Logback
{
	/// <summary>
	/// A logback logger definition that applies to classes with a given prefix.
	/// </summary>
	public class ChildLoggerDefinition : LoggerDefinition
	{
		private readonly string _name;
		private readonly bool _additivity;

		/// <summary>
		/// Create a new child definition.
		/// </summary>
		/// <param name="name">The name of the logger.</param>
		/// <param name="level">The trace level to use in this definition.</param>
		/// <param name="appenders">The appeners to use.</param>
		public ChildLoggerDefinition(string name, LogbackTraceLevel level, params AppenderDefinition[] appenders)
			: base(level, appenders)
		{
			_name = name;
			_additivity = true;
		}

		/// <summary>
		/// Create a new child definition.
		/// </summary>
		/// <param name="name">The name of the logger.</param>
		/// <param name="level">The trace level to use in this definition.</param>
		/// <param name="appender">The appender to use.</param>
		/// <param name="additivity">If false, this will not inherit the appenders of the parent definitions.</param>
		public ChildLoggerDefinition(string name, LogbackTraceLevel level, AppenderDefinition appender, bool additivity = true)
			: base(level, new[] { appender })
		{
			_name = name;
			_additivity = additivity;
		}

		/// <summary>
		/// The name of the logger.
		/// </summary>
		public string Name { get { return _name; } }

		/// <summary>
		/// If false, this will not inherit the appenders of the parent definitions.
		/// </summary>
		public bool Additivity { get { return _additivity; } }

		/// <summary>
		/// The XML definition
		/// </summary>
		public override XElement ToXml()
		{
			return new XElement("logger", new XObject[]
			{
				new XAttribute("name", _name),
				new XAttribute("additivity", _additivity.ToString()),
				new XElement("level",
					new XAttribute("value", Level.ToString())),
			}.Concat(CreateAppenderRefElements()));
		}
	}
}
