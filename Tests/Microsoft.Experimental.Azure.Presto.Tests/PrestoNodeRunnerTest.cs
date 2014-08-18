using Cassandra;
using Microsoft.Experimental.Azure.Cassandra;
using Microsoft.Experimental.Azure.CommonTestUtilities;
using Microsoft.Experimental.Azure.Hive;
using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Presto.Tests
{
	[TestClass]
	public class PrestoNodeRunnerTest
	{
		private const string JavaHome = @"C:\Program Files\Java\jdk1.7.0_21";

		[TestMethod]
		[Ignore]
		public void EndToEndTest()
		{
			var tempDirectory = @"C:\PrestoTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var catalogs = Enumerable.Empty<PrestoCatalogConfig>();
			var runner = SetupPresto(tempDirectory, catalogs);
			runner.Run(runContinuous: false);
		}

		[TestMethod]
		[Ignore]
		public void PrestoWithCassandraEndToEndTest()
		{
			var tempDirectory = @"C:\PrestoWithCassandraTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var cassandraRoot = Path.Combine(tempDirectory, "CassandraRoot");
			var prestoRoot = Path.Combine(tempDirectory, "Presto");
			const string cassandraNode = "127.0.0.1";
			var cassandraRunner = SetupCassandra(cassandraRoot, cassandraNode);
			var cassandraTask = Task.Factory.StartNew(() => cassandraRunner.Run(runContinuous: false));
			// Wait for Cassandra to start up
			CreateSampleTable(cassandraNode);
			var prestoRunner = SetupPresto(prestoRoot,
				new[] { new PrestoCassandraCatalogConfig(new[] { cassandraNode }) });
			var prestoTask = Task.Factory.StartNew(() => prestoRunner.Run(runContinuous: false));

			Task.WaitAll(cassandraTask, prestoTask);
		}

		[TestMethod]
		[Ignore]
		public void PrestoWithHiveEndToEndTest()
		{
			var tempDirectory = @"C:\PrestoWithHiveTestOutput";
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
			var hiveRoot = Path.Combine(tempDirectory, "HiveRoot");
			var prestoRoot = Path.Combine(tempDirectory, "Presto");
			var hiveRunner = SetupHive(hiveRoot);
			var metastoreConfig = new HiveDerbyMetastoreConfig(
				derbyDataDirectory: Path.Combine(hiveRoot, "metastore"));
			var hiveTask = Task.Factory.StartNew(() => hiveRunner.RunMetastore(metastoreConfig, runContinuous: false));
			var prestoRunner = SetupPresto(prestoRoot,
				new[] { new PrestoHiveCatalogConfig("thrift://localhost:9083") });
			var prestoTask = Task.Factory.StartNew(() => prestoRunner.Run(runContinuous: false));

			Task.WaitAll(hiveTask, prestoTask);
		}

		private static PrestoNodeRunner SetupPresto(string prestoRoot, IEnumerable<PrestoCatalogConfig> catalogs)
		{
			var config = new PrestoConfig(
				nodeId: "testnode",
				dataDirectory: Path.Combine(prestoRoot, "data"),
				pluginConfigDirectory: Path.Combine(prestoRoot, "etc"),
				pluginInstallDirectory: Path.Combine(prestoRoot, "plugin"),
				discoveryServerUri: "http://localhost:8080",
				catalogs: catalogs);
			var runner = new PrestoNodeRunner(
				resourceFileDirectory: ResourcePaths.PrestoResourcesPath,
				jarsDirectory: Path.Combine(prestoRoot, "jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(prestoRoot, "logs"),
				configDirectory: Path.Combine(prestoRoot, "conf"),
				config: config,
				traceLevel: Log4jTraceLevel.DEBUG);
			runner.Setup();
			return runner;
		}

		private static HiveRunner SetupHive(string hiveRoot)
		{
			var runner = new HiveRunner(
				resourceFileDirectory: ResourcePaths.HiveResourcesPath,
				jarsDirectory: Path.Combine(hiveRoot, "jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(hiveRoot, "logs"),
				configDirectory: Path.Combine(hiveRoot, "conf"));
			runner.Setup();
			return runner;
		}

		private static CassandraNodeRunner SetupCassandra(string cassandraRoot, string cassandraNode)
		{
			var cassandraConfig = new CassandraConfig(
				clusterName: "Test cluster",
				clusterNodes: new[] { cassandraNode },
				dataDirectories: new[] { Path.Combine(cassandraRoot, "data") },
				commitLogDirectory: Path.Combine(cassandraRoot, "commitlog"),
				savedCachesDirectory: Path.Combine(cassandraRoot, "savedcaches"));
			var cassandraRunner = new CassandraNodeRunner(
				resourceFileDirectory: ResourcePaths.CassandraResourcesPath,
				jarsDirectory: Path.Combine(cassandraRoot, "jars"),
				javaHome: JavaHome,
				logsDirctory: Path.Combine(cassandraRoot, "logs"),
				configDirectory: Path.Combine(cassandraRoot, "conf"),
				config: cassandraConfig);
			cassandraRunner.Setup();
			return cassandraRunner;
		}

		private static void CreateSampleTable(string cassandraNode)
		{
			var builder = Cluster.Builder()
				.AddContactPoints(cassandraNode)
				.WithPort(9042)
				.WithDefaultKeyspace("sample_keyspace");
			var cluster = builder.Build();
			using (var session = ConnectToCluster(cluster))
			{
				session.Execute("create table if not exists sampletable (uid int primary key)");
				for (int i = 0; i < 100; i++)
				{
					session.Execute("insert into sampletable (uid) values (" + i + ")");
				}
			}
		}

		private static ISession ConnectToCluster(Cluster cluster)
		{
			var timer = Stopwatch.StartNew();
			while (true)
			{
				try
				{
					return cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
				}
				catch (NoHostAvailableException)
				{
					if (timer.Elapsed > TimeSpan.FromSeconds(5))
					{
						throw;
					}
				}
			}
		}
	}
}
