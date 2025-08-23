namespace MusicGQL.Features.External.Downloads;

public static class InterServiceUrlRewriter
{
    /// <summary>
    /// Rewrites the base (scheme/host/port) of 'url' from 'fromBase' to 'toBase' when it starts with 'fromBase'.
    /// Returns the original url if inputs are null/blank or if it doesn't start with fromBase.
    /// </summary>
    public static string RewriteBase(string url, string? fromBase, string? toBase)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(fromBase) || string.IsNullOrWhiteSpace(toBase))
                return url;

            var fromTrim = fromBase!.TrimEnd('/');
            var toTrim = toBase!.TrimEnd('/');

            if (url.StartsWith(fromTrim, StringComparison.OrdinalIgnoreCase))
            {
                return toTrim + url.Substring(fromTrim.Length);
            }

            return url;
        }
        catch
        {
            return url;
        }
    }
}
