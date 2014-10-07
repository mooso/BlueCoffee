using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// Helper class to install Java and library components in Azure.
	/// </summary>
	public class JavaAzureInstaller
	{
		private JavaInstaller _javaInstaller;

		/// <summary>
		/// Install Java - assumes resources were downloaded already.
		/// </summary>
		public void InstallJava()
		{
			_javaInstaller = new JavaInstaller(
				installDirectory: Path.Combine(InstallDirectory, "Java"),
				resourceFileDirectory: Path.Combine(RootResourcesDirectory, JavaPlatformDirectory));
			_javaInstaller.Setup();
		}

		/// <summary>
		/// Extract a zip archive from the given downloaded component directory.
		/// </summary>
		/// <param name="componentName">The component we're extracting the archive from (presumably downloaded in DownloadResources).</param>
		/// <param name="archiveFileName">The zip file name within this component.</param>
		/// <param name="destinationDirectory">The destination directory to extract to.</param>
		public void ExtractResourceArchive(string componentName, string archiveFileName, string destinationDirectory)
		{
			ZipFile.ExtractToDirectory(
				Path.Combine(GetResourcesDirectory(componentName), archiveFileName),
				destinationDirectory);
		}

		/// <summary>
		/// The directory where Java was installed.
		/// </summary>
		public string JavaHome { get { return _javaInstaller == null ? null : _javaInstaller.JavaHome; } }

		/// <summary>
		/// Gets the resources directory for the given component.
		/// </summary>
		/// <param name="componentName">The component (directory name) for which we want resources.
		/// It should've been downloaded in <see cref="DownloadResources"/>.</param>
		/// <returns>The local directory where the component's resources are located.</returns>
		public string GetResourcesDirectory(string componentName)
		{
			return Path.Combine(RootResourcesDirectory, componentName);
		}

		/// <summary>
		/// The directory where we'll download resources.
		/// </summary>
		public virtual string RootResourcesDirectory
		{
			get { return Path.Combine(InstallDirectory, "Resources"); }
		}

		/// <summary>
		/// Name of the Java directory in the resources directory.
		/// </summary>
		public virtual string JavaPlatformDirectory
		{
			get { return "JavaPlatform"; }
		}

		/// <summary>
		/// Download the given resources.
		/// </summary>
		/// <param name="resources">The resources to download. If null we'll default to just Java.</param>
		public void DownloadResources(IEnumerable<string> resources = null)
		{
			resources = resources ?? new[] { JavaPlatformDirectory };
			var resourcesContainer = GetResourcesContainer();
			Parallel.ForEach(resources, directory =>
			{
				var cloudDirectory = resourcesContainer.GetDirectoryReference(directory);
				var localDirectory = Path.Combine(RootResourcesDirectory, directory);
				Directory.CreateDirectory(localDirectory);
				Parallel.ForEach(cloudDirectory.ListBlobs().OfType<CloudBlockBlob>(), blob =>
				{
					var blobSimpleName = blob.Name.Substring(blob.Name.LastIndexOf('/') + 1);
					blob.DownloadToFile(Path.Combine(localDirectory, blobSimpleName), FileMode.Create);
				});
			});
		}

		/// <summary>
		/// The Install directory, defaults to the local resource "InstallDirectory".
		/// </summary>
		public virtual string InstallDirectory
		{
			get { return RoleEnvironment.GetLocalResource("InstallDirectory").RootPath; }
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
		public virtual CloudBlobContainer GetResourcesContainer()
		{
			var connectionString = RoleEnvironment.GetConfigurationSettingValue(
				"BlueCoffee.Resources.Account.ConnectionString");
			var containerName = RoleEnvironment.GetConfigurationSettingValue(
				"BlueCoffee.Resources.Container.Name");
			var storageAccount = CloudStorageAccount.Parse(connectionString);
			return storageAccount.CreateCloudBlobClient().GetContainerReference(containerName);
		}
	}
}
