# load C# dll for backend:
$dllPath = Join-Path $PSScriptRoot 'Fasdr.Backend.dll'
[System.Reflection.Assembly]::LoadFrom($dllPath)
$dllPath = Join-Path $PSScriptRoot 'System.IO.Abstractions.dll'
[System.Reflection.Assembly]::LoadFile($dllPath)
$global:fasdrDatabase = $null


#region TabExpansion
# Save off the previous tab completion so it can be restored if this module
# is removed.
$global:oldTabExpansion = $function:TabExpansion
$global:oldTabExpansion2 = $function:TabExpansion2

[bool]$updatedTypeData = $false

$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove =
{
    if ($null -ne $oldTabExpansion)
    {
        Set-Item function:\TabExpansion $oldTabExpansion
    }
    if ($null -ne $oldTabExpansion2)
    {
        Set-Item function:\TabExpansion2 $oldTabExpansion2
    }
}

write-host ${function:oldTabExpansion2}
function global:TabExpansion2
{
    [CmdletBinding(DefaultParameterSetName = 'ScriptInputSet')]
    Param(
        [Parameter(ParameterSetName = 'ScriptInputSet', Mandatory, Position = 0)]
        [string] $inputScript,

        [Parameter(ParameterSetName = 'ScriptInputSet', Mandatory, Position = 1)]
        [int] $cursorColumn,

        [Parameter(ParameterSetName = 'AstInputSet', Mandatory, Position = 0)]
        [System.Management.Automation.Language.Ast] $ast,

        [Parameter(ParameterSetName = 'AstInputSet', Mandatory, Position = 1)]
        [System.Management.Automation.Language.Token[]] $tokens,

        [Parameter(ParameterSetName = 'AstInputSet', Mandatory, Position = 2)]
        [System.Management.Automation.Language.IScriptPosition] $positionOfCursor,

        [Parameter(ParameterSetName = 'ScriptInputSet', Position = 2)]
        [Parameter(ParameterSetName = 'AstInputSet', Position = 3)]
        [Hashtable] $options = $null
    )

	$results = $null
	if ($oldTabExpansion2 -ne $null -and $oldTabExpansion2.File -ne $null)
    {
        $results = (& $oldTabExpansion2 @PSBoundParameters)
    }

	# if the tab expansion couldn't match, then we check for our special 
	# word completion:
	if ($results.CompletionMatches.Count -eq 0)
	{
		if ($psCmdlet.ParameterSetName -eq 'ScriptInputSet')
		{
			$ast = [System.Management.Automation.Language.Parser]::ParseInput(
				$inputScript, [ref]$tokens, [ref]$null)

			$text = $ast.Extent.Text
			if ($text -match '\s::(.*)')
			{
				$results.ReplacementIndex = $text.IndexOf(" ::") + 1
				for ($i=$results.ReplacementIndex;$i -lt $text.Length;$i++) {
					if ([char]::IsWhiteSpace($text[$i])) {
						break
					}
				}
				$results.ReplacementLength = $i - $results.ReplacementIndex
				$currentCompletionText = $matches[1].Trim()
				Find-Frecent -ProviderPath $currentCompletionText | ForEach-Object {
					$textCompletion = $_
					if ($textCompletion -match '\s+') {
						$textCompletion = "'" + $textCompletion + "'"
					}
					$results.CompletionMatches.Add(
						(New-Object Management.Automation.CompletionResult $textCompletion,$_,"ProviderContainer",$_)
					)
				}
			}
		}
	}

	return $results
}
#endregion

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
	$totalItems = 4
	Write-Progress -Activity "Importing recents into Fasdr database" -Status "Progress->" -PercentComplete 0
	Write-Progress -ParentId 1 -Activity "Importing from jump lists" -Status "0% Complete" -PercentComplete 0
	$paths = [Fasdr.Windows.Collectors]::CollectPaths($paths,[fasdr.Windows.Collectors]::EnumerateJumpLists())
	$percentComplete = 100/$totalItems
	Write-Progress -Activity "Importing recents into Fasdr database" -Status "Progress->" -PercentComplete $percentComplete
	Write-Progress -ParentId 1 -Activity "Importing from recents" -Status "Progress" -PercentComplete 0
	$paths = [Fasdr.Windows.Collectors]::CollectPaths($paths,[fasdr.Windows.Collectors]::EnumerateRecents())
	$percentComplete = 200/$totalItems
	Write-Progress -Activity "Importing recents into Fasdr database" -Status "Progress->" -PercentComplete $percentComplete
	Write-Progress -ParentId 1 -Activity "Importing from special folders" -Status "Progress" -PercentComplete 0
	$paths = [Fasdr.Windows.Collectors]::CollectPaths($paths,[fasdr.Windows.Collectors]::EnumerateSpecialFolders())

	$percentComplete = 300/$totalItems
	Write-Progress -Activity "Importing recents into Fasdr database" -Status "Progress->" -PercentComplete $percentComplete
	$pred = [System.Predicate[string]]{param($fullPath) Test-Path $fullPath -PathType Leaf}
	$numPaths = $paths.Values.Count
	$paths.Values | ForEach-Object {
		$path = $_
		if ((test-path $path) -and $global:fasdrDatabase.AddEntry($fileSystemProvider,$path,$pred)) {
			$subPercentComplete = ($numAdded*100)/$numPaths
			Write-Progress -ParentId 1 -Activity "Adding $path" -Status "Progress" -PercentComplete $subPercentComplete
			$numAdded += 1	
		}
	}	

	Write-Output "imported $numAdded paths into $fileSystemProvider database"
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
	$matchAll = $true
	$result = [Fasdr.Backend.Matcher]::Matches($global:fasdrDatabase,$providerName,$FilterContainers,$FilterLeaves,$matchAll,$ProviderPath)
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
		$Path = $results[0]
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

if ($global:fasdrDatabase -eq $null) {
	Initialize-Database
}

# import recents into database on first load:
$providerName = "FileSystem"
$location = $global:fasdrDatabase.GetProviderDatabaseLocation($providerName)
if (!(Test-Path $location)) {
	Import-Recents
}
