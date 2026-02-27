namespace Server.Presentation.Authorization;

public static class PresentationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string SelfOrAdmin = nameof(SelfOrAdmin);
}
