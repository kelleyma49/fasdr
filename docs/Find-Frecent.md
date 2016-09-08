---
external help file: Fasdr.psm1-help.xml
schema: 2.0.0
---

# Find-Frecent
## SYNOPSIS
Attempts to find paths in the Fasdr database based on a string argument.
## SYNTAX

```
Find-Frecent [[-ProviderPath] <String>] [[-FilterContainers] <Boolean>] [[-FilterLeaves] <Boolean>]
 [[-ProviderName] <String>] [<CommonParameters>]
```

## DESCRIPTION
The Find-Frecent function finds paths and returns them based on the passed in string.
## EXAMPLES

### Find Directory
Finds directory that contains the word 'Windows'.


```
Find-Frecent Windows
```

## PARAMETERS

### -FilterContainers
Removes container paths from the results.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: 1
Default value: false
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilterLeaves
Removes leaf paths from the results.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases: 

Required: False
Position: 2
Default value: false
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProviderName
The name of the provider that that hosts the path.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 3
Default value: the name of the current working container provider
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProviderPath
The string to compare against items in the Fasdr database.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 0
Default value: none
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).
## INPUTS

### None
You cannot pipe objects to Find-Frecent
## OUTPUTS

### [string]
This function returns an array of strings.
## NOTES

## RELATED LINKS

