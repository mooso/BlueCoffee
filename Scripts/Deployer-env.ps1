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
	$service = Get-AzureService $serviceName
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
	$flavor = 'Release')
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
	Delete-ExistingDeployments $serviceName
	Write-Host "Deploying..."
	$deployment = New-AzureDeployment -ServiceName $serviceName -Package "$publishDirectory\$testServiceName.cspkg" -Configuration $tempConfigFile -Label "Deployment on $(Get-Date)" -Slot Production
}

function BuildAndDeploy([Parameter(Mandatory=$true)]$testServiceName,
	[Parameter(Mandatory=$true)]$serviceName)
{
	Trap
	{
		return $_
	}
	Build-TestService $testServiceName
	if ($LASTEXITCODE -eq 0)
	{
		Deploy-TestService $testServiceName $serviceName
	}
}
