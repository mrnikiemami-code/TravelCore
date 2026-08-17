using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Services;

/// <summary>
/// Deterministic public Visa composition reads (TC-P17-T007 / P17-R7).
/// Locale-explicit. Not SEO IndexPolicy and not a Search engine.
/// </summary>
internal sealed class VisaPublicQuery : IVisaPublicQuery
{
    private readonly VisaDbContext _db;

    public VisaPublicQuery(VisaDbContext db)
    {
        _db = db;
    }

    public async Task<PublicVisaDefinition?> GetByCodeAsync(
        string code,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = VisaDefinition.NormalizeCode(code);
        var locale = VisaPublicEligibility.NormalizeLocaleCode(localeCode);

        var definition = await _db.VisaDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);

        return definition is null ? null : VisaPublicReadMapper.TryMap(definition, locale);
    }
}
