using Server.Application.Abstractions;
using Server.Presentation.Contracts;

namespace Server.Application.Services;

public sealed class HealthInsuranceSeedService : IHealthInsuranceSeedService
{
    private static readonly string[] FirstNames =
    [
        "Liam", "Olivia", "Noah", "Emma", "Mason", "Ava", "Ethan", "Sophia", "Lucas", "Mia",
        "Elijah", "Isabella", "James", "Charlotte", "Benjamin", "Amelia", "Henry", "Harper", "Alexander", "Evelyn",
        "Daniel", "Abigail", "Sebastian", "Ella", "Matthew", "Scarlett", "Jackson", "Grace", "Levi", "Chloe",
    ];

    private static readonly string[] LastNames =
    [
        "Reyes", "Mendoza", "Delacruz", "Bautista", "Navarro", "Valdez", "Torres", "Ramos", "Castillo", "Santiago",
        "Garcia", "Flores", "Hernandez", "Ortiz", "Domingo", "Pineda", "Aquino", "Mercado", "Velasco", "Cabrera",
    ];

    public void EnsureSeeded(ICollection<HealthInsurancePlanResponse> plans)
    {
        if (plans.Count > 3)
        {
            return;
        }

        var usedMemberNames = new HashSet<string>(
            plans.Select(plan => plan.MemberName),
            StringComparer.OrdinalIgnoreCase);

        var planTypes = new[] { "Family PPO", "Individual HMO", "Senior Advantage", "Corporate EPO", "High Deductible PPO" };
        var providers = new[]
        {
            "Blue Horizon Health", "CarePlus Medical", "WellLife Assurance", "NovaCare Network",
            "SummitCare Partners", "MetroShield Health",
        };
        var statuses = new[] { "Active", "Pending Renewal", "Underwriting", "Grace Period", "Suspended" };
        var comments = new[]
        {
            "Preventive care included with annual wellness allowance.",
            "Primary care network expanded this plan year.",
            "Member selected enhanced specialist access rider.",
            "Renewal documents pending final eligibility check.",
            "Plan includes telehealth and chronic care management.",
            "Employer-sponsored plan with tiered copay structure.",
        };

        for (var i = 4; i <= 180; i++)
        {
            var index = i - 1;
            var planType = planTypes[index % planTypes.Length];
            var effectiveDate = new DateTime(2025, 1, 1).AddDays((index * 9) % 650);
            var premiumBase = planType switch
            {
                "Family PPO" => 420m,
                "Individual HMO" => 255m,
                "Senior Advantage" => 195m,
                "Corporate EPO" => 310m,
                _ => 235m,
            };

            var deductible = planType switch
            {
                "Family PPO" => 1600m,
                "Individual HMO" => 900m,
                "Senior Advantage" => 500m,
                "Corporate EPO" => 1200m,
                _ => 2600m,
            } + ((index % 4) * 250m);

            var outOfPocket = deductible + 2800m + ((index % 5) * 450m);

            plans.Add(new HealthInsurancePlanResponse(
                $"HC-2026-{i:0000}",
                BuildUniqueMemberName(index, usedMemberNames),
                providers[index % providers.Length],
                planType,
                premiumBase + ((index % 9) * 18.75m),
                deductible,
                outOfPocket,
                statuses[index % statuses.Length],
                effectiveDate,
                effectiveDate.AddYears(1).AddDays(-1),
                comments[index % comments.Length]));
        }
    }

    private static string BuildMemberName(int index)
    {
        var first = FirstNames[index % FirstNames.Length];
        var last = LastNames[(index / FirstNames.Length) % LastNames.Length];
        return $"{first} {last}";
    }

    private static string BuildUniqueMemberName(int index, ISet<string> usedNames)
    {
        var baseName = BuildMemberName(index);
        if (usedNames.Add(baseName))
        {
            return baseName;
        }

        var suffix = 2;
        while (true)
        {
            var candidate = $"{baseName} {suffix}";
            if (usedNames.Add(candidate))
            {
                return candidate;
            }

            suffix++;
        }
    }
}
