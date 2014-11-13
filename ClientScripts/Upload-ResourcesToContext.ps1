<# 
 .Synopsis
  Uploads the required resources for Blue Coffee into a storage account.

 .Description
  Blue Coffee needs the JAR and SDK files to be available to it while running in Azure.
  This function uploads all the files into the standard places in a given storage account.

 .Parameter StorageContext
  The storage context for the storage account we need to upload to.

 .Parameter JDKDownloadUri
  The URI to use to download the JDK you want to use as your Java JDK.

 .Parameter ProjectRoot
  The root of the project, which contains the BlueCoffeeResources directory.

 .Parameter Container
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
    $JDKDownloadUri = 'http://cdn.azulsystems.com/zulu/2014-10-8.4-bin/zulu1.8.0_25-8.4.0.1-win64.zip',
    $ProjectRoot = '.',
    $Container = 'bluecoffeeresources')

$ErrorActionPreference = 'Stop'
$LocalJDK = "$ProjectRoot\BlueCoffeeResources\JavaPlatform\jdk.zip"
If ($JDKDownloadUri -ne $null)
{
    Write-Host "Downloading JDK..."
    $OutputDirectory = Split-Path $LocalJDK
    If (-not $(Test-Path $OutputDirectory))
    {
        $MDOutput = md $OutputDirectory
    }
    $WebRequest = [System.Net.WebRequest]::CreateHttp($JDKDownloadUri)
    $WebRequest.Referer = 'http://www.azulsystems.com/products/zulu/downloads'
    $WebResponse = [System.Net.HttpWebResponse]$WebRequest.GetResponse()
    If ($WebResponse.StatusCode -ne [System.Net.HttpStatusCode]::OK)
    {
        Throw $WebResponse.StatusDescription
    }
    $ResponseStream = $WebResponse.GetResponseStream()
    $FileStream = [System.IO.File]::Create($LocalJDK)
    $ResponseStream.CopyTo($FileStream)
    $FileStream.Close()
    $ResponseStream.Close()
}
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
