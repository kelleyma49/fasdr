# load C# dll for backend:
$dllPath = Join-Path $PSScriptRoot 'Fasdr.Backend.dll'
[System.Reflection.Assembly]::LoadFile($dllPath)
$fasdrDatabase = New-Object Fasdr.Backend.Database
$fasdrDatabase.Load()

$fasdrMatcher = New-Object Fasdr.Backend.Matcher

<#
	My Function
#>
function Find-Frecent {
	$providerName = $PWD.Provider.Name
	return $fasdrMatcher.Matches($fasdrDatabase,$providerName,$args)
}