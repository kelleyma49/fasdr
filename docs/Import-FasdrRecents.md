---
external help file: Fasdr.psm1-help.xml
online version: 
schema: 2.0.0
---

# Import-FasdrRecents
## SYNOPSIS
Imports container and leaf paths from various areas in Windows.

## SYNTAX

```
Import-FasdrRecents
```

## DESCRIPTION
Imports containers and leaves based on stored locations in the Windows operating system.  This function currently only works with the FileSystem provider, and it pulls items from application jump lists, the Recents folder, and Windows Special Folders.

## EXAMPLES

### Example 1
```
PS C:\> Import-FasdrRecents
```

Imports entries into the Fasdr databases for the currently known providers.

## PARAMETERS

## INPUTS

### None


## OUTPUTS

### None

## NOTES

## RELATED LINKS

