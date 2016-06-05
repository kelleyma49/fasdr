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
Fasdr is available on the [PowerShell Gallery](https://www.powershellgallery.com/packages/Fasdr). Note that it will hook into the current prompt and tab completion functions for proper support of word completion mode.

# Matching
Fasdr has similar matching rules to [Fasd](https://github.com/clvv/fasd#matching).  Exact matches between the search string and the last element of items stored in the database are returned first; then fuzzy matches are returned.

# Search Syntax
Fasdr's search syntax supports prefix, suffix, and current directory searches.

| Token Example | Match type                              | Description                                          |
| ------------- | --------------------------------------  | ---------------------------------------------------- | 
| `^notepad`    | prefix exact match                      | Items that start with `notepad`                      |
| `.cpp$`       | suffix exact match                      | Items that end with `.cpp`                           |
| `**Documents` | match against current working directory | Items that exist below the current working directory |

# Word Completion
Fasdr has special tokens that can be used for tab completion from any command.

| Token Example         | Match type                      | Description                        |
| ------------- | --------------------------------------  | ---------------------------------- | 
| `:::Windows`  | containers and leaves                   | Items that contain Windows         |
| `c::Windows`  | containers only                         | Containers that contain  `Windows` |
| `l::txt`      | leaves only                             | Leaves that contain `txt`          |


# Global Settings
TBD

Reference: https://github.com/clvv/fasd ; https://github.com/hugows/hf ; https://github.com/junegunn/fzf
