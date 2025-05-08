namespace MusicGQL.Features.External.SoulSeek.Integration;

public class SoulSeekConnectOptions
{
    public string Host { get; set; } = "vps.slsknet.org";
    public int Port { get; set; } = 2271;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
