using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	/// <summary>
	/// Definition for an appender for log4j.
	/// </summary>
	public sealed class AppenderDefinition
	{
		private readonly string _name;
		private readonly string _className;
		private readonly ImmutableDictionary<string, string> _properties;
		private const string Log4jApppenderPrefix = "log4j.appender";

		/// <summary>
		/// Create a new definition.
		/// </summary>
		/// <param name="name">The name of the appender.</param>
		/// <param name="className">The Java fully qualified class name of the appender.</param>
		/// <param name="properties">The properties associated with this definition.</param>
		/// <seealso cref="Microsoft.Experimental.Azure.JavaPlatform.Log4j.AppenderDefinitionFactory"/>
		public AppenderDefinition(string name, string className, IEnumerable<KeyValuePair<string, string>> properties)
		{
			_name = name;
			_className = className;
			_properties = properties.ToImmutableDictionary();
		}

		/// <summary>
		/// Name of the appender.
		/// </summary>
		public string Name { get { return _name; } }
		/// <summary>
		/// The Java fully qualified class name of the appender.
		/// </summary>
		public string ClassName { get { return _className; } }
		/// <summary>
		/// The properties associated with this definition.
		/// </summary>
		public ImmutableDictionary<string, string> Properties { get { return _properties; } }

		/// <summary>
		/// The log4j properties to use to get this definition.
		/// </summary>
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
