using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Presentation.Authorization;
using Server.Presentation.Contracts;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/health-insurance")]
public class HealthInsuranceController : ControllerBase
{
    private static readonly object PlansLock = new();
    private static readonly List<HealthInsurancePlanResponse> Plans =
    [
        new(
            "HC-2026-0001",
            "Maria Santos",
            "Blue Horizon Health",
            "Family PPO",
            420.50m,
            1500m,
            6500m,
            "Active",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31)),
        new(
            "HC-2026-0002",
            "Jared Cruz",
            "CarePlus Medical",
            "Individual HMO",
            275.00m,
            1000m,
            4500m,
            "Active",
            new DateTime(2026, 2, 1),
            new DateTime(2027, 1, 31)),
        new(
            "HC-2026-0003",
            "Elena Rivera",
            "WellLife Assurance",
            "Senior Advantage",
            198.75m,
            500m,
            3200m,
            "Pending Renewal",
            new DateTime(2025, 4, 15),
            new DateTime(2026, 4, 14)),
    ];

    [HttpGet("plans")]
    public IActionResult ListPlans()
    {
        lock (PlansLock)
        {
            var items = Plans.OrderBy(plan => plan.PolicyId).ToList();
            return Ok(new { items });
        }
    }

    [HttpGet("plans/{policyId}")]
    public IActionResult GetByPolicyId(string policyId)
    {
        lock (PlansLock)
        {
            var item = Plans.FirstOrDefault(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                return NotFound(new { message = "Insurance plan not found." });
            }

            return Ok(new { item });
        }
    }

    [HttpPost("plans")]
    [AdminOnly]
    public IActionResult Create([FromBody] CreateHealthInsurancePlanRequest request)
    {
        lock (PlansLock)
        {
            var duplicate = Plans.Any(plan => plan.PolicyId.Equals(request.policyId, StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                return Conflict(new { message = "Policy ID already exists." });
            }

            var item = new HealthInsurancePlanResponse(
                request.policyId,
                request.memberName,
                request.provider,
                request.planType,
                request.monthlyPremium,
                request.deductible,
                request.outOfPocketMax,
                request.status,
                request.effectiveDate,
                request.renewalDate);

            Plans.Add(item);
            return Created($"/api/health-insurance/plans/{item.PolicyId}", new { item });
        }
    }

    [HttpPut("plans/{policyId}")]
    [AdminOnly]
    public IActionResult Update(string policyId, [FromBody] UpdateHealthInsurancePlanRequest request)
    {
        lock (PlansLock)
        {
            var index = Plans.FindIndex(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return NotFound(new { message = "Insurance plan not found." });
            }

            var current = Plans[index];
            var updated = current with
            {
                MemberName = request.memberName ?? current.MemberName,
                Provider = request.provider ?? current.Provider,
                PlanType = request.planType ?? current.PlanType,
                MonthlyPremium = request.monthlyPremium ?? current.MonthlyPremium,
                Deductible = request.deductible ?? current.Deductible,
                OutOfPocketMax = request.outOfPocketMax ?? current.OutOfPocketMax,
                Status = request.status ?? current.Status,
                EffectiveDate = request.effectiveDate ?? current.EffectiveDate,
                RenewalDate = request.renewalDate ?? current.RenewalDate,
            };

            Plans[index] = updated;
            return Ok(new { item = updated });
        }
    }

    [HttpDelete("plans/{policyId}")]
    [AdminOnly]
    public IActionResult Delete(string policyId)
    {
        lock (PlansLock)
        {
            var index = Plans.FindIndex(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return NotFound(new { message = "Insurance plan not found." });
            }

            Plans.RemoveAt(index);
            return Ok(new { ok = true });
        }
    }

}
