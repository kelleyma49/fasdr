---
schema: 2.0.0
external help file: Fasdr.psm1-help.xml
---

# Find-Frecent
## SYNOPSIS
Attempts to find paths in the Fasdr database based on a string argument.
## SYNTAX

```
Find-Frecent [-ProviderPath] <string> [-FilterContainers] [-FilterLeaves] [-ProviderName <string>]
```

## DESCRIPTION
The Find-Frecent function finds paths and returns them based on the passed in string.
## EXAMPLES
Find-Frecent Windows
## PARAMETERS

### -ProviderPath
The string to compare against items in the Fasdr database.
```yaml
Type: string
Parameter Sets: (All)
Aliases: 

Required: True
Position: Named
Default value: none
Accept pipeline input: False
Accept wildcard characters: False

```

### -FilterContainers
Removes container paths from the results.
```yaml
Type: switch
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilterLeaves
Removes leaf paths from the results.
```yaml
Type: switch
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProviderName
The name of the provider that that hosts the path.
```yaml
Type: string
Parameter Sets: (All)
Aliases: 

Required: False
Position: Named
Default value: the name of the current working container provider
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

### None
You cannot pipe objects to Find-Frecent

## OUTPUTS

### [string]
This function returns an array of strings.
## NOTES

## RELATED LINKS

[about_Fasdr]()
