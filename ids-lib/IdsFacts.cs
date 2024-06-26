﻿using System;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes;

/// <summary>
/// Collection of relevant IDS assumptions
/// </summary>
public class IdsFacts
{
#pragma warning disable IDE0060 // Remove unused parameter
	internal static IdsVersion GetVersionFromLocation(string location, ILogger? logger = null)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		return location switch
		{
			// the following are the only canonical versions accepted
			// todo: we keep 0.9.7 transitionally
			"http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/0.9.7/ids.xsd" => IdsVersion.Ids0_9_7,
			"http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/1.0/ids.xsd" => IdsVersion.Ids1_0,
			_ => IdsVersion.Invalid,
		};
		;
    }

	/// <summary>
	/// The version of IDS schema to be assumed when running programmatically
	/// </summary>
	public const IdsVersion DefaultIdsVersion = IdsVersion.Ids1_0;
}


/// <summary>
/// Enumeration to identify a single IDS version.
/// </summary>
public enum IdsVersion
{
    /// <summary>
    /// Invalid or incomplete version information
    /// </summary>
    Invalid,
	/// <summary>
	/// Version 0.9; this might be removed before first official release
	/// </summary>
	[Obsolete("Prefer specific version")]
    Ids0_9,
	/// <summary>
	/// Version 0.9.6; this might be removed before first official release
	/// </summary>
	Ids0_9_6,
	/// <summary>
	/// Version 0.9.7; this might be removed before first official release
	/// </summary>
	Ids0_9_7,
	/// <summary>
	/// Version 1; this is not final until formal release.
	/// </summary>
	Ids1_0
}