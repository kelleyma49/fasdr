#
# This is a PowerShell Unit Test file.
# You need a unit test framework such as Pester to run PowerShell Unit tests. 
# You can download Pester from http://go.microsoft.com/fwlink/?LinkID=534084
#
Get-Module Fasdr | Remove-Module
Import-Module $PSScriptRoot\Fasdr.psm1 -ErrorAction Stop
$testData = @"
c:\dir1\dir2\testStr|109|0|true
c:\dir1\dir2|111|0|false
c:\testStr|110|0|false
"@
$testDataTestDrive = @"
TestDrive:\dir1\dir2\testStr|109|0|true
TestDrive:\dir1\dir2|111|0|false
TestDrive:\testStr|110|0|false
"@
$testDatabase = "TestDrive:\db.FileSystem.txt"

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
			$frecents = Find-Frecent "" 
			$frecents[0] | Should Be 'c:\dir1\dir2\testStr'
			$frecents[1] | Should Be 'c:\testStr'
			$frecents[2] | Should Be 'c:\dir1\dir2'
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
	Context "Empty Database Add File" {
		$testFile = "TestDrive:\TestFile1.txt"
		Set-Content $testFile -value " "
		
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Adds File Entry That Does Not Exist in Database" {
			{ Add-Frecent $testFile } | Should Not Throw
			Find-Frecent "TestFile1.txt" | Should Be ( $testFile )
		}

	}

	Context "Empty Database Add Relative Entry" {
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Add Relative Entry" {
			New-Item "TestDrive:\AddRelativeEntryCheck" -ItemType Directory
			New-Item "TestDrive:\AddRelativeEntryCheck\TestDir" -ItemType Directory 
			Push-Location
			Set-Location "TestDrive:\AddRelativeEntryCheck"
			{ Add-Frecent '.\TestDir' } | Should Not Throw
			Find-Frecent "TestDir" | Should Be ("TestDrive:\AddRelativeEntryCheck\TestDir")
			Pop-Location
		}

		It "Relative Entry Doesnt Exist" {
			New-Item "TestDrive:\AddRelativeEntryCheck" -ItemType Directory
			Push-Location
			Set-Location "TestDrive:\AddRelativeEntryCheck"
			{ Add-Frecent '.\DoesNotExist' } | Should Throw
			Find-Frecent "DoesNotExist" | Should Be $Null
			Pop-Location
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

	Context "Small Database Remove All Stale Entries" {
		Set-Content $testDatabase -value $testDataTestDrive
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Save From Small Database Remove Stale Entries" {
			Save-FasdrDatabase -RemoveStaleEntries
			$testDatabase | Should Exist	
			$testDataTestDrive.Split('`n') | ForEach-Object {
				$testDatabase | Should Not Contain $_.Replace('\','\\')
			}
		}
	}

	Context "Small Database Remove All Stale Entries Except One" {
		Set-Content $testDatabase -value $testDataTestDrive
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
		$testFile = "TestDrive:\dir1\dir2\testStr"
		New-Item (Split-Path $testFile -Parent) -ItemType Directory
		Out-File -FilePath $testFile -InputObject " "
	
		It "Save From Small Database Remove Stale Entries" {
			Save-FasdrDatabase -RemoveStaleEntries
			$testDatabase | Should Exist	
			$testDatabase | Should ContainExactly $testFile.Replace('\','\\')
		}
	}
}

Describe "Get-Frecents" {
	Context "Empty Database" {
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"
	
		It "Get From Empty Database" {
			Get-Frecents | Should Be $null
		}
	}

	Context "Small Database" {
		Set-Content $testDatabase -value $testData
		Initialize-FasdrDatabase -defaultDrive "$TestDrive"

		It "Get From Database" {
			$frecents = Get-Frecents 
			$frecents | Should Not Be $null
			$frecents.Length | Should Be 3
			$frecents[0].FullPath | Should Be 'c:\dir1\dir2'
			$frecents[0].Frecency | Should BeGreaterThan 1
			$frecents[1].FullPath | Should Be 'c:\testStr'
			$frecents[1].Frecency | Should BeGreaterThan 1
			$frecents[2].FullPath | Should Be 'c:\dir1\dir2\testStr'
			$frecents[2].Frecency | Should BeGreaterThan 1
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
				$result.ProviderOverride | Should be $null
			}

			It 'Find All Completion with Completion String' {
				$result = Find-WordCompletion ' :::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be ':'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be $null
			}

			It 'Find Container (uppercase) Completion with Completion String' {
				$result = Find-WordCompletion ' C::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'c'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be $null
			}

			It 'Find Container (lowercase) Completion with Completion String' {
				$result = Find-WordCompletion ' C::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'c'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be $null
			}

			It 'Find Leaf (uppercase) Completion with Completion String' {
				$result = Find-WordCompletion ' L::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'l'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be $null
			}

			It 'Find Leaf (lowercase) Completion with Completion String' {
				$result = Find-WordCompletion ' L::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'l'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be $null
			}

			# FileSystem provider specific
			It 'Find Directory (uppercase) Completion with Completion String' {
				$result = Find-WordCompletion ' D::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'd'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be 'FileSystem'
			}

			It 'Find Directory (lowercase) Completion with Completion String' {
				$result = Find-WordCompletion ' D::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'd'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be 'FileSystem'
			}

			It 'Find File (uppercase) Completion with Completion String' {
				$result = Find-WordCompletion ' F::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'f'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be 'FileSystem'
			}

			It 'Find File (lowercase) Completion with Completion String' {
				$result = Find-WordCompletion ' F::Desktop'
				$result | Should Not Be $null
				$Result.CompletionText | Should be 'Desktop'
				$result.CompletionType | Should be 'f'
				$result.ReplacementIndex | Should be 1
				$result.ReplacementLength | Should be (':::'.Length + 'Desktop'.Length)
				$result.ProviderOverride | Should be 'FileSystem'
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
				(Get-FasdrFoundPaths).Length | Should be 0				
			}
		}

		Context "Cd Command Absolute Path" {
			$cmd = 'cd {0}' -f $env:windir
			Mock Get-History { $l = New-Object -TypeName PSObject ; $l | Add-Member -MemberType NoteProperty -Name CommandLine -Value $cmd; return $l }
			Mock Add-Frecent {}
			It 'Processes correctly' {
				{ FindPathsInLastCommand -PrevLocation $env:TEMP } | Should Not Throw
				Assert-MockCalled Add-Frecent -Exactly 1 -ParameterFilter {$FullName -eq $env:windir ; $ProviderName -eq 'FileSystem'}
				(Get-FasdrFoundPaths).Length | Should be 1
				(Get-FasdrFoundPaths)[0] | Should be "$env:windir"
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
						(Get-FasdrFoundPaths).Length | Should be 1
						(Get-FasdrFoundPaths)[0] | Should be "$env:SYSTEMDRIVE\windows"
					}
			}
		}

		Context "Home Directory" {
				$cmd = 'cd ~'
				Mock Get-History { $l = New-Object -TypeName PSObject ; $l | Add-Member -MemberType NoteProperty -Name CommandLine -Value $cmd; return $l }
				Mock Add-Frecent {}
				It "Processes '$cmd' correctly" {
					{ FindPathsInLastCommand -PrevLocation "~" } | Should Not Throw
					Assert-MockCalled Add-Frecent -Exactly 1 -ParameterFilter {$FullName -eq "$env:USERPROFILE" -and $ProviderName -eq 'FileSystem'}
					(Get-FasdrFoundPaths).Length | Should be 1
					(Get-FasdrFoundPaths)[0] | Should be "$env:USERPROFILE"
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
				(Get-FasdrFoundPaths).Length | Should be 2
				(Get-FasdrFoundPaths)[0] | Should be "$env:windir"
				(Get-FasdrFoundPaths)[1] | Should be "$env:ProgramData"
			}
		}

		Context "Multiple File Paths With Non-Path Arg" {
			$cmd = 'copy-item -Path {0} -Destination "{1}"' -f "::NotAPath::",$env:ProgramData
			Mock Get-History { $l = New-Object -TypeName PSObject ; $l | Add-Member -MemberType NoteProperty -Name CommandLine -Value $cmd; return $l }
			Mock Add-Frecent {}
			It "Processes '$cmd' correctly" {
				{ FindPathsInLastCommand -PrevLocation $env:TEMP } | Should Not Throw
				Assert-MockCalled Add-Frecent -Exactly 1 -ParameterFilter {$FullName -eq "$env:ProgramData" -and $ProviderName -eq 'FileSystem'}
				(Get-FasdrFoundPaths).Length | Should be 1
				(Get-FasdrFoundPaths)[0] | Should be "$env:ProgramData"
			}
		}
	}
}