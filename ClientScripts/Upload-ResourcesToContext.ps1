param([Parameter(Mandatory=$true)][Microsoft.WindowsAzure.Commands.Storage.Model.ResourceModel.AzureStorageContext]$storageContext,
    $projectRoot = '.',
    $container = 'bluecoffeeresources')
{
	Write-Host "Uploading resources..."
    $containerReference = New-AzureStorageContainer -Name $container -Context $storageContext -ErrorAction SilentlyContinue
    $resourceDirectories = Get-ChildItem "$projectRoot\BlueCoffeeResources"
    $resourceDirectories | %{
        $blobNamePrefix = $_.Name + "/"
        $myResources = Get-ChildItem "$($_.FullName)"
        $myResources | %{
            $blobName = $blobNamePrefix + $_.Name
            $existingBlob = Get-AzureStorageBlob -Blob $blobName -Context $storageContext -Container $container -ErrorAction SilentlyContinue
            if (($existingBlob -eq $null) -or ($existingBlob.Length -ne $_.Length))
            {
                Write-Host "Uploading $blobName ..."
                $newBlob = Set-AzureStorageBlobContent -Blob $blobName -Context $storageContext -Container $container -File $($_.FullName)
            }
        }
    }
}
