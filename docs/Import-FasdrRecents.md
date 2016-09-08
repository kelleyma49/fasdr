---
external help file: Fasdr.psm1-help.xml
schema: 2.0.0
online version: 
---

# Import-FasdrRecents
## SYNOPSIS
Imports container and leaf paths from various areas in the Windows operating system.
## SYNTAX

```
Import-FasdrRecents [<CommonParameters>]
```

## DESCRIPTION
Imports containers and leaves based on stored locations in the Windows operating system.  This function currently only works with the FileSystem provider, and it pulls items from application jump lists, the Recents folder, and Windows Special Folders.
## EXAMPLES

### Example 1
Imports entries into the Fasdr databases for the currently known providers.


```
PS C:\> Import-FasdrRecents
```

## PARAMETERS

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).
## INPUTS

### None

## OUTPUTS

### None

## NOTES

## RELATED LINKS

