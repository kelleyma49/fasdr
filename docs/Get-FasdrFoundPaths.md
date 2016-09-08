---
external help file: Fasdr.psm1-help.xml
schema: 2.0.0
online version: 
---

# Get-FasdrFoundPaths
## SYNOPSIS
Returns an array that contains the found paths in the last command line.
## SYNTAX

```
Get-FasdrFoundPaths [<CommonParameters>]
```

## DESCRIPTION
Returns an array that contains the found paths in the last command line.  The Fasdr prompt override, which is installed by default, must be installed for this to operate correctly.
## EXAMPLES

### Example 1
Outputs array of last found paths.


```
PS C:\> Get-FasdrFoundPaths
```

## PARAMETERS

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).
## INPUTS

### None

## OUTPUTS

### [PSObject]
An array of objects representing the database items.
## NOTES

## RELATED LINKS

