---
schema: 2.0.0
external help file: Fasdr.psm1-help.xml
---

# Set-FrecentFromLeaf
## SYNOPSIS
This function is intended only to be used by the Tab Expansion function.

## SYNTAX

```
Set-FrecentFromLeaf [[-Path] <String>]
```

## DESCRIPTION
Set-FrecentFromLeaf works exactly like Set-Frecent, except it is not intended to be used by users.  It is used by the tab expansion functions.
## EXAMPLES
### Example 1
Adds the `c:\Windows\` path to the current provider database.

```powershell
Set-Frecent c:\Windows\
```

## PARAMETERS

### -Path
{{Fill Path Description}}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 0
Default value: 
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

### None


## OUTPUTS

### None

## NOTES

## RELATED LINKS

