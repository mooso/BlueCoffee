function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

function Recreate-Directory($directory)
{
    if (Test-Path $directory)
    {
        Remove-Item -Force -Recurse $directory
    }
    $md = md $directory
}

function Get-AssemblyDescription($assemblyPath)
{
    $assembly = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($assemblyPath)
    $attributes = [System.Reflection.CustomAttributeData]::GetCustomAttributes($assembly)
    $descriptionAttribute = $attributes | ?{$_.AttributeType.Name -eq "AssemblyDescriptionAttribute"}
    $descriptionAttribute.ConstructorArguments[0].Value
}

$rootDirectory = Split-Path $(Get-ScriptDirectory)
$packageDirectory = "$rootDirectory\FinalPackages"
$stagingDirectory = "$rootDirectory\PackageStaging"
$nugetVersion = Get-Content -Raw "$rootDirectory\Libraries\NugetVersion.txt"

Recreate-Directory $packageDirectory

Get-ChildItem "$rootDirectory\Libraries" -Recurse -Include *.nuspec | %{
    $dir = Split-Path $_
    pushd $dir
    $projectFile = Get-ChildItem *.csproj
    $buildOutput = msbuild $projectFile /p:Configuration="Release" /p:Platform="AnyCPU" /p:VisualStudioVersion="12.0"
    popd
	if ($LASTEXITCODE -ne 0)
	{
		$buildOutput | Write-Host
        return
	}
    $libraryName = $projectFile.BaseName
    $libSimpleName = $libraryName.Substring("Microsoft.Experimental.Azure.".Length)
    $assemblyDescription = Get-AssemblyDescription "$dir\bin\Release\$libraryName.dll"
    Recreate-Directory $stagingDirectory
    $nuspecContent = Get-Content -Raw "$dir\$libraryName.nuspec"
    $nuspecContent = $nuspecContent.Replace('$id$', $libraryName).Replace('$title$', $libraryName).Replace('$description$', $assemblyDescription).Replace('$nugetversion$', $nugetVersion)
    $nuspecContent | Out-File -FilePath "$stagingDirectory\$libraryName.nuspec"
    $libDir = md "$stagingDirectory\lib\net45"
    Copy-Item "$dir\bin\Release\$libraryName.*" $libDir
    If (Test-Path "$dir\Resources")
    {
        $resourcesDir = md "$stagingDirectory\content\net45\BlueCoffeeResources\$libSimpleName"
        Copy-Item "$dir\Resources\*.zip" $resourcesDir
    }
    Copy-Item "$rootDirectory\ClientScripts\*.ps1" "$stagingDirectory\content\net45"
    pushd $stagingDirectory
    nuget pack -OutputDirectory $packageDirectory
    popd
    Remove-Item -Recurse -Force $stagingDirectory
}
