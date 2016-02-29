# load C# dll for backend:
$dllPath = Join-Path $PSScriptRoot 'Fasdr.Backend.dll'
[System.Reflection.Assembly]::LoadFile($dllPath)
$fasdrDatabase = $null

<#
#>
function Init-Database {
	param([System.IO.Abstractions.IFileSystem] $fileSystem=$null)
	if ($fileSystem -eq $null) {
		$fileSystem = New-Object System.IO.Abstractions.FileSystem
	}
	$fasdrDatabase = New-Object Fasdr.Backend.Database -ArgumentList $fileSystem
	$fasdrDatabase.Load()
	$fasdrMatcher = New-Object Fasdr.Backend.Matcher
}

<#
	My Function
#>
function Find-Frecent {
	if ($fasdrDatabase -eq $null) {
		Init-Database
	}
	$providerName = $PWD.Provider.Name
	return $fasdrMatcher.Matches($fasdrDatabase,$providerName,$args)
}