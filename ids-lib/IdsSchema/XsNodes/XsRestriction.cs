﻿using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using static IdsLib.Audit;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsRestriction : IdsXmlNode, IStringListMatcher, IStringPrefixMatcher, IFiniteStringMatcher
{
    internal string BaseAsString { get; init; }
    internal XsTypes.BaseTypes Base { get; init; }

    public string Value => string.Join(",", GetDicreteValues());

	internal static readonly string[] RestrictionIdentificationArray = { "restriction" };

	public XsRestriction(XmlReader reader, IdsXmlNode? parent) : base(reader, parent)
    {
        BaseAsString = reader.GetAttribute("base") ?? string.Empty;
        Base = XsTypes.GetBaseFrom(BaseAsString);
    }

	public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string variableName, IfcSchema.IfcSchemaVersions schemaContext)
    {
		var ret = Audit.Status.Ok;
        matches =  Enumerable.Empty<string>();
        if ( // todo: discuss with group: do we want to force the base type requirement for strings?
            !string.IsNullOrWhiteSpace(BaseAsString) &&
            BaseAsString != "xs:string"
            )
            return IdsErrorMessages.Report303RestrictionBadType(logger, this, BaseAsString);
        
        foreach (var child in Children)
        {
            // only matcher values are reported
            if (child is not IStringListMatcher ismv)
            {
                // ret |= IdsLoggerExtensions.ReportBadMatcher(logger, child, "string");
                // this would let xs:annotation pass with no issues
                continue;
            }
            ret |= ismv.DoesMatch(candidateStrings, ignoreCase, logger, out var thisM, variableName, schemaContext);
            if (thisM.Any())
                matches = matches.Union(thisM);   
        }
        return ret;
    }

    public IEnumerable<string> GetDicreteValues()
    {
        foreach (var child in Children.OfType<IFiniteStringMatcher>())
            foreach (var item in child.GetDicreteValues())
                yield return item;
    }

    public bool MatchesPrefix(string prefixString)
    {
        foreach (var child in Children.OfType<IStringPrefixMatcher>())
        {
            if (child.MatchesPrefix(prefixString)) 
                return true;
        }
        return false;
    }

    public bool TryMatch(IEnumerable<string> candidateStrings, bool ignoreCase, out IEnumerable<string> matches)
    {
        matches = Enumerable.Empty<string>();
        if ( // todo: discuss with group: do we want to force the base type requirement for strings?
            !string.IsNullOrWhiteSpace(BaseAsString) &&
            BaseAsString != "xs:string"
            )
            return false;
        foreach (var child in Children)
        {
            // only matcher values are reported
            if (child is not IStringListMatcher ismv)
            {
                continue;
            }
            if (ismv.TryMatch(candidateStrings, ignoreCase, out var thisM))
                matches = matches.Union(thisM);
        }
        return matches.Any();
    }

	// taken from: https://www.w3.org/TR/xmlschema-2/
	private Dictionary<XsTypes.BaseTypes, List<string>> validConstraintsDictionary = new Dictionary<XsTypes.BaseTypes, List<string>>()
    { 
        {XsTypes.BaseTypes.XsString, ["annotation", "pattern", "enumeration", "whiteSpace", "minLength", "maxLength", "length"] },
        {XsTypes.BaseTypes.XsDouble, ["annotation", "pattern", "enumeration", "whiteSpace", "minExclusive", "maxExclusive", "minInclusive", "maxInclusive"] },
        {XsTypes.BaseTypes.XsFloat, ["annotation", "pattern", "enumeration", "whiteSpace", "minExclusive", "maxExclusive", "minInclusive", "maxInclusive"] },
        {XsTypes.BaseTypes.XsDuration, ["annotation", "pattern", "enumeration", "whiteSpace", "minExclusive", "maxExclusive", "minInclusive", "maxInclusive"] },
        {XsTypes.BaseTypes.XsDateTime, ["annotation", "pattern", "enumeration", "whiteSpace", "minExclusive", "maxExclusive", "minInclusive", "maxInclusive"] },
        {XsTypes.BaseTypes.XsTime, ["annotation", "pattern", "enumeration", "whiteSpace", "minExclusive", "maxExclusive", "minInclusive", "maxInclusive"] },
        {XsTypes.BaseTypes.XsDate, ["annotation", "pattern", "enumeration", "whiteSpace", "minExclusive", "maxExclusive", "minInclusive", "maxInclusive"] },
        {XsTypes.BaseTypes.XsDecimal, ["annotation", "totalDigits", "fractionDigits", "pattern", "enumeration", "whiteSpace", "minExclusive", "maxExclusive", "minInclusive", "maxInclusive"] },
        {XsTypes.BaseTypes.XsBoolean, ["annotation", "pattern", "whiteSpace"] },
        {XsTypes.BaseTypes.XsInteger, ["annotation", "totalDigits", "fractionDigits", "pattern", "whiteSpace", "enumeration", "maxInclusive", "maxExclusive", "minInclusive", "minExclusive"] },
	};

    protected internal override Audit.Status PerformAudit(AuditStateInformation stateInfo, ILogger? logger)
    {
		var ret = Audit.Status.Ok;
        if (!Children.Any())
            ret |= IdsErrorMessages.Report304RestrictionEmptyContent(logger, this);
        if (Base == XsTypes.BaseTypes.Invalid || Base == XsTypes.BaseTypes.Undefined)
            ret |= IdsErrorMessages.Report303RestrictionBadType(logger, this, BaseAsString);
        else
        {
            if (validConstraintsDictionary.TryGetValue(Base, out var validConstraints))
            {
                foreach (var child in Children)
                {
                    if (!validConstraints!.Contains(child.type))
                    {
						ret |= IdsErrorMessages.Report308RestrictionInvalidFacet(logger, Base, child, validConstraints);
					}
                }
            }
            else
            {
                Debug.WriteLine($"We need to implement the list of validConstraints for {Base}");
            }
        }

		return ret;
    }
}
