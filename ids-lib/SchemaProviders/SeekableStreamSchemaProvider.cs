using IdsLib.IdsSchema;
using IdsLib.IdsSchema.IdsNodes;
using IdsLib.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace IdsLib.SchemaProviders;

internal class SeekableStreamSchemaProvider : SchemaProvider, Audit.ISchemaProvider
{
    /// <summary>
    /// The version to fall back to when the source has no xsi:schemaLocation attribute (missing
    /// or empty) to detect a version from at all. Null preserves the previous strict behaviour
    /// of failing whenever detection is inconclusive; this is what the batch/file audit path
    /// still uses, since it has no single expected version to fall back to.
    /// <see cref="SingleAuditOptions"/>-based single-stream audits, which do carry a
    /// caller-declared <see cref="SingleAuditOptions.IdsVersion"/>, use it as the fallback here
    /// instead of hard-failing on a missing/empty schemaLocation: that attribute is a generic,
    /// optional XML hint and its absence does not affect the file's actual validity. A
    /// schemaLocation that IS present but does not resolve to a recognised version (e.g. a typo
    /// in the URL) is left alone and still fails - the caller made an explicit, if mistaken,
    /// claim about the version, which the fallback should not silently paper over.
    /// </summary>
    private readonly IdsVersion? fallbackVersion;

    public SeekableStreamSchemaProvider()
        : this(null)
    {
    }

    internal SeekableStreamSchemaProvider(IdsVersion? fallbackVersion)
    {
        this.fallbackVersion = fallbackVersion;
    }

    public Audit.Status GetSchemas(Stream source, ILogger? logger, out IEnumerable<XmlSchema> schemas)
    {
        if (!source.CanSeek)
        {
            schemas = Enumerable.Empty<XmlSchema>();
            return IdsToolMessages.ReportUnseekableStream(logger);

        }
        var originalPosition = source.Position;
        source.Seek(0, SeekOrigin.Begin);
        var info = IdsXmlHelpers.GetIdsInformationAsync(source).Result;
        source.Position = originalPosition;
        if (!info.IsIds)
        {
            schemas = Enumerable.Empty<XmlSchema>();
            return IdsToolMessages.ReportUnexpectedScenario(logger, !string.IsNullOrWhiteSpace(info.StatusMessage)
                    ? info.StatusMessage
                    : "The stream provided does not contain a recognised IDS."
                );

        }
        var version = info.GetVersion(logger);

        if (version == IdsVersion.Invalid)
        {
            if (string.IsNullOrWhiteSpace(info.SchemaLocation)
                && fallbackVersion is IdsVersion fallback && fallback != IdsVersion.Invalid)
            {
                logger?.LogWarning(
                    "The source has no xsi:schemaLocation to detect the IDS version from; falling back to the configured IdsVersion {fallbackVersion}.",
                    fallback);
                version = fallback;
            }
            else
            {
                schemas = Enumerable.Empty<XmlSchema>();
                return IdsToolMessages.ReportInvalidVersion(info.SchemaLocation, logger);
            }
        }
        else if (version != IdsVersion.Ids1_0)
        {
            logger?.LogWarning("Version {detectedVersion} is transitional, update to 1.0 before circulating.", version);
        }
        return GetResourceSchemasByVersion(version, logger, out schemas);
    }
}
