using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils.Log4j
{
	public sealed class AppenderDefinition
	{
		private readonly string _name;
		private readonly string _className;
		private readonly ImmutableDictionary<string, string> _properties;
		private const string Log4jApppenderPrefix = "log4j.appender";

		public AppenderDefinition(string name, string className, IEnumerable<KeyValuePair<string, string>> properties)
		{
			_name = name;
			_className = className;
			_properties = properties.ToImmutableDictionary();
		}

		public string Name { get { return _name; } }
		public string ClassName { get { return _className; } }
		public ImmutableDictionary<string, string> Properties { get { return _properties; } }

		public ImmutableDictionary<string, string> FullLog4jProperties
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ Log4jApppenderPrefix + "." + _name, _className }
				}
				.Concat(
					_properties
					.Select(kv => new KeyValuePair<string, string>(String.Join(".", Log4jApppenderPrefix, _name, kv.Key), kv.Value))
				).ToImmutableDictionary();
			}
		}
	}
}
