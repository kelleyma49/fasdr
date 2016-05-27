# load C# dll for backend:
$dllPath = Join-Path $PSScriptRoot 'Fasdr.Backend.dll'
[System.Reflection.Assembly]::LoadFrom($dllPath)
$dllPath = Join-Path $PSScriptRoot 'System.IO.Abstractions.dll'
[System.Reflection.Assembly]::LoadFile($dllPath)
$global:fasdrDatabase = $null

# configuration vars
$global:Fasdr = @{
	MaxResults = 50
}

#region prompt
$global:oldPrompt = $function:prompt
$global:fasdrPrevLocation = $null
$global:fasdrPrevArgs = @(@(),@(),@(),@(),@(),@(),@(),@(),@(),@())

function FindPathsInLastCommand
{
	param([string]$PrevLocation = $null)

	if (-not [string]::IsNullOrWhiteSpace($PrevLocation)) {
        $global:fasdrPrevLocation = $PrevLocation
	}
	if ($global:fasdrPrevLocation -ne $null) {
        $lastHistory = Get-History -Count 1
		$lastCommand = $lastHistory.CommandLine   

		$foundArgs = @()

		[System.Management.Automation.PsParser]::Tokenize($lastCommand, [ref] $null) |
			Where-Object {$_.type -eq "commandargument" -or $_.type -eq "string"} | foreach-object {
				$path = $_.Content
        		if (Split-Path $path -IsAbsolute) {
					$foundPath = Resolve-Path $path -ErrorAction SilentlyContinue
 				} else {
					# attempt to find the path in the prev directory:
                    $foundPath = Resolve-Path (Join-Path $global:fasdrPrevLocation $path) -ErrorAction SilentlyContinue	
				}

				if ($null -ne $foundPath) {
					Add-Frecent $foundPath.Path $foundPath.Provider.Name | out-null
					$foundArgs += $foundPath
				}
			}

			#$global:fasdrPrevArgs = $foundArgs + $global:fasdrPrevArgs[0..8]
	}

	$global:fasdrPrevLocation = (Get-Location).Path
}

function global:prompt
{
	# parse history; attempt to find paths for leaves and directories:
	try 
	{
		FindPathsInLastCommand		
	}
	catch
	{
		# ignore errors
	}
	finally
	{
		(& $oldPrompt @PSBoundParameters)
	}
}
#endregion

#region TabExpansion
# Save off the previous tab completion so it can be restored if this module
# is removed.
$global:oldTabExpansion = $function:TabExpansion
$global:oldTabExpansion2 = $function:TabExpansion2

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

			$findWord = Find-WordCompletion $ast.Extent.Text
			if ($findWord -ne $null) {
				$results.ReplacementIndex = $findWord.ReplacementIndex
				$results.ReplacementLength = $findWord.ReplacementLength
				switch ($findWord.CompletionType) 
				{
					':' { $FilterContainers = $FilterLeaves = $false }
					'c' { $FilterContainers = $false; $FilterLeaves = $true }
					'l' { $FilterContainers = $true; $FilterLeaves = $false }
				}
				Find-Frecent -ProviderPath $findWord.CompletionText -FilterContainers $FilterContainers -FilterLeaves $FilterLeaves | 
					Select-Object -First $global:Fasdr.MaxResults | ForEach-Object {
					$textCompletion = $_

					# taken from https://github.com/lzybkr/TabExpansionPlusPlus/blob/master/TabExpansionPlusPlus.psm1
					# Add single quotes for the caller in case they are needed.
					# We use the parser to robustly determine how it will treat
					# the argument.  If we end up with too many tokens, or if
					# the parser found something expandable in the results, we
					# know quotes are needed.
					$tokens = $null
					$null = [System.Management.Automation.Language.Parser]::ParseInput("echo $textCompletion", [ref]$tokens, [ref]$null)
					if ($tokens.Length -ne 3 -or
						($tokens[1] -is [System.Management.Automation.Language.StringExpandableToken] -and
						 $tokens[1].Kind -eq [System.Management.Automation.Language.TokenKind]::Generic))
					{
						$textCompletion = "'$textCompletion'"
					}
				
					$results.CompletionMatches.Add(
						(New-Object Management.Automation.CompletionResult $textCompletion,$_,"Text",$_)
					)
				}
			}
		}
	}

	return $results
}

