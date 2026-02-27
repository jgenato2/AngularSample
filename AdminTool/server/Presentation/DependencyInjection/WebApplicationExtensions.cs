using Microsoft.AspNetCore.Builder;

namespace Server.Presentation.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication UsePresentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("client");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", () => Results.Ok(new { status = "ok" }));
        app.MapControllers();

        return app;
    }
}
