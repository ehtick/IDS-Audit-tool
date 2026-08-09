using Xbim.Common.Metadata;
using Xbim.Properties;

namespace IdsLib.codegen;

internal class TypeMapper
{
	internal string IdsName { get; }
	internal ExpressType IfcMapToExpressType { get; }

	public TypeMapper(string ifcName, ExpressMetaData metaData)
	{
		IdsName = ifcName;
		if (!metaData.TryGetExpressType(ifcName.ToUpperInvariant(), out var expressType))
			throw new Exception($"Could not find express type for {ifcName} in schema.");
		IfcMapToExpressType = expressType;
	}

	internal static List<TypeMapper> GetFor(string schema, List<IfcSchema_Ifc2x3MapperGenerator.Ifc2x3EntityMappingInformation> maps, out ExpressMetaData metaData)
	{
		var factory = SchemaHelper.GetFactory(schema);
		var schemaMetaData = ExpressMetaData.GetMetadata(factory);
		List<TypeMapper> tpNames = schemaMetaData.Types().Select(x => new TypeMapper(x.Name, schemaMetaData)).ToList();

		// special mapping case for Ifc2x3, to include the mapped names
		// see https://github.com/buildingSMART/IDS/blob/development/Documentation/ImplementersDocumentation/ifc2x3-occurrence-type-mapping-table.md
		if (schema == "Ifc2x3")
		{
			// over-ride the schema metadata with the Ifc4 metadata for the mapped entities to ensure correct PredefinedTypes are picked up
			var ifc4MetaData = ExpressMetaData.GetMetadata(SchemaHelper.GetFactory("IFC4"));
			tpNames.AddRange(maps.Select(x => new TypeMapper(x.IdsEntity, ifc4MetaData)));
		}
		metaData = schemaMetaData;
		return tpNames;
	}
}

