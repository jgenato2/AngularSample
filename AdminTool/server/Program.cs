using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"] ?? "change_this_dev_secret";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AdminTool";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AdminToolClient";

builder.Services.AddCors(options =>
{
    options.AddPolicy("client", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AdminTool API",
        Version = "v1",
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT as: Bearer {token}",
    };

    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(doc =>
    {
        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", doc, null);
        return new OpenApiSecurityRequirement
        {
            [schemeRef] = new List<string>(),
        };
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("client");
app.UseAuthentication();
app.UseAuthorization();

var store = new UserStore();
store.SeedAdmin();

app.MapGet("/", () => Results.Ok(new { status = "ok" }));

var api = app.MapGroup("/api");

api.MapPost("/auth/register", (RegisterRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Name, email, and password required." });
    }

    var result = store.CreateUser(request.Name, request.Email, "user", request.Password);
    if (!result.Success)
    {
        return Results.Conflict(new { message = result.Error });
    }

    var token = TokenService.CreateToken(result.User!, jwtKey, jwtIssuer, jwtAudience);
    return Results.Created($"/api/users/{result.User!.Id}", new { token, user = result.User!.ToDto() });
});

api.MapPost("/auth/login", (LoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password required." });
    }

    var user = store.FindByEmail(request.Email);
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = TokenService.CreateToken(user, jwtKey, jwtIssuer, jwtAudience);
    return Results.Ok(new { token, user = user.ToDto() });
});

var users = api.MapGroup("/users").RequireAuthorization();

users.MapGet("/", (ClaimsPrincipal principal) =>
{
    if (!AuthHelpers.IsAdmin(principal))
    {
        return Results.Forbid();
    }

    return Results.Ok(new { items = store.List().Select(user => user.ToDto()) });
});

users.MapPost("/", (ClaimsPrincipal principal, CreateUserRequest request) =>
{
    if (!AuthHelpers.IsAdmin(principal))
    {
        return Results.Forbid();
    }

    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Role) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Name, email, role, and password required." });
    }

    var result = store.CreateUser(request.Name, request.Email, request.Role, request.Password);
    if (!result.Success)
    {
        return Results.Conflict(new { message = result.Error });
    }

    return Results.Created($"/api/users/{result.User!.Id}", new { item = result.User!.ToDto() });
});

users.MapGet("/{id}", (ClaimsPrincipal principal, string id) =>
{
    if (!AuthHelpers.IsSelfOrAdmin(principal, id))
    {
        return Results.Forbid();
    }

    var user = store.FindById(id);
    if (user is null)
    {
        return Results.NotFound(new { message = "User not found." });
    }

    return Results.Ok(new { item = user.ToDto() });
});

users.MapPut("/{id}", (ClaimsPrincipal principal, string id, UpdateUserRequest request) =>
{
    if (!AuthHelpers.IsSelfOrAdmin(principal, id))
    {
        return Results.Forbid();
    }

    var result = store.UpdateUser(id, request, AuthHelpers.IsAdmin(principal));
    if (!result.Success)
    {
        return result.Error == "User not found."
            ? Results.NotFound(new { message = result.Error })
            : Results.Conflict(new { message = result.Error });
    }

    return Results.Ok(new { item = result.User!.ToDto() });
});

users.MapDelete("/{id}", (ClaimsPrincipal principal, string id) =>
{
    if (!AuthHelpers.IsAdmin(principal))
    {
        return Results.Forbid();
    }

    var result = store.DeleteUser(id);
    if (!result.Success)
    {
        return Results.NotFound(new { message = result.Error });
    }

    return Results.Ok(new { ok = true });
});

app.Run();

record LoginRequest(string Email, string Password);
record RegisterRequest(string Name, string Email, string Password);
record CreateUserRequest(string Name, string Email, string Role, string Password);
record UpdateUserRequest(string? Name, string? Email, string? Role, string? Password);

sealed class User
{
    public string Id { get; init; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "user";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserDto ToDto() => new(Id, Name, Email, Role, CreatedAt, UpdatedAt);
}

record UserDto(string Id, string Name, string Email, string Role, DateTime CreatedAt, DateTime UpdatedAt);

sealed class UserStore
{
    private readonly List<User> _users = new();
    private int _nextId = 2;

    public void SeedAdmin()
    {
        _users.Add(new User
        {
            Id = "1",
            Name = "Admin",
            Email = "admin@local.com",
            Role = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
    }

    public IEnumerable<User> List() => _users;

    public User? FindByEmail(string email) =>
        _users.FirstOrDefault(user => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public User? FindById(string id) => _users.FirstOrDefault(user => user.Id == id);

    public StoreResult CreateUser(string name, string email, string role, string password)
    {
        if (FindByEmail(email) != null)
        {
            return StoreResult.Fail("Email already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = _nextId.ToString(),
            Name = name,
            Email = email,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _nextId++;
        _users.Add(user);
        return StoreResult.Ok(user);
    }

    public StoreResult UpdateUser(string id, UpdateUserRequest updates, bool allowRole)
    {
        var user = FindById(id);
        if (user is null)
        {
            return StoreResult.Fail("User not found.");
        }

        if (!string.IsNullOrWhiteSpace(updates.Email) &&
            !updates.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase) &&
            FindByEmail(updates.Email) != null)
        {
            return StoreResult.Fail("Email already exists.");
        }

        if (!string.IsNullOrWhiteSpace(updates.Name))
        {
            user.Name = updates.Name;
        }

        if (!string.IsNullOrWhiteSpace(updates.Email))
        {
            user.Email = updates.Email;
        }

        if (allowRole && !string.IsNullOrWhiteSpace(updates.Role))
        {
            user.Role = updates.Role;
        }

        if (!string.IsNullOrWhiteSpace(updates.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updates.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;
        return StoreResult.Ok(user);
    }

    public StoreResult DeleteUser(string id)
    {
        var user = FindById(id);
        if (user is null)
        {
            return StoreResult.Fail("User not found.");
        }

        _users.Remove(user);
        return StoreResult.Ok(user);
    }
}

record StoreResult(bool Success, User? User, string? Error)
{
    public static StoreResult Ok(User user) => new(true, user, null);
    public static StoreResult Fail(string error) => new(false, null, error);
}

static class AuthHelpers
{
    public static bool IsAdmin(ClaimsPrincipal principal) => principal.IsInRole("admin");

    public static bool IsSelfOrAdmin(ClaimsPrincipal principal, string userId)
    {
        if (IsAdmin(principal))
        {
            return true;
        }

        var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return subject == userId;
    }
}

static class TokenService
{
    public static string CreateToken(User user, string key, string issuer, string audience)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
