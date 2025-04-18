﻿using System.Text;
using Xbim.Common.Metadata;

namespace IdsLib.codegen;

public class IfcSchema_PartOfRelationGenerator
{
    internal static string Execute()
    {
        var relationNames = GetRelationNames();

        var schemaInfos = new Dictionary<string, StringBuilder>();
        foreach (var schema in Program.schemas)
        {
            var sb = new StringBuilder();
			var module = SchemaHelper.GetFactory(schema);
			var metaD = ExpressMetaData.GetMetadata(module);
            foreach (var daRelation in relationNames)
            {
                try
                {
                    var t = metaD.ExpressType(daRelation.ToUpperInvariant());
                    if (t is null)
                        continue;

                    var propOnPartSide = t.Properties.SingleOrDefault(x => x.Value.EnumerableType is not null).Value;
                    Type? partType = null;
					if (propOnPartSide is not null)
                    {
						partType = propOnPartSide.EnumerableType;
                    }
					else
                    {
						propOnPartSide = t.Properties.Single(x => x.Value.EnumerableType is null && x.Value.Name.StartsWith("Related")).Value;
						partType = propOnPartSide.PropertyInfo.PropertyType;
					}
					var partExpressType = metaD.ExpressType(partType.Name.ToUpperInvariant());

					var propOnOwnerSide = t.Properties.Single(x => x.Value.EnumerableType is null && x.Value.Name.StartsWith("Relating")).Value;
                    var ownerType = propOnOwnerSide.PropertyInfo.PropertyType;
                    var ownerExpressType = metaD.ExpressType(ownerType.Name.ToUpperInvariant());

					sb.AppendLine($"""                yield return new PartOfRelationInformation("{daRelation}", "{ownerExpressType.Name.ToUpperInvariant()}", "{partExpressType.Name.ToUpperInvariant()}");""");
                }
                catch 
                {
                    continue;
                }                
            }
            schemaInfos.Add(schema.ToString(), sb);
        }
        var source = stub;
        foreach (var schema in schemaInfos.Keys.OrderBy(x => x))
        {
			source = source.Replace($"<PlaceHolderRelations{schema}>\r\n", schemaInfos[schema].ToString());
        }
        
        source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(ExpressMetaData)));
        return source;
    }

    private static IEnumerable<string> GetRelationNames()
    {
        yield return "IFCRELAGGREGATES";
        yield return "IFCRELASSIGNSTOGROUP";
        yield return "IFCRELCONTAINEDINSPATIALSTRUCTURE";
        yield return "IFCRELNESTS";
        yield return "IFCRELVOIDSELEMENT";
        yield return "IFCRELFILLSELEMENT";
    }

    private const string stub = """
		// <auto-generated/>
		// This code was automatically generated with information from Xbim.Essentials <PlaceHolderVersion>.
		// Any changes made to this file will be lost.

		using System;
		using System.Collections.Generic;

		namespace IdsLib.IfcSchema;

		public partial class SchemaInfo
		{
		    /// <summary>
		    /// The names of classes across all schemas.
		    /// </summary>
		    public IEnumerable<PartOfRelationInformation> AllPartOfRelations
		    {
		        get
		        {
		            if (Version == IfcSchemaVersions.Ifc2x3)
		            {
		<PlaceHolderRelationsIfc2x3>
		            }
		            if (Version == IfcSchemaVersions.Ifc4)
		            {
		<PlaceHolderRelationsIfc4>
		            }
		            if (Version == IfcSchemaVersions.Ifc4x3)
		            {
		<PlaceHolderRelationsIfc4x3>
		            }
		        }
		    }
		}

		""";
}
