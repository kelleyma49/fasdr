---
schema: 2.0.0
external help file: Fasdr.psm1-help.xml
---

# Set-Frecent

## SYNOPSIS
Sets the current container location and stores the location in the Fasdr database.

## SYNTAX
### (Default)
```
Set-Frecent [-Path <String>]
```

## DESCRIPTION
The Set-Frecent function sets the current container location.  If Path is not a valid path, Set-Frecent will call Find-Frecent and use the first result.
If Path is a file, Set-Frecent will use the parent directory of the file.  If Path is a valid path, the path will be saved to the Fasdr database.

## EXAMPLES
Adds the c:\Windows\ path to the current provider database.

```powershell
Set-Frecent c:\Windows\
```

## PARAMETERS

### -Path
This parameter is used to specify the path to a new working location.

```yaml
Type: string
Parameter Sets: (All)

Required: true
Position: named
Default value: None
Accept pipeline input: false
Accept wildcard characters: False
```

## INPUTS
### None
You cannot pipe objects to Set-Frecent

## OUTPUTS
### None
This cmdlet does not generate any output.

## RELATED LINKS

