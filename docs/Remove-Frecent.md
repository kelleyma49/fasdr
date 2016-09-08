---
external help file: Fasdr.psm1-help.xml
schema: 2.0.0
---

# Remove-Frecent
## SYNOPSIS
Removes a path from the Fasdr database.
## SYNTAX

```
Remove-Frecent [-FullName] <String> [[-ProviderName] <String>] [<CommonParameters>]
```

## DESCRIPTION
The Remove-Frecent function removes a path from the Fasdr database for the passed in provider.
## EXAMPLES

### Remove Directory
Removes a directory from the database.



```
Remove-Frecent c:\Windows\
```

## PARAMETERS

### -FullName
The full name of the path to remove from the database.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -ProviderName
The name of the provider that contains the path to remove.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 1
Default value: the name of the current working container provider
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).
## INPUTS

### System.String
You can pipe a string that contains a path to Remove-Frecent.
## OUTPUTS

### None
This cmdlet does not generate any output.
## NOTES

## RELATED LINKS

