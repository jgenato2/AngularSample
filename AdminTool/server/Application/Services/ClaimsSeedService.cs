using Server.Application.Abstractions;
using Server.Domain.Entities;

namespace Server.Application.Services;

public sealed class ClaimsSeedService(IClaimsStore claimsStore) : IClaimsSeedService
{
    private static readonly object SeedLock = new();
    private static bool Seeded;

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

            var claimTypes = new[] { "Outpatient", "Inpatient", "Emergency", "Pharmacy" };
            var serviceCategories = new[] { "Diagnostics", "Surgery", "Emergency Room", "Prescription" };
            var statuses = new[] { "Submitted", "Under Review", "Approved", "Rejected" };

            for (var i = 4; i <= 180; i++)
            {
                var index = i - 1;
                seededClaims.Add(new Claim
                {
                    ClaimId = $"CLM-2026-{i:0000}",
                    PolicyId = $"HC-2026-{i:0000}",
                    MemberName = $"Member {i:000}",
                    Provider = index % 2 == 0 ? "Blue Horizon Health" : "CarePlus Medical",
                    ClaimType = claimTypes[index % claimTypes.Length],
                    ServiceCategory = serviceCategories[index % serviceCategories.Length],
                    DiagnosisCode = $"D{i:000}",
                    SubmittedAt = new DateTime(2026, 1, 1).AddDays(index),
                    ServiceDate = new DateTime(2025, 12, 20).AddDays(index),
                    ClaimAmount = 75m + (index * 11.5m),
                    Status = statuses[index % statuses.Length],
                    Notes = $"Auto-seeded claim {i:0000}",
                });
            }

            claimsStore.SeedIfEmpty(seededClaims);
            Seeded = true;
        }
    }
}
