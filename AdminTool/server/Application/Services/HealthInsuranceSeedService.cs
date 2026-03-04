using Server.Application.Abstractions;
using Server.Presentation.Contracts;

namespace Server.Application.Services;

public sealed class HealthInsuranceSeedService : IHealthInsuranceSeedService
{
    public void EnsureSeeded(ICollection<HealthInsurancePlanResponse> plans)
    {
        if (plans.Count > 3)
        {
            return;
        }

        var planTypes = new[] { "Family PPO", "Individual HMO", "Senior Advantage", "Corporate EPO" };
        var providers = new[] { "Blue Horizon Health", "CarePlus Medical", "WellLife Assurance", "NovaCare Network" };
        var statuses = new[] { "Active", "Pending Renewal", "Underwriting", "Grace Period" };

        for (var i = 4; i <= 180; i++)
        {
            var index = i - 1;
            var effectiveDate = new DateTime(2026, 1, 1).AddDays(index * 2);
            plans.Add(new HealthInsurancePlanResponse(
                $"HC-2026-{i:0000}",
                $"Member {i:000}",
                providers[index % providers.Length],
                planTypes[index % planTypes.Length],
                180m + (index * 4.5m),
                500m + ((index % 8) * 250m),
                3000m + ((index % 10) * 400m),
                statuses[index % statuses.Length],
                effectiveDate,
                effectiveDate.AddYears(1).AddDays(-1),
                $"Auto-seeded policy {i:0000}."));
        }
    }
}
