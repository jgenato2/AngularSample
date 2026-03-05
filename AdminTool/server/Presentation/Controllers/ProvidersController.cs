using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Presentation.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/providers")]
public class ProvidersController(ICqrsDispatcher cqrsDispatcher) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "sort")] string[]? sort, [FromQuery(Name = "query")] string? queryText, CancellationToken cancellationToken)
    {
        var query = new ListHealthInsurancePlansQuery(GetActor());
        var plans = await cqrsDispatcher.ExecuteQuery<ListHealthInsurancePlansQuery, IEnumerable<Server.Presentation.Contracts.HealthInsurancePlanResponse>>(query, cancellationToken);

        var items = plans
            .Where(plan => !string.IsNullOrWhiteSpace(plan.Provider))
            .GroupBy(plan => plan.Provider.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new ProviderListItem(
                group.First().Provider.Trim(),
                group.Count(),
                group.Max(item => item.EffectiveDate)))
            .ToList();

        var filteredItems = ApplyFiltering(items, queryText);

        var sortedItems = ApplySorting(filteredItems, sort)
            .ToList();

        return Ok(new { items = sortedItems });
    }

    [HttpGet("{provider}")]
    public async Task<IActionResult> GetByProvider(string provider, CancellationToken cancellationToken)
    {
        var normalizedProvider = provider?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedProvider))
        {
            return BadRequest(new { message = "Provider is required." });
        }

        var query = new ListHealthInsurancePlansQuery(GetActor());
        var plans = await cqrsDispatcher.ExecuteQuery<ListHealthInsurancePlansQuery, IEnumerable<Server.Presentation.Contracts.HealthInsurancePlanResponse>>(query, cancellationToken);

        var providerPlans = plans
            .Where(plan => !string.IsNullOrWhiteSpace(plan.Provider)
                && string.Equals(plan.Provider.Trim(), normalizedProvider, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (providerPlans.Count == 0)
        {
            return NotFound(new { message = "Provider not found." });
        }

        var displayProvider = providerPlans.First().Provider.Trim();
        var statusGroups = providerPlans
            .GroupBy(plan => plan.Status?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var recentNotes = providerPlans
            .Where(plan => !string.IsNullOrWhiteSpace(plan.Comments))
            .OrderByDescending(plan => plan.EffectiveDate)
            .Select(plan => plan.Comments!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToArray();

        var item = new ProviderDetailItem(
            displayProvider,
            providerPlans.Count,
            providerPlans.Min(plan => plan.EffectiveDate),
            providerPlans.Max(plan => plan.EffectiveDate),
            statusGroups.TryGetValue("Active", out var activeCount) ? activeCount : 0,
            statusGroups.TryGetValue("Pending", out var pendingCount) ? pendingCount : 0,
            statusGroups.TryGetValue("Expired", out var expiredCount) ? expiredCount : 0,
            Math.Round(providerPlans.Average(plan => plan.MonthlyPremium), 2),
            Math.Round(providerPlans.Average(plan => plan.Deductible), 2),
            Math.Round(providerPlans.Average(plan => plan.OutOfPocketMax), 2),
            providerPlans
                .Select(plan => plan.PlanType?.Trim())
                .Where(planType => !string.IsNullOrWhiteSpace(planType))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(planType => planType, StringComparer.OrdinalIgnoreCase)
                .ToArray()!,
            providerPlans
                .Select(plan => plan.MemberName?.Trim())
                .Where(memberName => !string.IsNullOrWhiteSpace(memberName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(memberName => memberName, StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToArray()!,
            recentNotes);

        return Ok(new { item });
    }

    private static IEnumerable<ProviderListItem> ApplySorting(
        IEnumerable<ProviderListItem> items,
        IEnumerable<string>? sortTokens)
    {
        var parsedSorts = ParseSorts(sortTokens).ToList();
        if (parsedSorts.Count == 0)
        {
            return items.OrderBy(item => item.provider, StringComparer.OrdinalIgnoreCase);
        }

        IOrderedEnumerable<ProviderListItem>? ordered = null;
        foreach (var sort in parsedSorts)
        {
            ordered = ApplySort(ordered ?? items, ordered is not null, sort.Field, sort.Descending);
        }

        return ordered ?? items;
    }

    private static IEnumerable<ProviderListItem> ApplyFiltering(
        IEnumerable<ProviderListItem> items,
        string? queryText)
    {
        var query = queryText?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return items;
        }

        return items.Where(item => item.provider.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<(string Field, bool Descending)> ParseSorts(IEnumerable<string>? sortTokens)
    {
        foreach (var token in sortTokens ?? Enumerable.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            var segments = token.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length == 0)
            {
                continue;
            }

            var field = segments[0].ToLowerInvariant();
            var direction = segments.Length > 1 ? segments[1].ToLowerInvariant() : "asc";

            yield return (field, direction == "desc");
        }
    }

    private static IOrderedEnumerable<ProviderListItem> ApplySort(
        IEnumerable<ProviderListItem> source,
        bool thenBy,
        string field,
        bool descending)
    {
        return field switch
        {
            "provider" => OrderBy(source, thenBy, descending, item => item.provider),
            "plancount" => OrderBy(source, thenBy, descending, item => item.planCount),
            "latesteffectivedate" => OrderBy(source, thenBy, descending, item => item.latestEffectiveDate),
            _ => thenBy ? (IOrderedEnumerable<ProviderListItem>)source : source.OrderBy(item => 0),
        };
    }

    private static IOrderedEnumerable<ProviderListItem> OrderBy<TKey>(
        IEnumerable<ProviderListItem> source,
        bool thenBy,
        bool descending,
        Func<ProviderListItem, TKey> keySelector)
    {
        if (thenBy)
        {
            var ordered = (IOrderedEnumerable<ProviderListItem>)source;
            return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
        }

        return descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
    }

    private sealed record ProviderListItem(
        string provider,
        int planCount,
        DateTime latestEffectiveDate);

    private sealed record ProviderDetailItem(
        string provider,
        int planCount,
        DateTime earliestEffectiveDate,
        DateTime latestEffectiveDate,
        int activePlans,
        int pendingPlans,
        int expiredPlans,
        decimal averageMonthlyPremium,
        decimal averageDeductible,
        decimal averageOutOfPocketMax,
        string[] planTypes,
        string[] members,
        string[] recentNotes);

    private string GetActor()
    {
        var userName = User.FindFirstValue(JwtRegisteredClaimNames.Name);
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(email))
        {
            return $"{userName} ({email})";
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            return userName;
        }

        return subject ?? "system";
    }
}
