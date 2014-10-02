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
		private JavaInstaller _javaInstaller;
		private const string JavaPlatformDirectory = "JavaPlatform";
		private string _rootResourcesDirectory;

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
				DownloadResources();
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
		/// Gets the resources directory for hte given component.
		/// </summary>
		/// <param name="componentName">The component (directory name) for which we want resources.
		/// It should've been included in <see cref="ResourceDirectoriesToDownload"/>.</param>
		/// <returns>The local directory where the component's resources are located.</returns>
		protected string GetResourcesDirectory(string componentName)
		{
			return Path.Combine(_rootResourcesDirectory, componentName);
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
		/// Gets the container that has all the resource files for the components used in this node.
		/// </summary>
		/// <returns>The container reference.</returns>
		/// <remarks>
		/// By default we get it using the connection string and container name specified in the role
		/// settings:
		/// "BlueCoffee.Resources.Account.ConnectionString" and
		/// "BlueCoffee.Resources.Container.Name".
		/// </remarks>
		protected virtual CloudBlobContainer GetResourcesContainer()
		{
			var connectionString = RoleEnvironment.GetConfigurationSettingValue(
				"BlueCoffee.Resources.Account.ConnectionString");
			var containerName = RoleEnvironment.GetConfigurationSettingValue(
				"BlueCoffee.Resources.Container.Name");
			var storageAccount = CloudStorageAccount.Parse(connectionString);
			return storageAccount.CreateCloudBlobClient().GetContainerReference(containerName);
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

		private void DownloadResources()
		{
			_rootResourcesDirectory = Path.Combine(InstallDirectory, "Resources");
			var resourcesContainer = GetResourcesContainer();
			Parallel.ForEach(ResourceDirectoriesToDownload, directory =>
			{
				var cloudDirectory = resourcesContainer.GetDirectoryReference(directory);
				var localDirectory = Path.Combine(_rootResourcesDirectory, directory);
				Directory.CreateDirectory(localDirectory);
				Parallel.ForEach(cloudDirectory.ListBlobs().OfType<CloudBlockBlob>(), blob =>
					{
						var blobSimpleName = blob.Name.Substring(blob.Name.LastIndexOf('/') + 1);
						blob.DownloadToFile(Path.Combine(localDirectory, blobSimpleName), FileMode.Create);
					});
			});
		}

		private void InstallJava()
		{
			_javaInstaller = new JavaInstaller(
				installDirectory: Path.Combine(InstallDirectory, "Java"),
				resourceFileDirectory: Path.Combine(_rootResourcesDirectory, JavaPlatformDirectory));
			_javaInstaller.Setup();
		}
	}
}
