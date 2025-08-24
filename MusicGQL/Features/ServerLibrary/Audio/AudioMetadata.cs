namespace MusicGQL.Features.ServerLibrary.Audio;

public record AudioMetadata(string Format, int? BitrateKbps, bool IsLossless)
{
    public string QualityLabel =>
        IsLossless ? "lossless"
        : string.Equals(Format, "mp3", StringComparison.OrdinalIgnoreCase) && BitrateKbps is int kb
            ? $"mp3 {kb}kbps"
        : Format.ToLowerInvariant();
}
