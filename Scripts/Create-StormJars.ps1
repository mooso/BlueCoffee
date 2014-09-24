Param ([Parameter(Mandatory=$true)] $StormHome)

# Find out where I am
$Invocation = (Get-Variable MyInvocation).Value
$scriptLocation = Split-Path $Invocation.MyCommand.Path

# Put all the jars in a single directory
$jars = Get-ChildItem "$StormHome\lib","$StormHome\external" -Recurse -Include *.jar
$tempDir = md "$env:TEMP\StormJarsToZip"
$jars | %{Copy-Item $_ $tempDir}

# Find out where to create the zip file
$zipFileName = "$scriptLocation\..\Libraries\Microsoft.Experimental.Azure.Storm\Resources\Jars.zip"
if (Test-Path $zipFileName)
{
    Remove-Item -Force $zipFileName
}

# Create the zip file
Add-Type -Assembly System.IO.Compression.FileSystem
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $zipfilename, $compressionLevel, $false)

# Cleanup the temp directory
Remove-Item -Recurse -Force $tempDir

# Split up the zip file
#Add-Type -TypeDefinition $(Get-Content "$scriptLocation\..\Libraries\CombineJar.cs" -Raw) -Language CSharp
#[SplitAndMerge.Program]::Split($zipfilename)