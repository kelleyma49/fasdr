---
schema: 2.0.0
external help file: Fasdr.psm1-help.xml
---

# Add-Frecent

## SYNOPSIS
Adds a path to the Fasdr database.

## SYNTAX
### (Default)
```
Add-Frecent [-FullName] <string> [-ProviderName <string>]
```

## DESCRIPTION
The Add-Frecent function adds a path to the Fasdr database for the passed in provider.

## EXAMPLES
### Add Directory
	
Adds a directory to the database.

```powershell
Add-Frecent c:\Windows\
```

## PARAMETERS

### -FullName
The full name of the path to add from the database.

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
The name of the provider that that hosts the path.

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
You can pipe a string that contains a path to Add-Frecent.

## OUTPUTS
### None
This cmdlet does not generate any output.

## RELATED LINKS
