#
# Fasdr.ArgumentCompleters.ps1
#
function FasdrCompletion {
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)

	switch($commandName)
	{
		'Set-Frecent'         { $filterContainers = $false; $filterLeaves = $true}
		'Set-FrecentFromLeaf' { $filterContainers = $true; $filterLeaves = $false}
	}
	Find-Frecent "$wordToComplete" $filterContainers $filterLeaves | Select-Object -First $global:Fasdr.MaxResults |
        ForEach-Object {
			New-CompletionResult -CompletionText "$_"
        }   
}

# register for tab completion:
if (Get-Command Register-ArgumentCompleter -ea Ignore)
{
    Register-ArgumentCompleter -Command Set-Frecent -Parameter Path -ScriptBlock $function:FasdrCompletion
	Register-ArgumentCompleter -Command Set-FrecentFromLeaf -Parameter Path -ScriptBlock $function:FasdrCompletion
} else {
	Register-ArgumentCompleter -CommandName Set-Frecent -Parameter Path -ScriptBlock $function:FasdrCompletion -Description 'This argument completer handles the -Verb parameter of the Get-Verb command.'
	Register-ArgumentCompleter -CommandName Set-FrecentFromLeaf -Parameter Path -ScriptBlock $function:FasdrCompletion -Description 'This argument completer handles the -Verb parameter of the Get-Verb command.'
}
