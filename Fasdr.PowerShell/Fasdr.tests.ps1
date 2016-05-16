#
# This is a PowerShell Unit Test file.
# You need a unit test framework such as Pester to run PowerShell Unit tests. 
# You can download Pester from http://go.microsoft.com/fwlink/?LinkID=534084
#
Get-Module Fasdr | Remove-Module
Import-Module $PSScriptRoot\Fasdr.psm1 -ErrorAction Stop
$testData = @"
c:\dir1\dir2\testStr|109|0|true
c:\dir1\dir2|110|0|false
c:\testStr|110|0|false
"@
$testDatabase = "TestDrive:\fasdrConfig.FileSystem.txt"

Describe "Find-Frecent" {
	Context "Empty Database" {
		Initialize-Database -defaultDrive "$TestDrive"
	
		It "Finds Nothing" {
			Find-Frecent "shouldNo" | Should Be $null
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData

		Initialize-Database -defaultDrive "$TestDrive"

		It "Finds All With Empty Input" {
			$expected = $testData.Split("`n")
			$i = 0
			Find-Frecent "" | % { $_ | Should Be $expected[$i].Split('|')[0] ; $i++ }
		}

		It "Finds Nothing" {
			Find-Frecent "shouldNo" | Should Be $null
		}

		It "Finds Single Item" {
			Find-Frecent "dir2" | Should Be 'c:\dir1\dir2'
		}

		It "Finds Double Items" {
			Find-Frecent "testStr" | Should Be ('c:\testStr','c:\dir1\dir2\testStr')
		}
	}
}


Describe "Add-Frecent" {
	Context "Empty Database" {
		$testFile = "TestDrive:\TestFile1.txt"
		Set-Content $testFile -value " "
		
		Initialize-Database -defaultDrive "$TestDrive"
	
		It "Adds File Entry That Does Not Exist in Database" {
			Add-Frecent $testFile | Should Be $true
			Find-Frecent "TestFile1.txt" | Should Be ( $testFile )
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData
		Initialize-Database -defaultDrive "$TestDrive"

		It "Should Find Different Order" {
			Find-Frecent "testStr" | Should Be ('c:\testStr','c:\dir1\dir2\testStr')
			Add-Frecent 'c:\dir1\dir2\testStr' | Should Be $true
			Find-Frecent "testStr" | Should Be ('c:\dir1\dir2\testStr','c:\testStr')
		}

		It "Should Find New Entry" {
			Add-Frecent 'c:\dir1\BrandNewEntry' | Should Be $true
			Find-Frecent "BrandNewEntry" | Should Be 'c:\dir1\BrandNewEntry'
		}

	}
}

Describe "Save-Database" {
	Context "Empty Database" {
		$testFile = "TestDrive:\TestFile1.txt"
		Set-Content $testFile -value " "
		
		Initialize-Database -defaultDrive "$TestDrive"
	
		It "Save From Empty Database" {
			Add-Frecent $testFile | Should Be $true
			Save-Database 
			$testDatabase | Should Exist	
			$testDatabase | Should Contain "$testFile|1|.*|true".Replace('\','\\')
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData
		Initialize-Database -defaultDrive "$TestDrive"
	
		It "Save From Small Database" {
			Save-Database 
			$testDatabase | Should Exist	
			$testData.Split('`n') | ForEach-Object {
				$testDatabase | Should Contain $_.Replace('\','\\')
			}
			
		}
	}
}


Describe "Find-WordCompletion" {
	InModuleScope Fasdr {
		Context "No Token" {
			It 'Empty Text' {
				Find-WordCompletion '' | Should Be $null
			}

			It 'Whitespace Only' {
				Find-WordCompletion ' ' | Should Be $null
			}

			
			It 'Single Colon' {
				Find-WordCompletion ' :' | Should Be $null
			}
		}

		Context "Token Found" {
			It 'Find All Completion' {
				$result = Find-WordCompletion ' :::'
				$result | Should Not Be $null
				$Result.CompletionText | Should be ''
				$result.CompletionType | Should be ''
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be 2
			}
		}
	}
}