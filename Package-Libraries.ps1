if (Test-Path .\FinalPackages)
{
    Remove-Item -Force -Recurse .\FinalPackages
}
$finalPackagesDir = md .\FinalPackages
Get-ChildItem Libraries -Recurse -Include *.nuspec | %{ `
    $dir = Split-Path $_
    pushd $dir
    $projectFile = Get-ChildItem *.csproj
    nuget pack -OutputDirectory $finalPackagesDir -Symbols -Properties 'Configuration=Release;Platform=AnyCPU' -Build $projectFile
    popd
}