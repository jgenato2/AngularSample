using Microsoft.AspNetCore.Builder;

namespace Server.Presentation.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication UsePresentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("client");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }
}
