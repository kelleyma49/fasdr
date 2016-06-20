---
schema: 2.0.0
external help file: Fasdr.psm1-help.xml
---

# Remove-Frecent
## SYNOPSIS
Removes a path from the Fasdr database.
## SYNTAX

```
Remove-Frecent [-FullName] <string> [-ProviderName <string>]
```

## DESCRIPTION
The Remove-Frecent function removes a path from the Fasdr database for the passed in provider.
## EXAMPLES
### Remove Directory

Removes a directory from the database.

```powershell
Remove-Frecent c:\Windows\
```

## PARAMETERS

### -FullName
The full name of the path to remove from the database.

```yaml
Type: string
Parameter Sets: (All)

Required: true
Position: named
Default value: None
Accept pipeline input: true (ByValue, ByPropertyName)
Accept wildcard characters: false

```
### -ProviderName
The name of the provider that contains the path to remove.

```yaml
Type: string
Parameter Sets: (All)

Required: false
Position: named
Default value: the name of the current working container provider
Accept pipeline input: false
Accept wildcard characters: false
```
## INPUTS
###System.String
You can pipe a string that contains a path to Remove-Frecent.

## OUTPUTS
### None
This cmdlet does not generate any output.

## RELATED LINKS
