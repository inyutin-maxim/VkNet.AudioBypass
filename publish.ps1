param ($Key = $(throw "-Key is required."), $Source = "https://api.nuget.org/v3/index.json")

$artifacts = "$PSScriptRoot\artifacts"

dotnet pack -c Release -o $artifacts
dotnet nuget push $artifacts\*.nupkg -s $Source -k $Key