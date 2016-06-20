---
schema: 2.0.0
external help file: Fasdr.psm1-help.xml
---

# Find-Frecent

## SYNOPSIS
Attempts to find paths in the Fasdr database based on a string argument.

## SYNTAX
### (Default)
```
Find-Frecent [-ProviderPath] <string> [-FilterContainers] [-FilterLeaves] [-ProviderName <string>]
```

## DESCRIPTION
The Find-Frecent function finds paths and returns them based on the passed in string.

## EXAMPLES
### Find Directory

Finds directory that contains the word 'Windows'.
```powershell
Find-Frecent Windows
```

## PARAMETERS

### -ProviderPath
The string to compare against items in the Fasdr database.

```yaml
Type: string
Parameter Sets: (All)

Required: true
Position: named
Default value: none
Accept pipeline input: false
Accept wildcard characters: false
```

### -FilterContainers
Removes container paths from the results.

```yaml
Type: switch
Parameter Sets: (All)

Required: false
Position: named
Default value: false
Accept pipeline input: false
Accept wildcard characters: false
```

### -FilterLeaves
Removes leaf paths from the results.

```yaml
Type: switch
Parameter Sets: (All)

Required: false
Position: named
Default value: false
Accept pipeline input: false
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
### None
You cannot pipe objects to Find-Frecent

## OUTPUTS
### [string]
This function returns an array of strings.

## RELATED LINKS