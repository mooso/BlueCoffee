<# 
 .Synopsis
  Uploads the required resources for Blue Coffee into a storage account.

 .Description
  Blue Coffee needs the JAR and SDK files to be available to it while running in Azure.
  This function uploads all the files into the standard places in a given storage account.

 .Parameter storageContext
  The storage context for the storage account we need to upload to.

 .Parameter projectRoot
  The root of the project, which contains the BlueCoffeeResources directory.

 .Parameter container
  The name of the container in which to put the resources

 .Example
   # Upload the resources to the local storage emulator for debugging
   $LocalStorage = New-AzureStorageContext -ConnectionString 'UseDevelopmentStorage=true'
   Upload-ResourcesToContext $LocalStorage

 .Example
   # Upload the resources to an account in my current subscription
   $AccountName = 'myacc'
   $Storage = New-AzureStorageContext -StorageAccountName $AccountName -StorageAccountKey $(Get-AzureStorageKey $AccountName).Primary
   Upload-ResourcesToContext $Storage

#>
param([Parameter(Mandatory=$true)]$StorageContext,
    $ProjectRoot = '.',
    $Container = 'bluecoffeeresources')

Write-Host "Uploading resources..."
$ContainerReference = New-AzureStorageContainer -Name $Container -Context $StorageContext -ErrorAction SilentlyContinue
$ResourceDirectories = Get-ChildItem "$ProjectRoot\BlueCoffeeResources"
$ResourceDirectories | %{
    $BlobNamePrefix = $_.Name + "/"
    $MyResources = Get-ChildItem "$($_.FullName)"
    $MyResources | %{
        $BlobName = $BlobNamePrefix + $_.Name
        $ExistingBlob = Get-AzureStorageBlob -Blob $BlobName -Context $StorageContext -Container $Container -ErrorAction SilentlyContinue
        if (($ExistingBlob -eq $null) -or ($ExistingBlob.Length -ne $_.Length))
        {
            Write-Host "Uploading $BlobName ..."
            $NewBlob = Set-AzureStorageBlobContent -Blob $BlobName -Context $StorageContext -Container $Container -File $($_.FullName)
        }
    }
}
