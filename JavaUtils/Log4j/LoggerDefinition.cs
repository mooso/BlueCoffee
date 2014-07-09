using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils.Log4j
{
	public abstract class LoggerDefinition
	{
		private readonly Log4jTraceLevel _level;
		private readonly AppenderDefinition _appender;

		protected LoggerDefinition(Log4jTraceLevel level, AppenderDefinition appender)
		{
			_level = level;
			_appender = appender;
		}

		public Log4jTraceLevel Level { get { return _level; } }
		public AppenderDefinition Appender { get { return _appender; } }

		protected string DefinitionLine
		{
			get
			{
				return _level.ToString() + ", " + _appender.Name;
			}
		}

		public abstract ImmutableDictionary<string, string> FullLog4jProperties { get; }
	}
}
