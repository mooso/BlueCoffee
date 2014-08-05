using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// A base class for Azure worker roles that run with java installed.
	/// </summary>
	public abstract class NodeWithJavaBase : RoleEntryPoint
	{
		private JavaInstaller _javaInstaller;

		/// <summary>
		/// Overrides the Run() method to do the run logic while always logging exceptions before rethrowing them.
		/// </summary>
		public sealed override void Run()
		{
			try
			{
				GuardedRun();
			}
			catch (Exception ex)
			{
				LogException(ex);
				throw;
			}
		}

		/// <summary>
		/// Overrides the OnStart() method for the worker role to install Java then do any other initalization.
		/// </summary>
		/// <returns></returns>
		public sealed override bool OnStart()
		{
			try
			{
				InstallJava();
				PostJavaInstallInitialize();
			}
			catch (Exception ex)
			{
				LogException(ex);
				throw;
			}
			return base.OnStart();
		}

		/// <summary>
		/// Do any other needed initialization after Java is installed.
		/// </summary>
		protected virtual void PostJavaInstallInitialize()
		{ }

		/// <summary>
		/// Run the actual logic of the role (if exceptions are thrown they will be logged first before rethrowing).
		/// </summary>
		protected abstract void GuardedRun();

		/// <summary>
		/// The directory where Java was installed.
		/// </summary>
		protected string JavaHome { get { return _javaInstaller.JavaHome; } }

		/// <summary>
		/// Helper method to get the IP address of a given role instanace that exposes at least one internal endpoint.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns>The IP address.</returns>
		protected static string GetIPAddress(RoleInstance instance)
		{
			return instance.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString();
		}

		/// <summary>
		/// The install directory to put Java in. By default we assume the existence of local resource called "InstallDirectory"
		/// that we use.
		/// </summary>
		protected virtual string InstallDirectory
		{
			get { return RoleEnvironment.GetLocalResource("InstallDirectory").RootPath; }
		}

		/// <summary>
		/// Log the given exception. By default we upload it to a specialized container in the Diagnostics storage account.
		/// </summary>
		/// <param name="ex">The exception.</param>
		protected virtual void LogException(Exception ex)
		{
			var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
			var container = storageAccount
					.CreateCloudBlobClient()
					.GetContainerReference("logs");
			container.CreateIfNotExists();
			container
					.GetBlockBlobReference("Exception from " + RoleEnvironment.CurrentRoleInstance.Id + " on " + DateTime.Now)
					.UploadText(ex.ToString());
		}

		private void InstallJava()
		{
			_javaInstaller = new JavaInstaller(Path.Combine(InstallDirectory, "Java"));
			_javaInstaller.Setup();
		}
	}
}
