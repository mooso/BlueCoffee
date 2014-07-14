using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	/// <summary>
	/// Definition of a layout for log4j messages.
	/// </summary>
	public sealed class LayoutDefinition
	{
		private readonly string _className;
		private readonly ImmutableDictionary<string, string> _properties;

		/// <summary>
		/// Create a new layout definition.
		/// </summary>
		/// <param name="className">The Java fully qualified class name of the layout.</param>
		/// <param name="properties">The properties associated with this layout.</param>
		/// <remarks>
		/// If possible you should use the factory methods on this class instead of this constructor.
		/// </remarks>
		public LayoutDefinition(string className, IEnumerable<KeyValuePair<string, string>> properties)
		{
			_className = className;
			_properties = properties.ToImmutableDictionary();
		}

		/// <summary>
		/// The Java fully qualified class name of the layout.
		/// </summary>
		public string ClassName { get { return _className; } }
		/// <summary>
		/// The properties associated with this layout.
		/// </summary>
		public ImmutableDictionary<string, string> Properties { get { return _properties; } }

		/// <summary>
		/// Create a new pattern layout.
		/// </summary>
		/// <param name="pattern">The pattern to use.</param>
		/// <returns>The definition.</returns>
		public static LayoutDefinition PatternLayout(string pattern = "[%d] %p %m (%c)%n")
		{
			return new LayoutDefinition("org.apache.log4j.PatternLayout",
				new Dictionary<string, string>() { { "ConversionPattern", pattern } });
		}
	}
}
