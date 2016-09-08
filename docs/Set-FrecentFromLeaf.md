---
external help file: Fasdr.psm1-help.xml
schema: 2.0.0
---

# Set-FrecentFromLeaf
## SYNOPSIS
This function is intended only to be used by the Tab Expansion function.
## SYNTAX

```
Set-FrecentFromLeaf [[-Path] <String>] [<CommonParameters>]
```

## DESCRIPTION
Set-FrecentFromLeaf works exactly like Set-Frecent, except it is not intended to be used by users.  It is used by the tab expansion functions.
## EXAMPLES

### Example 1
Adds the `c:\Windows\` path to the current provider database.



```
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).
## INPUTS

### None

## OUTPUTS

### None

## NOTES

## RELATED LINKS

