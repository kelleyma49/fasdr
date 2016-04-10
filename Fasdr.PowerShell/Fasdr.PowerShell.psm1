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

function Set-DatabaseFromJumpList {
	$dllPath = Join-Path $PSScriptRoot 'JumpList.dll'
	[System.Reflection.Assembly]::LoadFrom($dllPath)
	
	$numEntriesAdded = 0
	$dirs = @{}

	$fileSystemProvider = 'FileSystem'
	$autoPath = join-path $env:APPDATA 'Microsoft\Windows\Recent\AutomaticDestinations\'
	gci -Path $autoPath -Filter *.automaticDestinations-ms | ForEach-Object {
		$aj = [JumpList.JumpList]::LoadAutoJumplist($_.FullName) 
		$aj.DestListEntries | ForEach-Object {
			$path = $_.Path
			if (Resolve-Path $path -ErrorAction SilentlyContinue) {
				if  (test-path $path -PathType Leaf) {
					$path = Split-Path $path -Parent
				}

				$pathLower = $path.ToLower()
				if (!$dirs.ContainsKey($pathLower)) {
					$dirs[$path.ToLower()] = $path
					if (Add-Frecent $path $FileSystemProvider) {
						$numEntriesAdded = $numEntriesAdded + 1
					}
				}
			}
		} 
	}

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
	$provider = $null

	# create provider if it doesn't exist:
	if (!$global:fasdrDatabase.Providers.TryGetValue($providerName,[ref] $provider)) {
		$provider = New-Object Fasdr.Backend.Provider $providerName
		$global:fasdrDatabase.Providers[$providerName] = $provider
	}
		
	return $provider.UpdateEntry($providerPath,[System.Predicate[string]]{param($fullPath) Test-Path $fullPath -PathType Leaf})
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