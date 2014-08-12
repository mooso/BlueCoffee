Param ([Parameter(Mandatory=$true)] $SparkHome, [Parameter(Mandatory=$true)] $SharkHome)

# Find out where I am
$Invocation = (Get-Variable MyInvocation).Value
$scriptLocation = Split-Path $Invocation.MyCommand.Path

# Put all the jars in a single directory
$jars = Get-ChildItem "$SparkHome\assembly\target\scala-2.10","$SparkHome\examples\target\scala-2.10","$SparkHome\lib_managed\jars\datanucleus-*.jar","$SharkHome\target\scala-2.10","$SharkHome\lib_managed\jars\edu.berkeley.cs.shark","$scriptLocation\..\CommonJars" -Recurse -Include *.jar,*.exe
$tempDir = md "$env:TEMP\SparkJarsToZip"
$jars | %{Copy-Item $_ $tempDir}

# Add compute-classpath.cmd
Copy-Item "$SparkHome\bin\compute-classpath.cmd" $tempDir

# Find out where to create the zip file
$zipFileName = "$scriptLocation\..\Libraries\Microsoft.Experimental.Azure.Spark\Resources\Jars.zip"
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
Add-Type -TypeDefinition $(Get-Content "$scriptLocation\..\Libraries\CombineJar.cs" -Raw) -Language CSharp
[SplitAndMerge.Program]::Split($zipfilename)