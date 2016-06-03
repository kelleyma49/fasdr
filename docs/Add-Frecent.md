---
schema: 2.0.0
external help file: Fasdr.psm1-help.xml
---

# Add-Frecent
## SYNOPSIS
Adds a path to the Fasdr database.
## SYNTAX

```
Add-Frecent [-FullName] <string> [-ProviderName <string>]
```

## DESCRIPTION
The Add-Frecent function adds a path to the Fasdr database for the passed in provider.
## EXAMPLES
Add-Frecent c:\Windows\
## PARAMETERS

### -FullName
The full name of the path to add from the database.
```yaml
Type: string
Parameter Sets: (All)
Aliases: 

Required: True
Position: Named
Default value: none
Accept pipeline input: true (ValueFromPipeline,ValueFromPipelineByPropertyName)
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
The input type is the type of the objects that you can pipe to the cmdlet.

###System.String
You can pipe a string that contains a path to Add-Frecent.

## OUTPUTS

### None
This cmdlet does not generate any output.
## NOTES

## RELATED LINKS

[about_Fasdr]()
