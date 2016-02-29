# load C# dll for backend:
$dllPath = Join-Path $PSScriptRoot 'Fasdr.Backend.dll'
[System.Reflection.Assembly]::LoadFrom($dllPath)
$dllPath = Join-Path $PSScriptRoot 'System.IO.Abstractions.dll'
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
}

<#
	My Function
#>
function Find-Frecent {
	if ($fasdrDatabase -eq $null) {
		Init-Database
	}
	$providerName = $PWD.Provider.Name
	return [Fasdr.Backend.Matcher]::Matches($fasdrDatabase,$providerName,$args)
}


Export-ModuleMember -Function Find-Frecent