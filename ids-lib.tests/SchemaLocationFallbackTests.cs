using idsLib.tests.Helpers;
using IdsLib;
using IdsLib.IdsSchema.IdsNodes;
using idsTool.tests.Helpers;
using System.IO;
using System.Text;
using Xunit;

namespace idsLib.tests;

/// <summary>
/// xsi:schemaLocation is a generic, optional XML hint attribute (not IDS-specific) and its
/// absence does not affect an IDS file's actual structural or semantic validity. But
/// <see cref="SeekableStreamSchemaProvider"/> determines the schema version to audit against
/// solely from that attribute, and previously failed the whole audit with "Unrecognised
/// version from location value" whenever it was missing or empty - even though callers of the
/// single-stream <see cref="Audit.Run(Stream, SingleAuditOptions, Microsoft.Extensions.Logging.ILogger?)"/>
/// entry point already declare the version they expect via <see cref="SingleAuditOptions.IdsVersion"/>.
/// These tests cover the fallback to that declared version.
/// </summary>
public class SchemaLocationFallbackTests : BuildingSmartRepoFiles
{
	public SchemaLocationFallbackTests(ITestOutputHelper outputHelper)
	{
		XunitOutputHelper = outputHelper;
	}

	private ITestOutputHelper XunitOutputHelper { get; }

	private const string ValidBody = """
		<info>
			<title>Minimal valid IDS</title>
		</info>
		<specifications>
			<specification name="Walls" ifcVersion="IFC4">
				<applicability maxOccurs="unbounded">
					<entity><name><simpleValue>IFCWALL</simpleValue></name></entity>
				</applicability>
				<requirements>
					<entity><name><simpleValue>IFCWALL</simpleValue></name></entity>
				</requirements>
			</specification>
		</specifications>
		""";

	private Stream PrepareStream(string schemaLocationAttribute)
	{
		var xml =
			$"""
			<ids xmlns="http://standards.buildingsmart.org/IDS" 
				xmlns:xs="http://www.w3.org/2001/XMLSchema" 
				xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
				{schemaLocationAttribute}
				>
			{ValidBody}
			</ids>
			""";
		XunitOutputHelper.WriteLine(xml);
		return new MemoryStream(Encoding.UTF8.GetBytes(xml));
	}

	[Fact]
	public void FallsBackToDeclaredIdsVersionWhenSchemaLocationMissing()
	{
		using var stream = PrepareStream(schemaLocationAttribute: string.Empty);
		var options = new SingleAuditOptions()
		{
			IdsVersion = IdsVersion.Ids1_0,
			OmitIdsContentAudit = true,
		};
		LoggerAndAuditHelpers.AuditWithStream(stream, options, XunitOutputHelper, Audit.Status.Ok, expectedWarnAndErrors: 1);
	}

	[Fact]
	public void FallsBackToDeclaredIdsVersionWhenSchemaLocationEmpty()
	{
		using var stream = PrepareStream("xsi:schemaLocation=\"\"");
		var options = new SingleAuditOptions()
		{
			IdsVersion = IdsVersion.Ids1_0,
			OmitIdsContentAudit = true,
		};
		LoggerAndAuditHelpers.AuditWithStream(stream, options, XunitOutputHelper, Audit.Status.Ok, expectedWarnAndErrors: 1);
	}

	[Fact]
	public void DoesNotOverrideAnExistingCorrectSchemaLocation()
	{
		using var stream = PrepareStream("xsi:schemaLocation=\"http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/1.0/ids.xsd\"");
		var options = new SingleAuditOptions()
		{
			IdsVersion = IdsVersion.Ids1_0,
			OmitIdsContentAudit = true,
		};
		LoggerAndAuditHelpers.AuditWithStream(stream, options, XunitOutputHelper, Audit.Status.Ok);
	}

	[Fact]
	public void StillFailsWhenAnExistingSchemaLocationIsUnrecognised()
	{
		using var stream = PrepareStream("xsi:schemaLocation=\"http://standards.buildingsmart.org/IDS http://example.com/wrong.xsd\"");
		var options = new SingleAuditOptions()
		{
			IdsVersion = IdsVersion.Ids1_0,
			OmitIdsContentAudit = true,
		};
		LoggerAndAuditHelpers.AuditWithStream(stream, options, XunitOutputHelper, Audit.Status.IdsStructureError);
	}
}
