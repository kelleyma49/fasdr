---
external help file: Fasdr.psm1-help.xml
schema: 2.0.0
---

# Set-Frecent
## SYNOPSIS
Sets the current container location and stores the location in the Fasdr database.
## SYNTAX

```
Set-Frecent [[-Path] <String>] [<CommonParameters>]
```

## DESCRIPTION
The Set-Frecent function sets the current container location.  If Path is not a valid path, Set-Frecent will call Find-Frecent and use the first result.
If Path is a file, Set-Frecent will use the parent directory of the file.  If Path is a valid path, the path will be saved to the Fasdr database.
If path is $null or whitespace, Set-Frecent shows a menu that allows the user to select a parent path relative to the current location.
## EXAMPLES

### Example 1
Adds the c:\Windows\ path to the current provider database.



```
Set-Frecent c:\Windows\
```

## PARAMETERS

### -Path
This parameter is used to specify the path to a new working location.

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).
## INPUTS

### None
You cannot pipe objects to Set-Frecent
## OUTPUTS

### None
This cmdlet does not generate any output.
## NOTES

## RELATED LINKS

