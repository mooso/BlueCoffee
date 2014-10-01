using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Experimental.Azure.Storm;
using System.IO;

namespace Nimbus
{
	public class WorkerRole : StormNodeBase
	{
		private string _topologyJarPath;

		protected override bool IsNimbus
		{
			get { return true; }
		}

		protected override Task StartOtherWork()
		{
			var uiTask = Task.Factory.StartNew(() => StormRunner.RunUI());
			var outputConnectionString = RoleEnvironment.GetConfigurationSettingValue("Output.Account.ConnectionString");
			var outputContainerName = "fromstorm";
			var outputBlobPrefix = "output";
			var jarTask = Task.Factory.StartNew(() => StormRunner.RunJar(
				className: "com.microsoft.experimental.storm.test.topologies.Main",
				arguments: new[] { outputConnectionString, outputContainerName, outputBlobPrefix },
				jarPath: _topologyJarPath,
				runContinuous: false));
			return Task.WhenAll(uiTask, jarTask);
		}

		protected override void PostStormInstallInitialize()
		{
			_topologyJarPath = Path.Combine(InstallDirectory, "my-topology.jar");
			WriteResourceToFile(_topologyJarPath);
		}

		private static void WriteResourceToFile(string fileName)
		{
			var myAssembly = typeof(WorkerRole).Assembly;
			var resourceName = myAssembly.GetManifestResourceNames().Single();
			using (var resourceStream = myAssembly.GetManifestResourceStream(resourceName))
			using (var fileStream = File.Create(fileName))
			{
				byte[] buffer = new byte[10 * 1024 * 1024];
				int bytesRead;
				while ((bytesRead = resourceStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					fileStream.Write(buffer, 0, bytesRead);
				}
			}
		}
	}
}
