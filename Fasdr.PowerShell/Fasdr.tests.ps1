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
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Finds Nothing" {
			Find-Frecent "shouldNo" | Should Be $null
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData

		Initialize-FasdrDatabase -defaultDrive "$TestDrive"

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
		
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Adds File Entry That Does Not Exist in Database" {
			{ Add-Frecent $testFile } | Should Not Throw
			Find-Frecent "TestFile1.txt" | Should Be ( $testFile )
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"

		It "Should Find Different Order" {
			Find-Frecent "testStr" | Should Be ('c:\testStr','c:\dir1\dir2\testStr')
			{ Add-Frecent 'c:\dir1\dir2\testStr' } | Should Not Throw
			Find-Frecent "testStr" | Should Be ('c:\dir1\dir2\testStr','c:\testStr')
		}

		It "Should Find New Entry" {
			{ Add-Frecent 'c:\dir1\BrandNewEntry' } | Should Not Throw
			Find-Frecent "BrandNewEntry" | Should Be 'c:\dir1\BrandNewEntry'
		}

		It "Should Find Entries Set By Array" {
			New-Item -ItemType Directory "$TestDrive\AddFrecent1"
			New-Item -ItemType Directory "$TestDrive\AddFrecent2"
			{ gci "$TestDrive\AddFrecent*" | Add-Frecent } | Should Not Throw
			Find-Frecent "AddFrecent1" | Should Be ("$TestDrive\AddFrecent1","$TestDrive\AddFrecent2")
			Find-Frecent "AddFrecent2" | Should Be ("$TestDrive\AddFrecent2","$TestDrive\AddFrecent1")
		}
	}
}

Describe "Remove-Frecent" {
	Context "Empty Database" {
		$testFile = "TestDrive:\TestFile1.txt"
		Set-Content $testFile -value " "
		
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Removes File Entry That Does Not Exist in Database" {
			{ Remove-Frecent $testFile } | Should Throw
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"

		It "Should Find Entry Removed" {
			Find-Frecent "testStr" | Should Be ('c:\testStr','c:\dir1\dir2\testStr')
			Remove-Frecent 'c:\dir1\dir2\testStr' 
			{ Remove-Frecent 'c:\dir1\dir2\testStr'}  | Should Throw
			Find-Frecent "testStr" | Should Be ('c:\testStr')
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"

		It "Should Remove Entries By Array" {
			@('c:\testStr','c:\dir1\dir2\testStr') | Remove-Frecent
			Find-Frecent "testStr" | Should Be $null
			{ @('c:\testStr','c:\dir1\dir2\testStr') | Remove-Frecent } | Should Throw
		}
	}
}

Describe "Save-FasdrDatabase" {
	Context "Empty Database" {
		$testFile = "TestDrive:\TestFile1.txt"
		Set-Content $testFile -value " "
		
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Save From Empty Database" {
			{ Add-Frecent $testFile } | Should Not Throw
			Save-FasdrDatabase 
			$testDatabase | Should Exist	
			$testDatabase | Should Contain "$testFile|1|.*|true".Replace('\','\\')
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Save From Small Database" {
			Save-FasdrDatabase 
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

			It 'Unknown Token' {
				Find-WordCompletion ' u::' | Should Be $null
			}
		}

		Context "Token Found" {
			It 'Find All Completion' {
				$result = Find-WordCompletion ' :::'
				$result | Should Not Be $null
				$Result.CompletionText | Should be ''
				$result.CompletionType | Should be ':'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length)
			}

			It 'Find All Completion with Completion String' {
				$result = Find-WordCompletion ' :::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be ':'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
			}

			It 'Find Container (uppercase) Completion with Completion String' {
				$result = Find-WordCompletion ' C::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'c'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
			}

			It 'Find Container (lowercase) Completion with Completion String' {
				$result = Find-WordCompletion ' C::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'c'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
			}

			It 'Find Leaf (uppercase) Completion with Completion String' {
				$result = Find-WordCompletion ' L::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'l'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
			}

			It 'Find Leaf (lowercase) Completion with Completion String' {
				$result = Find-WordCompletion ' L::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'l'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
			}
		}
	}
}


Describe "FindPathsInLastCommand" {
	InModuleScope Fasdr {
		Context "Blank Line" {
			Mock Get-History { $l = New-Object -TypeName PSObject ; $l | Add-Member -MemberType NoteProperty -Name CommandLine -Value ''; return $l }
			Mock Add-Frecent {}
			It 'Processes empty history line' {
				{ FindPathsInLastCommand -PrevLocation "$env:TEMP" } | Should Not Throw
				Assert-MockCalled Add-Frecent -Exactly 0
			}
		}

		Context "Cd Command Absolute Path" {
			$cmd = 'cd {0}' -f $env:windir
			Mock Get-History { $l = New-Object -TypeName PSObject ; $l | Add-Member -MemberType NoteProperty -Name CommandLine -Value $cmd; return $l }
			Mock Add-Frecent {}
			It 'Processes correctly' {
				{ FindPathsInLastCommand -PrevLocation $env:TEMP } | Should Not Throw
				Assert-MockCalled Add-Frecent -Exactly 1 -ParameterFilter {$FullName -eq $env:windir ; $ProviderName -eq 'FileSystem'}
			}
		}

		'.\windows', 'windows' | Foreach-Object { 
			Context "Cd Command Relative Path $_" {
					$cmd = 'cd {0}' -f $_
					Mock Get-History { $l = New-Object -TypeName PSObject ; $l | Add-Member -MemberType NoteProperty -Name CommandLine -Value $cmd; return $l }
					Mock Add-Frecent {}
					It "Processes '$cmd' correctly" {
						{ FindPathsInLastCommand -PrevLocation "$env:SYSTEMDRIVE\" } | Should Not Throw
						Assert-MockCalled Add-Frecent -Exactly 1 -ParameterFilter {$FullName -eq "$env:SYSTEMDRIVE\windows" -and $ProviderName -eq 'FileSystem'}
					}
			}
		}

		Context "Multiple File Paths" {
			$cmd = 'copy-item -Path {0} -Destination "{1}"' -f $env:windir,$env:ProgramData
			Mock Get-History { $l = New-Object -TypeName PSObject ; $l | Add-Member -MemberType NoteProperty -Name CommandLine -Value $cmd; return $l }
			Mock Add-Frecent {}
			It "Processes '$cmd' correctly" {
				{ FindPathsInLastCommand -PrevLocation $env:TEMP } | Should Not Throw
				Assert-MockCalled Add-Frecent -Exactly 1 -ParameterFilter {$FullName -eq "$env:windir" -and $ProviderName -eq 'FileSystem'}
				Assert-MockCalled Add-Frecent -Exactly 1 -ParameterFilter {$FullName -eq "$env:ProgramData" -and $ProviderName -eq 'FileSystem'}
			}
		}

		Context "Multiple File Paths With Non-Path Arg" {
			$cmd = 'copy-item -Path {0} -Destination "{1}"' -f "::NotAPath::",$env:ProgramData
			Mock Get-History { $l = New-Object -TypeName PSObject ; $l | Add-Member -MemberType NoteProperty -Name CommandLine -Value $cmd; return $l }
			Mock Add-Frecent {}
			It "Processes '$cmd' correctly" {
				{ FindPathsInLastCommand -PrevLocation $env:TEMP } | Should Not Throw
				Assert-MockCalled Add-Frecent -Exactly 1 -ParameterFilter {$FullName -eq "$env:ProgramData" -and $ProviderName -eq 'FileSystem'}
			}
		}
	}
}