namespace MusicGQL.Integration.Youtube.Configuration
{
    public class YouTubeServiceOptions
    {
        public const string SectionName = "YouTube";

        public string? ApiKey { get; set; }
        public string? ApplicationName { get; set; }
    }
}
