# fasdr [![Build Status Travis](https://travis-ci.org/kelleyma49/fasdr.svg?branch=master)](https://travis-ci.org/kelleyma49/fasdr) [![Build Status Appveyor](https://ci.appveyor.com/api/projects/status/x2wm66qujmxf2ln3?svg=true)](https://ci.appveyor.com/project/kelleyma49/fasdr) [![Coverity Scan Build Status](https://scan.coverity.com/projects/8537/badge.svg)](https://scan.coverity.com/projects/kelleyma49-fasdr) [![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/kelleyma49/fasdr/blob/master/LICENSE.md)

Fasdr (pronounced similar to "faster") is a command-line productivity booster for PowerShell.  It supports quick access to leaf and container classes for PowerShell providers.  Leaf and container paths are tracked and ranked based on frequency and date.

# Introduction
Fasdr allows you to open files or change directories by using accessing its database that it keeps of your history.  `Set-Frecent` is used to jump between paths.  You can even use tab completion to iterate through Fasdr's database.  Here are some examples:

```powershell
  j 'Files'         # cd 'c:\Program Files'
  jl 'notepad.exe'  # cd 'c:\Windows\System32'
```
`j` is aliased to `Set-Frecent`, which accepts leaves and containers.  `jl` is aliased to `Set-FrecentFromLeaf`, which forwards to `Set-Frecent`; however, tab completion will filter only leaf paths.

# Installation
Fasdr is available on the [PowerShell Gallery](https://www.powershellgallery.com/packages/Fasdr).

# Matching
Fasdr has similar matching rules to [Fasd](https://github.com/clvv/fasd#matching).

# Search Syntax
| Token      | Match type         | Description                     |
| ---------- | -----------------  | ------------------------------- |
| `^notepad` | prefix exact match | Items that start with `notepad` |
| `.cpp$`    | suffix exact match | Items that end with `.cpp`      |

# Word Completion
Fasdr has special tokens that can be used for tab completion from any command.

# Global Settings
TBD

Reference: https://github.com/clvv/fasd ; https://github.com/hugows/hf ; https://github.com/junegunn/fzf
