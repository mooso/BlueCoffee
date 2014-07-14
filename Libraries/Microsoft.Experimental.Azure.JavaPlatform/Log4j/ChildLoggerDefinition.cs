using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	/// <summary>
	/// A log4j logger definition that applies to classes with a given prefix.
	/// </summary>
	public class ChildLoggerDefinition : LoggerDefinition
	{
		private readonly string _classPrefix;
		private readonly bool _additivity;

		/// <summary>
		/// Create a new child definition.
		/// </summary>
		/// <param name="classPrefix">The prefix that classes with this definition must have.</param>
		/// <param name="level">The trace level to use in this definition.</param>
		/// <param name="appenders">The appeners to use.</param>
		public ChildLoggerDefinition(string classPrefix, Log4jTraceLevel level, params AppenderDefinition[] appenders)
			: base(level, appenders)
		{
			_classPrefix = classPrefix;
			_additivity = true;
		}

		/// <summary>
		/// Create a new child definition.
		/// </summary>
		/// <param name="classPrefix">The prefix that classes with this definition must have.</param>
		/// <param name="level">The trace level to use in this definition.</param>
		/// <param name="appender">The appender to use.</param>
		/// <param name="additivity">If false, this will not inherit the appenders of the parent definitions.</param>
		public ChildLoggerDefinition(string classPrefix, Log4jTraceLevel level, AppenderDefinition appender, bool additivity = true)
			: base(level, new[] { appender })
		{
			_classPrefix = classPrefix;
			_additivity = additivity;
		}

		/// <summary>
		/// The prefix that classes with this definition must have.
		/// </summary>
		public string ClassPrefix { get { return _classPrefix; } }

		/// <summary>
		/// If false, this will not inherit the appenders of the parent definitions.
		/// </summary>
		public bool Additivity { get { return _additivity; } }

		/// <summary>
		/// The log4j properties to use to get this definition.
		/// </summary>
		public override ImmutableDictionary<string, string> FullLog4jProperties
		{
			get
			{
				var properties = new Dictionary<string, string>();
				properties.Add("log4j.logger." + _classPrefix, DefinitionLine);
				if (!_additivity)
				{
					properties.Add("log4j.additivity." + _classPrefix, "false");
				}
				return properties.ToImmutableDictionary();
			}
		}
	}
}
