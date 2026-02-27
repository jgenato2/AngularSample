namespace Server.Presentation.Contracts;

public record UserResponse(string Id, string Name, string Email, string Role, DateTime CreatedAt, DateTime UpdatedAt);
