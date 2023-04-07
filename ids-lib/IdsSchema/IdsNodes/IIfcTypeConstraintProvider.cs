﻿using IdsLib.IfcSchema.TypeFilters;

namespace IdsLib.IdsSchema.IdsNodes
{
    internal interface IIfcTypeConstraintProvider
    {
        /// <summary>
        /// Collection of the only valid types determined by the facet, 
        /// used for auditing of compatibility of multiple facets together 
        /// </summary>
        IIfcTypeConstraint ValidTypes { get; }
    }
}
