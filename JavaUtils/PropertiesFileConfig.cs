using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public abstract class PropertiesFileConfig
	{
		private readonly ImmutableDictionary<string, string> _configEntries;

		protected PropertiesFileConfig(IDictionary<string, string> configEntires)
		{
			_configEntries = configEntires.ToImmutableDictionary();
		}

		public ImmutableDictionary<string, string> ConfigEntries
		{
			get { return _configEntries; }
		}

		public void WriteToFile(string configFilePath)
		{
			File.WriteAllText(configFilePath,
				String.Join("\n", _configEntries.Select(kv => String.Join("=", kv.Key, kv.Value))),
				Encoding.ASCII);
		}
	}
}
