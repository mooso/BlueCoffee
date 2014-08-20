using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.CommonTestUtilities
{
	public static class TestConfigFile
	{
		public static IEnumerable<string> ReadFile(string name)
		{
			using (Stream resourceStream =
				typeof(TestConfigFile).Assembly.GetManifestResourceStream("Microsoft.Experimental.Azure.CommonTestUtilities." + name + ".txt"))
			{
				StreamReader reader = new StreamReader(resourceStream);
				string currentLine;
				while ((currentLine = reader.ReadLine()) != null)
				{
					currentLine = currentLine.Trim();
					if (currentLine.StartsWith("#")) // Comment
					{
						continue;
					}
					if (currentLine == "")
					{
						continue;
					}
					yield return currentLine;
				}
				reader.Close();
			}
		}
	}
}
