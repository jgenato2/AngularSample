using Server.Presentation.Contracts;

namespace Server.Application.Abstractions;

public interface IHealthInsuranceSeedService
{
    void EnsureSeeded(ICollection<HealthInsurancePlanResponse> plans);
}
