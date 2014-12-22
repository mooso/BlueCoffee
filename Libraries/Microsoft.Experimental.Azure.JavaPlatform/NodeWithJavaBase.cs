using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
		private JavaAzureInstaller _javaInstaller;
		private const string JavaPlatformDirectory = "JavaPlatform";

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
				_javaInstaller = CreateJavaInstaller();
				_javaInstaller.DownloadResources(ResourceDirectoriesToDownload);
				_javaInstaller.InstallJava();
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
		/// Create a new Java Azure Installer to use.
		/// </summary>
		/// <returns>The installer.</returns>
		protected virtual JavaAzureInstaller CreateJavaInstaller()
		{
			return new JavaAzureInstaller();
		}

		/// <summary>
		/// Run the actual logic of the role (if exceptions are thrown they will be logged first before rethrowing).
		/// </summary>
		protected abstract void GuardedRun();

		/// <summary>
		/// The directory where Java was installed.
		/// </summary>
		protected string JavaHome { get { return _javaInstaller.JavaHome; } }

		/// <summary>
		/// The Install directory from the JavaInstaller.
		/// </summary>
		protected string InstallDirectory { get { return _javaInstaller.InstallDirectory; } }

		/// <summary>
		/// The Java Azure Installer we're using.
		/// </summary>
		protected JavaAzureInstaller JavaInstaller { get { return _javaInstaller; } }

		/// <summary>
		/// Gets the resources directory for the given component.
		/// </summary>
		/// <param name="componentName">The component (directory name) for which we want resources.
		/// It should've been included in <see cref="ResourceDirectoriesToDownload"/>.</param>
		/// <returns>The local directory where the component's resources are located.</returns>
		public string GetResourcesDirectory(string componentName)
		{
			return _javaInstaller.GetResourcesDirectory(componentName);
		}

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
		/// The resource directories to download. By default it's just JavaPlatform.
		/// </summary>
		protected virtual IEnumerable<string> ResourceDirectoriesToDownload
		{
			get
			{
				yield return JavaPlatformDirectory;
			}
		}

		/// <summary>
		/// Log the given exception. By default we upload it to a specialized container in the BlueCoffee.Resources.Account.ConnectionString storage account.
		/// </summary>
		/// <param name="ex">The exception.</param>
		protected virtual void LogException(Exception ex)
		{
			var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlueCoffee.Resources.Account.ConnectionString"));
			var container = storageAccount
					.CreateCloudBlobClient()
					.GetContainerReference("logs");
			container.CreateIfNotExists();
			container
					.GetBlockBlobReference("Exception from " + RoleEnvironment.CurrentRoleInstance.Id + " on " + DateTime.Now)
					.UploadText(ex.ToString());
		}

		/// <summary>
		/// Gets a Role from the RoleEnvironment that has any of the given name alternatives.
		/// </summary>
		/// <param name="alternativeNames">The possible names for the role.</param>
		/// <returns>The Role. Throws if not found.</returns>
		protected Role GetRole(params string[] alternativeNames)
		{
			foreach (var name in alternativeNames)
			{
				Role obtainedRole;
				if (RoleEnvironment.Roles.TryGetValue(name, out obtainedRole))
				{
					return obtainedRole;
				}
			}
			throw new InvalidOperationException(String.Format(
				"Unable to find role ({{0}) in the role environment. Available roles: {1}",
				String.Join(",", alternativeNames),
				RoleEnvironment.Roles.Keys));
		}
	}
}
