---
external help file: Fasdr.psm1-help.xml
schema: 2.0.0
online version: 
---

# Save-FasdrDatabase
## SYNOPSIS
Saves all provider entries out to the file system.
## SYNTAX

```
Save-FasdrDatabase [-RemoveStaleEntries] [<CommonParameters>]
```

## DESCRIPTION
This function saves all provider entries in the current PowerShell session out to the file system.  Immediately before saving, the current process will merge databases currently saved to the file system.  This prevents the current PowerShell session from wiping previously saved entries from another PowerShell session.
## EXAMPLES

### Example 1
Saves the current in memory database out to the file system.


```
PS C:\> Save-FasdrDatabase
```

## PARAMETERS

### -RemoveStaleEntries
If this switch is specified, entries that no longer exist in the provider will be removed.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: false
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).
## INPUTS

### None

## OUTPUTS

### None
This cmdlet does not generate any output.
## NOTES

## RELATED LINKS

