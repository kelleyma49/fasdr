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
	$numAdded = 0


	$paths = @{}
	$paths = [Fasdr.Windows.Collectors]::CollectPaths($paths,[fasdr.Windows.Collectors]::EnumerateJumpLists())
	$paths = [Fasdr.Windows.Collectors]::CollectPaths($paths,[fasdr.Windows.Collectors]::EnumerateRecents())
	$paths = [Fasdr.Windows.Collectors]::CollectPaths($paths,[fasdr.Windows.Collectors]::EnumerateSpecialFolders())

	$pred = [System.Predicate[string]]{param($fullPath) Test-Path $fullPath -PathType Leaf}
	$paths.Values | ForEach-Object {
		$path = $_
		if ((test-path $path) -and $global:fasdrDatabase.AddEntry($fileSystemProvider,$_,$pred)) {
			$numAdded += 1	
		}
	}	

	Write-Output "Num entries added: $numAdded"
	Save-Database
}


<#
	Find-Frecent
#>
function Find-Frecent {
	param([string]$ProviderPath,[bool]$FilterContainers=$false,[bool]$FilterLeaves=$false)
	if ($global:fasdrDatabase -eq $null) {
		Initialize-Database
	}
	$providerName = $PWD.Provider.Name
	$result = [Fasdr.Backend.Matcher]::Matches($global:fasdrDatabase,$providerName,$FilterContainers,$FilterLeaves,$ProviderPath)
	if ($result -isnot [system.array]) { $result = @($result)}
	$result
}

<#
	Add-Frecent
#>
function Add-Frecent {
	param([string]$providerPath,[string]$providerName = $PWD.Provider.Name)
	if ($global:fasdrDatabase -eq $null) {
		Initialize-Database
	}

	return $global:fasdrDatabase.AddEntry($providerName,$providerPath,[System.Predicate[string]]{param($fullPath) Test-Path $fullPath -PathType Leaf})
}

<# 
	Set-Frecent
#>
function Set-Frecent {
	param([string]$Path)

	# if it's not a valid path from tab completion or input,
	# find the last result:
	if (!(Resolve-Path "$Path" -ErrorAction SilentlyContinue)) {
		$results = Find-Frecent "$Path" $false $true
		if ($results -ne $null)  {
			if ($result -isnot [system.array]) { 
				$Path = $results
			} else {
				$Path = $results[0]
			}
		}
	}
	Set-Location $Path
	if ($?) {
		$Path = (Get-Item -Path ".\" -Verbose).FullName
		Add-Frecent $Path
		Save-Database
	} 
}

foreach ($file in dir $PSScriptRoot\*.ArgumentCompleters.ps1)
{
    . $file.FullName
}