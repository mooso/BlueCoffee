using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	public class ChildLoggerDefinition : LoggerDefinition
	{
		private readonly string _classPrefix;
		private readonly bool _additivity;

		public ChildLoggerDefinition(string classPrefix, Log4jTraceLevel level, params AppenderDefinition[] appenders)
			: base(level, appenders)
		{
			_classPrefix = classPrefix;
			_additivity = true;
		}

		public ChildLoggerDefinition(string classPrefix, Log4jTraceLevel level, AppenderDefinition appender, bool additivity = true)
			: base(level, new[] { appender })
		{
			_classPrefix = classPrefix;
			_additivity = additivity;
		}

		public string ClassPrefix { get { return _classPrefix; } }
		public bool Additivity { get { return _additivity; } }

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
