using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.External.Downloads.QBittorrent;

public static class QbFinalizeCopier
{
    private static readonly string[] AudioExtensions = new[]
    {
        ".mp3",
        ".flac",
        ".m4a",
        ".wav",
        ".ogg",
    };

    /// <summary>
    /// Recursively scans the source root for audio files and copies them flat into targetDir.
    /// Existing files are not overwritten.
    /// Returns the number of files copied.
    /// </summary>
    public static int CopyAudioFilesRecursively(
        string sourceRoot,
        string targetDir,
        IDownloadLogger logger
    )
    {
        if (string.IsNullOrWhiteSpace(sourceRoot) || !Directory.Exists(sourceRoot))
        {
            logger.Warn($"[qB Finalize] Source root does not exist: {sourceRoot}");
            return 0;
        }
        Directory.CreateDirectory(targetDir);

        int copied = 0;
        IEnumerable<string> files;
        try
        {
            files = Directory
                .EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
                .Where(f =>
                    AudioExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant())
                );
        }
        catch (Exception ex)
        {
            logger.Error($"[qB Finalize] Failed to enumerate files: {ex.Message}");
            return 0;
        }

        foreach (var src in files)
        {
            try
            {
                var fname = System.IO.Path.GetFileName(src);
                var dst = System.IO.Path.Combine(targetDir, fname);
                if (File.Exists(dst))
                {
                    logger.Info($"[qB Finalize] Skipping existing file: {fname}");
                    continue;
                }
                logger.Info($"[qB Finalize] Copying {src} -> {dst}");
                File.Copy(src, dst, overwrite: false);
                copied++;
            }
            catch (Exception ex)
            {
                logger.Warn($"[qB Finalize] Failed to copy file '{src}': {ex.Message}");
            }
        }

        logger.Info(
            $"[qB Finalize] Copied {copied} audio file(s) from {sourceRoot} to {targetDir}"
        );
        return copied;
    }
}
