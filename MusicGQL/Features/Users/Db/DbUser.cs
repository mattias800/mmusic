namespace MusicGQL.Features.Users.Db;

public class DbUser
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? ListenBrainzUserId { get; set; }
    public string? ListenBrainzToken { get; set; }
    public Roles.UserRoles Roles { get; set; } = Users.Roles.UserRoles.None;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