function Find-WordCompletion {
	param([string]$text)

	$foundToken = $text -match '(\s)(\w|:)::(.*)' 
	if ($foundToken)
	{
		$space = $matches[1]
		$compType = $matches[2].Trim().ToLower()
		$compText = $matches[3].Trim()
		if ($compText -eq $null) {
			$compText = ''
		}

		# find bounds of text to replace:
		$searchStr = '{0}{1}::' -f $space,$compType
		$index = $text.ToLower().IndexOf($searchStr) + 1
		for ($i=$index;$i -lt $text.Length;$i++) {
			if ([char]::IsWhiteSpace($text[$i])) {
				break
			}
		}		

		# make sure that we have an acceptable token:
		switch ($compType) {
			':'    { break } # all values
			'c'   { break } # container
			'l'   { break } # leaves
 			default { $foundToken = $false }
		}

		if ($foundToken) {
			return New-Object PsObject -Property @{
				CompletionText=$compText ; 
				CompletionType=$compType ;
				ReplacementIndex=$index
				ReplacementLength=$i-$index}
		}
	}		

	return $null
}

#endregion

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

	if ($null -ne $oldPrompt)
	{
		Set-Item Function:\prompt $oldPrompt
	}
}

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
	param(
		[parameter(Mandatory,ValueFromPipeline,ValueFromPipelineByPropertyName)]
		[string]$FullName,
		[string]$ProviderName = $PWD.Provider.Name)

	Begin {
		if ($global:fasdrDatabase -eq $null) {
			Initialize-Database
		}
	}
	
    Process {
		if (!$global:fasdrDatabase.AddEntry($ProviderName,$FullName,[System.Predicate[string]]{param($fullPath) Test-Path $fullPath -PathType Leaf})) {
			throw ("Failed to add '{0}' for provider '{1}'" -f $FullName,$ProviderName)
		}
	}
}

<#
	Remove-Frecent
#>
function Remove-Frecent {
	param(
		[parameter(Mandatory,ValueFromPipeline,ValueFromPipelineByPropertyName)]
		[string]$FullName,
		[string]$ProviderName = $PWD.Provider.Name)

	Begin {
		$errors = @()
		if ($global:fasdrDatabase -eq $null) {
			Initialize-Database
		}
	}
	
    Process {
		if (!$global:fasdrDatabase.RemoveEntry($ProviderName,$FullName)) {
			throw ("Failed to remove '{0}' for provider '{1}'" -f $FullName,$ProviderName)
		}
	}

	End {
	}
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
	
	# if it's a file, goto the file's parent directory:
	if (Test-Path $Path -PathType Leaf) {
		$Path = Split-Path $Path -Parent
	}

	Set-Location $Path
	if ($?) {
		$Path = (Get-Item -Path ".\" -Verbose).FullName
		Add-Frecent $Path
		Save-Database
	} 
}

<# This exists only for tab completion purposes #>
function Set-FrecentFromLeaf {
	param([string]$Path)

	Set-Frecent $Path
}

if (((Get-Module TabExpansionPlusPlus) -ne $null) -or ($PSVersionTable.PsVersion.Major -ge 5)) {
	foreach ($file in dir $PSScriptRoot\*.ArgumentCompleters.ps1)
	{
		. $file.FullName
	}
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

New-Alias j Set-Frecent
New-Alias jl Set-FrecentFromLeaf

Export-ModuleMember -Function   Find-Frecent,`
								Initialize-Database,`
								Add-Frecent,`
								Set-Frecent,`
								Set-FrecentFromLeaf,`
								Remove-Frecent,`
								Save-Database,`
								Import-Recents

Export-ModuleMember -Alias      j,jl
