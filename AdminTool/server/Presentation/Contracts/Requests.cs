namespace Server.Presentation.Contracts;

public record LoginRequest(string email, string password);
public record RegisterRequest(string name, string email, string password);
public record CreateUserRequest(string name, string email, string role, string password);
public record UpdateUserRequest(string? name, string? email, string? role, string? password);
