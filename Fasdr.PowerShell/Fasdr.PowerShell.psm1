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
	if ($global:fasdrDatabase -eq $null) {
		Write-Host 'database is null!'
	}
	$global:fasdrDatabase.Load() 
}

function Save-Database {
	$global:fasdrDatabase.Save() 
}

function Import-Recents {
	if ($global:fasdrDatabase -eq $null) {
		Initialize-Database
	}

	$dllPath = Join-Path $PSScriptRoot 'Fasdr.Windows.dll'
	[System.Reflection.Assembly]::LoadFrom($dllPath)
	
	$fileSystemProvider = 'FileSystem'
	$numEntriesAdded = 0
	$dirs = @{}

	$numEntriesAdded += [Fasdr.Windows.DatabaseExt]::AddFromJumplists($global::fasdrDatabase,$fileSystemProvider)
	$numEntriesAdded += [Fasdr.Windows.DatabaseExt]::AddFromRecents($global::fasdrDatabase,$fileSystemProvider)
	
	Write-Output "Num entries added: $numEntriesAdded"
	Save-Database
}


<#
	Find-Frecent
#>
function Find-Frecent {
	param([string]$ProviderPath)
	if ($global:fasdrDatabase -eq $null) {
		Initialize-Database
	}
	$providerName = $PWD.Provider.Name
	return [Fasdr.Backend.Matcher]::Matches($global:fasdrDatabase,$providerName,$ProviderPath)
}

<#
	Add-Frecent
#>
function Add-Frecent {
	param([string]$providerPath,[string]$providerName = $PWD.Provider.Name)
	if ($global:fasdrDatabase -eq $null) {
		Initialize-Database
	}

	return $global:fasdrDatabase.AddEntry($providerName,$providerPath,[System.Predicate[string]]{param($fullPath) Test-Path $fullPath -PathType Leaf});
}

<# 
	Set-Frecent
#>
function Set-Frecent {
	param([string]$Path)
	Set-Location $Path
	if ($?) {
		if (Add-Frecent $Path) {
			Save-Database
		}
	} 
}

foreach ($file in dir $PSScriptRoot\*.ArgumentCompleters.ps1)
{
    . $file.FullName
}