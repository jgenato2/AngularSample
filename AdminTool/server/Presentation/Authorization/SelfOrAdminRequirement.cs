using Microsoft.AspNetCore.Authorization;

namespace Server.Presentation.Authorization;

public sealed class SelfOrAdminRequirement : IAuthorizationRequirement
{
}
