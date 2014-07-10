using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	public sealed class LayoutDefinition
	{
		private readonly string _className;
		private readonly ImmutableDictionary<string, string> _properties;

		public LayoutDefinition(string className, IEnumerable<KeyValuePair<string, string>> properties)
		{
			_className = className;
			_properties = properties.ToImmutableDictionary();
		}

		public string ClassName { get { return _className; } }
		public ImmutableDictionary<string, string> Properties { get { return _properties; } }

		public static LayoutDefinition PatternLayout(string pattern = "[%d] %p %m (%c)%n")
		{
			return new LayoutDefinition("org.apache.log4j.PatternLayout",
				new Dictionary<string, string>() { { "ConversionPattern", pattern } });
		}
	}
}
