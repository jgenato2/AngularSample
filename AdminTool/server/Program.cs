using Server.Application.DependencyInjection;
using Server.Infrastructure.DependencyInjection;
using Server.Presentation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

app.InitializeData();
app.UsePresentation();

app.Run();
