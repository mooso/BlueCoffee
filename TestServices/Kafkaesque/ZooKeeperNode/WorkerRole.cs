using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Microsoft.Experimental.Azure.JavaPlatform;
using System.IO.Compression;
using Microsoft.Experimental.Azure.ZooKeeper;

namespace Zookeeper
{
	public class WorkerRole : NodeWithJavaBase
	{
		private ZooKeeperNodeRunner _nodeRunner;

		protected override void GuardedRun()
		{
			_nodeRunner.Run();
		}

		protected override void PostJavaInstallInitialize()
		{
			InstallZooKeeper();
		}

		private void InstallZooKeeper()
		{
			_nodeRunner = new ZooKeeperNodeRunner(
				dataDirectory: Path.Combine(DataDirectory, "Data"),
				configsDirectory: Path.Combine(DataDirectory, "Config"),
				logsDirectory: Path.Combine(DataDirectory, "Logs"),
				jarsDirectory: Path.Combine(InstallDirectory, "Jars"),
				javaHome: JavaHome);
			_nodeRunner.Setup();
		}

		private static string DataDirectory
		{
			get { return RoleEnvironment.GetLocalResource("DataDir").RootPath; }
		}
	}
}
