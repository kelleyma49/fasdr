---
external help file: Fasdr.psm1-help.xml
online version: 
schema: 2.0.0
---

# Save-FasdrDatabase
## SYNOPSIS
Saves all provider entries out to the file system.

## SYNTAX

```
Save-FasdrDatabase [-RemoveStaleEntries]
```

## DESCRIPTION
This function saves all provider entries in the current PowerShell session out to the file system.  Immediately before saving, the current process will merge databases currently saved to the file system.  This prevents the current PowerShell session from wiping previously saved entries from another PowerShell session.

## EXAMPLES

### Example 1

Saves the current in memory database out to the file system.
```powershell
PS C:\> Save-FasdrDatabase
```

## PARAMETERS

### -RemoveStaleEntries
If this switch is specified, entries that no longer exist in the provider will be removed.

```yaml
Type: switch
Parameter Sets: (All)

Required: false
Position: named
Default value: false
Accept pipeline input: false
Accept wildcard characters: false
```

## INPUTS

### None


## OUTPUTS
### None
This cmdlet does not generate any output.

## RELATED LINKS

