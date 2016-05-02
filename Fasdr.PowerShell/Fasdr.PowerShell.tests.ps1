#
# This is a PowerShell Unit Test file.
# You need a unit test framework such as Pester to run PowerShell Unit tests. 
# You can download Pester from http://go.microsoft.com/fwlink/?LinkID=534084
#
Import-Module $PSScriptRoot\Fasdr.PowerShell.psm1 -ErrorAction Stop
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