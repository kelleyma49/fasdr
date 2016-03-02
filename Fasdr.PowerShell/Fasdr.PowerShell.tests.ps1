#
# This is a PowerShell Unit Test file.
# You need a unit test framework such as Pester to run PowerShell Unit tests. 
# You can download Pester from http://go.microsoft.com/fwlink/?LinkID=534084
#
Import-Module $PSScriptRoot\Fasdr.PowerShell.psm1
$testData = @"
c:\dir1\dir2\testStr|101|0|true
c:\dir1\dir2|110|0|false
"@

Describe "Get-Function" {
	$testDatabase = "TestDrive:\fasdrConfig.FileSystem.txt"
	Set-Content $testDatabase -value $testData

	Init-Database $null $TestDrive

	Context "Function Exists" {
		It "Should Return" {
			Find-Frecent "shouldNotBeFound" | Should Be $null
		}
	}
}