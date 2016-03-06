# load C# dll for backend:
$dllPath = Join-Path $PSScriptRoot 'Fasdr.Backend.dll'
[System.Reflection.Assembly]::LoadFrom($dllPath)
$dllPath = Join-Path $PSScriptRoot 'System.IO.Abstractions.dll'
[System.Reflection.Assembly]::LoadFile($dllPath)
$global:fasdrDatabase = $null

<#
#>
function Initialize-Database {
	param([System.IO.Abstractions.IFileSystem] $fileSystem=$null,$defaultDrive=$null)
	if ($fileSystem -eq $null) {
		$fileSystem = New-Object System.IO.Abstractions.FileSystem
	}
	$global:fasdrDatabase = New-Object Fasdr.Backend.Database -ArgumentList $fileSystem,$defaultDrive
	$global:fasdrDatabase.Load() 
}

<#
	My Function
#>
function Find-Frecent {
	if ($global:fasdrDatabase -eq $null) {
		Initialize-Database
	}
	$providerName = $PWD.Provider.Name
	#return [Fasdr.Backend.Matcher]::Matches($global:fasdrDatabase,$providerName,$args)
	return [Fasdr.Backend.Matcher]::Matches($global:fasdrDatabase,$providerName,$args)
}


Export-ModuleMember -Function Initialize-Database
Export-ModuleMember -Function Find-Frecent