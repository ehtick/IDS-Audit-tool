﻿using IdsLib.IdsSchema.Cardinality;
using IdsLib.IfcSchema;
using IdsLib.IfcSchema.TypeFilters;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacetCollection : IdsXmlNode, IIfcTypeConstraintProvider
{
	private readonly MinMaxCardinality minMaxOccurr;

	public IdsFacetCollection(System.Xml.XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        // this is only relevant for applicability, but we can attempt to read it in any case.
		minMaxOccurr = new MinMaxCardinality(reader);
	}

    private IIfcTypeConstraint? typeFilter = null;
    internal IEnumerable<IIdsFacet> ChildFacets => Children.OfType<IIdsFacet>();

    private bool typesFilterInitialized = false;

    public IIfcTypeConstraint? GetTypesFilter(SchemaInfo schema)
	{   
        if (!typesFilterInitialized) 
        {
            typeFilter = null;
            foreach (var provider in Children.OfType<IIfcTypeConstraintProvider>())
            {
                if (provider is IIdsCardinalityFacet card && !card.IsRequired)
                    continue;
                typeFilter = IfcTypeConstraint.Intersect(typeFilter, provider.GetTypesFilter(schema));
                if (IfcTypeConstraint.IsNotNullAndEmpty(typeFilter))
                    break;                    
            }
            typesFilterInitialized = true;
        }
        return typeFilter;
    }

    protected internal override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
        var ret = Audit.Status.Ok;
        if (type == "requirements")
        {
            foreach (var extendedRequirement in Children.OfType<IIdsCardinalityFacet>())
            {
                ret |= extendedRequirement.PerformCardinalityAudit(logger);
            }
        }
        else if (type == "applicability")
        {
            if (minMaxOccurr.Audit(out var _) != Audit.Status.Ok)
            {
                ret |= IdsErrorMessages.Report301InvalidCardinality(logger, this, minMaxOccurr);
            }
        }

		if (!TryGetUpperNode<IdsSpecification>(logger, this, IdsSpecification.SpecificationIdentificationArray, out var spec, out var retStatus))
			return retStatus;
		var requiredSchemaVersions = spec.IfcSchemaVersions;
        if (requiredSchemaVersions.TryGetSchemaInformation(out var schemaInfos))
        {
            foreach (var schemaInfo in schemaInfos)
            {
				if (!ChildFacets.Any(x => !x.IsValid) && IfcTypeConstraint.IsNotNullAndEmpty(GetTypesFilter(schemaInfo)))
					ret |= IdsErrorMessages.Report201IncompatibleClauses(logger, this, schemaInfo, "impossible match of constraints in set");
			}
        }
        return ret;
    }
}
