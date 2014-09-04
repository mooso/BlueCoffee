# Script written to be dot-sourced to help with building and deploying the Azure service packages.
# Assumes that you have Azure Powershell module installed, and the current subscription is set to a
# a valid subscription.

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$rootDirectory = Split-Path $(Get-ScriptDirectory)
$solutionDirectory = $rootDirectory

function Ensure-NugetRestored
{
	pushd $solutionDirectory
	nuget restore
	popd
}

function Build-TestService($testServiceName, $flavor = 'Release')
{
	Write-Host "Building package..."
	Ensure-NugetRestored
	pushd "$solutionDirectory\TestServices\$testServiceName\$testServiceName"
	$buildOutput = msbuild "$testServiceName.ccproj" /t:Publish "/p:Configuration=$flavor" /p:Platform="AnyCPU" /p:VisualStudioVersion="12.0"
	if ($LASTEXITCODE -ne 0)
	{
		$buildOutput | Write-Host
	}
	popd
}

function Discover-AccountsForLocation($location)
{
	Trap
	{
		return $_
	}
	Get-AzureStorageAccount | ?{$_.GeoPrimaryLocation -eq $location}
}

function Discover-Accounts($serviceName)
{
	Trap
	{
		return $_
	}
	$service = Get-AzureService $serviceName
	Discover-AccountsForLocation $service.Location
}

function Get-ConnectionString([Microsoft.WindowsAzure.Commands.ServiceManagement.Model.StorageServicePropertiesOperationContext]$storageAccount)
{
	$key = Get-AzureStorageKey $storageAccount.StorageAccountName
	"DefaultEndpointsProtocol=https;AccountName=$($storageAccount.StorageAccountName);AccountKey=$($key.Primary)"
}

function Upload-ResourcesToContext([Microsoft.WindowsAzure.Commands.Storage.Model.ResourceModel.AzureStorageContext]$storageContext)
{
	Write-Host "Uploading resources..."
    $container = 'bluecoffeeresources'
    $containerReference = New-AzureStorageContainer -Name $container -Context $storageContext -ErrorAction SilentlyContinue
    $libraryPrefix = "Microsoft.Experimental.Azure."
    $libraryDirectories = Get-ChildItem "$rootDirectory\Libraries" | ?{$_.Name.StartsWith($libraryPrefix)};
    $libraryDirectories | %{
        $blobNamePrefix = $_.Name.Substring($libraryPrefix.Length) + "/"
        $myResources = Get-ChildItem "$($_.FullName)\Resources" | ?{$_.Extension -eq ".zip"}
        $myResources | %{
            $blobName = $blobNamePrefix + $_.Name
            $existingBlob = Get-AzureStorageBlob -Blob $blobName -Context $storageContext -Container $container -ErrorAction SilentlyContinue
            if (($existingBlob -eq $null) -or ($existingBlob.Length -ne $_.Length))
            {
                Write-Host "Uploading $blobName ..."
                $newBlob = Set-AzureStorageBlobContent -Blob $blobName -Context $storageContext -Container $container -File $($_.FullName) -Force
            }
        }
    }
}

function Upload-Resources([Microsoft.WindowsAzure.Commands.ServiceManagement.Model.StorageServicePropertiesOperationContext]$storageAccount)
{
    Upload-ResourcesToContext $(New-AzureStorageContext -ConnectionString $(Get-ConnectionString $storageAccount))
}

function Upload-ResourcesToLocal()
{
    Upload-ResourcesToContext $(New-AzureStorageContext -ConnectionString "UseDevelopmentStorage=true")
}

function Delete-ExistingDeployments([Parameter(Mandatory=$true)]$serviceName)
{
	Write-Host "Deleting existing deployments..."
	try
	{
		$removeOutput = Remove-AzureDeployment -ServiceName $serviceName -Slot Production -Force -DeleteVHD
	} catch {}
}

function Deploy-TestService(
	[Parameter(Mandatory=$true)]$testServiceName,
	[Parameter(Mandatory=$true)]$serviceName,
	[Microsoft.WindowsAzure.Commands.ServiceManagement.Model.StorageServicePropertiesOperationContext]$storageAccount = $null,
	$flavor = 'Release',
	[Switch]$upgradeInPlace)
{
	Trap
	{
		return $_
	}
	$publishDirectory = "$solutionDirectory\TestServices\$testServiceName\$testServiceName\bin\Release\app.publish"
	if ($storageAccount -eq $null)
	{
		Write-Host "Discovering storage account..."
		$storageAccount = $(Discover-Accounts $serviceName)[0]
	}
    Upload-Resources $storageAccount
	Write-Host "Constructing connection string..."
	$connectionString = Get-ConnectionString $storageAccount
	Write-Host "Writing configuration file..."
	$tempConfigFile = "$env:TEMP\TestServiceFinalSettings.cscfg"
	if (Test-Path $tempConfigFile)
	{
		Remove-Item $tempConfigFile -Force
	}
	Get-Content "$publishDirectory\ServiceConfiguration.Cloud.cscfg" |
		%{$_ -replace "UseDevelopmentStorage=true",$connectionString} > $tempConfigFile
	Write-Host "Deploying..."
	if ($upgradeInPlace -and ($existingDeployment = Get-AzureDeployment $serviceName -Sl Production -ErrorAction Ignore))
	{
		$deployment = Set-AzureDeployment -ServiceName $serviceName -Package "$publishDirectory\$testServiceName.cspkg" -Configuration $tempConfigFile -Label "Deployment on $(Get-Date)" -Slot Production -Upgrade -Force
	}
	else
	{
		Delete-ExistingDeployments $serviceName
        Write-Host "Creating new deployment..."
		$deployment = New-AzureDeployment -ServiceName $serviceName -Package "$publishDirectory\$testServiceName.cspkg" -Configuration $tempConfigFile -Label "Deployment on $(Get-Date)" -Slot Production
	}
}

function BuildAndDeploy([Parameter(Mandatory=$true)]$testServiceName,
	[Parameter(Mandatory=$true)]$serviceName,
	[Switch]$upgradeInPlace)
{
	Trap
	{
		return $_
	}
	Build-TestService $testServiceName
	if ($LASTEXITCODE -eq 0)
	{
		Deploy-TestService $testServiceName $serviceName -upgradeInPlace:$upgradeInPlace
	}
}
