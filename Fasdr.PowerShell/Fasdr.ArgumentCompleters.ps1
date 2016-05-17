#
# Fasdr.ArgumentCompleters.ps1
#
function FasdrCompletion {
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)

	switch($commandName)
	{
		'Set-Frecent' { $filterContainers = $false; $filterLeaves = $true}
	}
	Find-Frecent "$wordToComplete" $filterContainers $filterLeaves |
        ForEach-Object {
			New-CompletionResult -CompletionText "$_"
        }   
}

# register for tab completion:
if (Get-Command Register-ArgumentCompleter -ea Ignore)
{
    Register-ArgumentCompleter -Command Set-Frecent -Parameter Path -ScriptBlock $function:FasdrCompletion
} else {
	Register-ArgumentCompleter -CommandName Set-Frecent -Parameter Path -ScriptBlock $function:FasdrCompletion -Description 'This argument completer handles the -Verb parameter of the Get-Verb command.'
}
