#
# This is a PowerShell Unit Test file.
# You need a unit test framework such as Pester to run PowerShell Unit tests. 
# You can download Pester from http://go.microsoft.com/fwlink/?LinkID=534084
#
Import-Module $PSScriptRoot\Fasdr.PowerShell.psm1
$testData = @"
c:\dir1\dir2\testStr|101|0|true
c:\dir1\dir2|110|0|false
c:\testStr|110|0|false
"@

Describe "Find-Frecent" {
	$testDatabase = "TestDrive:\fasdrConfig.FileSystem.txt"
	Set-Content $testDatabase -value $testData

	Initialize-Database -defaultDrive "$TestDrive"
	
	Context "Function Exists" {
		It "Find Nothing" {
			Find-Frecent "shouldNo" | Should Be $null
		}

		It "Find Single Item" {
			Find-Frecent "dir2" | Should Be 'c:\dir1\dir2'
		}

		It "Find Double Items" {
			Find-Frecent "testStr" | Should Be ('c:\testStr','c:\dir1\dir2\testStr')
		}

		
	}
}