---
external help file: Fasdr.psm1-help.xml
schema: 2.0.0
online version: 
---

# Get-Frecents
## SYNOPSIS
Gets the current entries from the database for a given provider.
## SYNTAX

```
Get-Frecents [[-ProviderName] <String>] [<CommonParameters>]
```

## DESCRIPTION
Returns a list of entries in the Fasdr database for given provider.  Entries include the calculated frecency value, the last access date and time, and the full path.  Modifications to the returned object will have no effect on the items in the database.
## EXAMPLES

### Example 1
Gets all entries for the FileSystem provider.


```
PS C:\> Get-Frecents -ProviderName 'FileSystem'
```

## PARAMETERS

### -ProviderName
The provider to query for entries

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: False
Position: 0
Default value: the name of the current working container provider
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).
## INPUTS

### None

## OUTPUTS

### [PSObject]
An array of objects representing the database items.
## NOTES

## RELATED LINKS

