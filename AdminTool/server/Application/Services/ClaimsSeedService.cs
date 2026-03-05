using Server.Application.Abstractions;
using Server.Domain.Entities;

namespace Server.Application.Services;

public sealed class ClaimsSeedService(IClaimsStore claimsStore) : IClaimsSeedService
{
    private static readonly object SeedLock = new();
    private static bool Seeded;

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

    public void EnsureSeeded()
    {
        if (Seeded)
        {
            return;
        }

        lock (SeedLock)
        {
            if (Seeded)
            {
                return;
            }

            var seededClaims = new List<Claim>
            {
                new()
                {
                    ClaimId = "CLM-2026-0001",
                    PolicyId = "HC-2026-0001",
                    MemberName = "Maria Santos",
                    Provider = "Blue Horizon Health",
                    ClaimType = "Outpatient",
                    ServiceCategory = "Diagnostics",
                    DiagnosisCode = "R51",
                    SubmittedAt = new DateTime(2026, 2, 11),
                    ServiceDate = new DateTime(2026, 2, 10),
                    ClaimAmount = 325.75m,
                    Status = "Submitted",
                    Notes = "Outpatient diagnostics",
                },
                new()
                {
                    ClaimId = "CLM-2026-0002",
                    PolicyId = "HC-2026-0002",
                    MemberName = "Jared Cruz",
                    Provider = "CarePlus Medical",
                    ClaimType = "Emergency",
                    ServiceCategory = "Emergency Room",
                    DiagnosisCode = "S06.0X0A",
                    SubmittedAt = new DateTime(2026, 2, 17),
                    ServiceDate = new DateTime(2026, 2, 16),
                    ClaimAmount = 1420.00m,
                    Status = "Under Review",
                    Notes = "Emergency room claim",
                },
                new()
                {
                    ClaimId = "CLM-2026-0003",
                    PolicyId = "HC-2026-0003",
                    MemberName = "Elena Rivera",
                    Provider = "WellLife Assurance",
                    ClaimType = "Pharmacy",
                    ServiceCategory = "Prescription",
                    DiagnosisCode = "E11.9",
                    SubmittedAt = new DateTime(2026, 1, 29),
                    ServiceDate = new DateTime(2026, 1, 28),
                    ClaimAmount = 88.40m,
                    Status = "Approved",
                    Notes = "Prescription reimbursement",
                },
            };

            var usedMemberNames = new HashSet<string>(
                seededClaims.Select(claim => claim.MemberName),
                StringComparer.OrdinalIgnoreCase);

            var claimTypes = new[] { "Outpatient", "Inpatient", "Emergency", "Pharmacy" };
            var outpatientCategories = new[] { "Diagnostics", "Specialist Visit", "Physical Therapy", "Laboratory" };
            var inpatientCategories = new[] { "Surgery", "Room and Board", "Anesthesia", "Inpatient Procedures" };
            var emergencyCategories = new[] { "Emergency Room", "Trauma Care", "Urgent Diagnostics" };
            var pharmacyCategories = new[] { "Prescription", "Specialty Medication", "Maintenance Medication" };
            var statuses = new[] { "Submitted", "Submitted", "Under Review", "Approved", "Rejected", "Approved" };
            var diagnosisCodes = new[]
            {
                "I10", "E11.9", "J45.909", "M54.50", "R51", "K21.9", "N39.0", "L20.9", "G43.909", "S93.401A",
                "H10.9", "F41.9", "M25.561", "R07.9", "J06.9", "E78.5", "R10.9", "M79.1", "K52.9", "R42",
            };
            var providers = new[]
            {
                "Blue Horizon Health", "CarePlus Medical", "WellLife Assurance", "NovaCare Network",
                "St. Raphael Medical Center", "Riverside Community Hospital", "North Valley Clinic", "Harborview Health Group",
            };
            var notes = new[]
            {
                "Pre-authorization documents attached.",
                "Member requested expedited review.",
                "Clinical notes received from attending physician.",
                "Awaiting final billing statement from provider.",
                "Claim adjusted after benefits coordination.",
                "Coverage verified against active plan benefits.",
            };

            for (var i = 4; i <= 180; i++)
            {
                var index = i - 1;
                var claimType = claimTypes[index % claimTypes.Length];
                var serviceCategory = claimType switch
                {
                    "Outpatient" => outpatientCategories[index % outpatientCategories.Length],
                    "Inpatient" => inpatientCategories[index % inpatientCategories.Length],
                    "Emergency" => emergencyCategories[index % emergencyCategories.Length],
                    _ => pharmacyCategories[index % pharmacyCategories.Length],
                };

                var serviceDate = new DateTime(2025, 9, 1).AddDays((index * 5) % 420);
                var submittedAt = serviceDate.AddDays(1 + (index % 4));
                var baseAmount = claimType switch
                {
                    "Outpatient" => 180m,
                    "Inpatient" => 2800m,
                    "Emergency" => 1200m,
                    _ => 95m,
                };

                seededClaims.Add(new Claim
                {
                    ClaimId = $"CLM-2026-{i:0000}",
                    PolicyId = $"HC-2026-{i:0000}",
                    MemberName = BuildUniqueMemberName(index, usedMemberNames),
                    Provider = providers[index % providers.Length],
                    ClaimType = claimType,
                    ServiceCategory = serviceCategory,
                    DiagnosisCode = diagnosisCodes[(index * 3) % diagnosisCodes.Length],
                    SubmittedAt = submittedAt,
                    ServiceDate = serviceDate,
                    ClaimAmount = baseAmount + ((index % 11) * 37.5m),
                    Status = statuses[index % statuses.Length],
                    Notes = notes[index % notes.Length],
                });
            }

            claimsStore.SeedIfEmpty(seededClaims);
            Seeded = true;
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
