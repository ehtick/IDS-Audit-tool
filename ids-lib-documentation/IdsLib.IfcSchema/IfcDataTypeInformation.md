# IfcDataTypeInformation class

Metadata container for entities containing measures of an IfcSchema

```csharp
public class IfcDataTypeInformation
```

## Public Members

| name | description |
| --- | --- |
| [IfcDataTypeInformation](IfcDataTypeInformation/IfcDataTypeInformation.md)(…) | Default constructor, ensures static nullable analysis (2 constructors) |
| [IfcDataTypeClassName](IfcDataTypeInformation/IfcDataTypeClassName.md) { get; } | Name of the entity as a string, stored in UPPERCASE |
| [Measure](IfcDataTypeInformation/Measure.md) { get; } | Metadata for unit of measure conversion, if relevant. |
| [ValidSchemaVersions](IfcDataTypeInformation/ValidSchemaVersions.md) { get; } | Versions of the schema that contain the class |

## See Also

* namespace [IdsLib.IfcSchema](../ids-lib.md)
* [IfcDataTypeInformation.cs](https://github.com/buildingSMART/IDS-Audit-tool/tree/main/ids-lib/IfcSchema/IfcDataTypeInformation.cs)

<!-- DO NOT EDIT: generated by xmldocmd for ids-lib.dll -->