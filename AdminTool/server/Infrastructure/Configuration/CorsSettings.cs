namespace Server.Infrastructure.Configuration;

public sealed class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
}
