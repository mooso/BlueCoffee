using DarkNotes;
using Microsoft.Experimental.Azure.JavaPlatform;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SharkFrontEnd
{
	public class HiveDataSource
	{
		private static Lazy<HiveDataSource> _instance = new Lazy<HiveDataSource>(Initialize);
		private readonly DarkJava _java;
		private readonly dynamic _driver;
		private readonly dynamic _emptyProperties;
		private readonly string _jdbcUri;
		private const string JavaPlatformDirectory = "JavaPlatform";
		private const string SparkDirectory = "Spark";

		private HiveDataSource(DarkJava java, string serverIP, dynamic driver)
		{
			_java = java;
			_jdbcUri = String.Format("jdbc:hive2://{0}:8082", serverIP);
			_driver = driver;
			dynamic dynJava = java;
			_emptyProperties = dynJava.java.util.Properties.@new();
		}

		private static HiveDataSource Initialize()
		{
			var installer = new JavaAzureInstaller();
			installer.DownloadResources(new[] { installer.JavaPlatformDirectory, "Spark" });
			installer.InstallJava();
			var sparkRoot = Path.Combine(installer.InstallDirectory, "Spark");
			installer.ExtractResourceArchive("Spark", "Jars.zip", sparkRoot);
			var sharkNode = RoleEnvironment.IsEmulated ? "localhost" :
				RoleEnvironment.Roles["Shark"].Instances[0].InstanceEndpoints.First().Value.IPEndpoint.Address.ToString();
			return HiveDataSource.Initialize(javaRoot: installer.JavaHome, sparkRoot: sparkRoot, serverIP: sharkNode);
		}

		private static HiveDataSource Initialize(string javaRoot, string sparkRoot, string serverIP)
		{
			var allJars = String.Join(";", Directory.EnumerateFiles(sparkRoot));
			dynamic java = DarkJava.CreateVm(
				jvmDllPath: Path.Combine(javaRoot, "java", "jre", "bin", "server", "jvm.dll"),
				options: new[] { JavaOption.DefineProperty("java.class.path", allJars) });
			// Import some packages
			java.ImportPackage("java.lang");
			java.ImportPackage("java.sql");
			// Initialize the driver
			var driver = java.org.apache.hive.jdbc.HiveDriver.@new();
			return new HiveDataSource(java, serverIP, driver);
		}

		public static HiveDataSource Instance { get { return _instance.Value; } }

		public List<dynamic> ExecuteQuery(string query)
		{
			dynamic conn = _driver.connect(_jdbcUri, _emptyProperties);
			dynamic statement = conn.createStatement();
			dynamic resultSet = statement.executeQuery(query);
			dynamic metadata = resultSet.getMetaData();
			var returnList = new List<dynamic>();
			int columnCount = (int)metadata.getColumnCount();
			string[] columnNames = new string[columnCount];
			for (int i = 0; i < columnCount; i++)
			{
				columnNames[i] = (string)metadata.getColumnName(i + 1);
			}
			while ((bool)resultSet.next())
			{
				var newRow = (IDictionary<string, object>)new ExpandoObject();
				for (int i = 0; i < columnCount; i++)
				{
					newRow[columnNames[i]] = (string)resultSet.getObject(i + 1).toString();
				}
				returnList.Add(newRow);
			}
			try
			{
				conn.close();
			}
			catch (Exception ex)
			{
				Trace.TraceWarning("Error while closing: " + ex);
			}
			return returnList;
		}
	}
}