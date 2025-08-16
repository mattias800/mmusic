namespace MusicGQL.Features.Users.Db;

public class DbUser
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? ListenBrainzUserId { get; set; }
    public string? ListenBrainzToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
