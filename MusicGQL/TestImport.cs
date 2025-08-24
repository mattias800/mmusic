using MusicGQL.Features.Import;

namespace MusicGQL;

/// <summary>
/// Test script to demonstrate the import functionality
/// Usage: Call this from Program.cs during development
/// </summary>
public static class TestImport
{
    /// <summary>
    /// Test importing an artist (Ghost) with their releases
    /// </summary>
    public static async Task TestImportGhostAsync(LibraryImportService importService)
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine("🧪 TESTING MUSIC IMPORT SYSTEM");
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine();

        // Test 1: Import Ghost (the band)
        Console.WriteLine("🎸 Testing Artist Import: Ghost");
        Console.WriteLine("─────────────────────────────────────────────");

        var artistResult = await importService.ImportArtistAsync("Ghost");

        if (artistResult.Success)
        {
            Console.WriteLine($"✅ Artist Import Successful!");
            Console.WriteLine($"   📁 Artist ID: {artistResult.ArtistJson?.Id}");
            Console.WriteLine($"   🆔 MusicBrainz ID: {artistResult.MusicBrainzId}");
            Console.WriteLine($"   🎵 Spotify ID: {artistResult.SpotifyId ?? "Not found"}");
            Console.WriteLine($"   📸 Photos Downloaded:");
            Console.WriteLine(
                $"      🖼️  Thumbs: {artistResult.DownloadedPhotos?.Thumbs.Count ?? 0}"
            );
            Console.WriteLine(
                $"      🌄 Backgrounds: {artistResult.DownloadedPhotos?.Backgrounds.Count ?? 0}"
            );
            Console.WriteLine(
                $"      🏷️  Banners: {artistResult.DownloadedPhotos?.Banners.Count ?? 0}"
            );
            Console.WriteLine($"      🎨 Logos: {artistResult.DownloadedPhotos?.Logos.Count ?? 0}");

            // Test 2: Import Ghost's releases
            Console.WriteLine();
            Console.WriteLine("💿 Testing Release Import for Ghost");
            Console.WriteLine("─────────────────────────────────────────────");

            var releasesResult = await importService.ImportArtistReleasesAsync(
                artistResult.ArtistJson!.Id
            );

            if (releasesResult.Success)
            {
                Console.WriteLine($"✅ Release Import Successful!");
                Console.WriteLine($"   📀 Total Releases: {releasesResult.TotalReleases}");
                Console.WriteLine($"   ✅ Successful: {releasesResult.SuccessfulReleases}");
                Console.WriteLine($"   ❌ Failed: {releasesResult.FailedReleases}");
                Console.WriteLine();
                Console.WriteLine("📋 Imported Releases:");

                foreach (var release in releasesResult.ImportedReleases)
                {
                    var status = release.Success ? "✅" : "❌";
                    Console.WriteLine($"   {status} {release.Title}");
                    if (!release.Success && !string.IsNullOrEmpty(release.ErrorMessage))
                    {
                        Console.WriteLine($"      💭 Error: {release.ErrorMessage}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ Release Import Failed: {releasesResult.ErrorMessage}");
            }
        }
        else
        {
            Console.WriteLine($"❌ Artist Import Failed: {artistResult.ErrorMessage}");
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine("🧪 IMPORT TEST COMPLETE");
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine();
    }

    /// <summary>
    /// Test importing multiple artists to demonstrate the system
    /// </summary>
    public static async Task TestImportMultipleArtistsAsync(LibraryImportService importService)
    {
        string[] testArtists = ["Ghost", "Metallica", "Iron Maiden"];

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine("🧪 TESTING MULTIPLE ARTIST IMPORTS");
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine();

        foreach (var artistName in testArtists)
        {
            Console.WriteLine($"🎤 Importing: {artistName}");
            Console.WriteLine("─────────────────────────────────────────────");

            try
            {
                var result = await importService.ImportArtistAsync(artistName);

                if (result.Success)
                {
                    Console.WriteLine($"✅ {artistName}: SUCCESS");
                    Console.WriteLine($"   🆔 MusicBrainz: {result.MusicBrainzId}");
                    Console.WriteLine($"   🎵 Spotify: {result.SpotifyId ?? "Not found"}");

                    var totalPhotos =
                        (result.DownloadedPhotos?.Thumbs.Count ?? 0)
                        + (result.DownloadedPhotos?.Backgrounds.Count ?? 0)
                        + (result.DownloadedPhotos?.Banners.Count ?? 0)
                        + (result.DownloadedPhotos?.Logos.Count ?? 0);
                    Console.WriteLine($"   📸 Photos: {totalPhotos}");
                }
                else
                {
                    Console.WriteLine($"❌ {artistName}: FAILED - {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 {artistName}: EXCEPTION - {ex.Message}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine("🧪 MULTIPLE IMPORT TEST COMPLETE");
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine();
    }
}

/// <summary>
/// GraphQL test queries you can use to test the import system
/// </summary>
public static class ImportTestQueries
{
    /// <summary>
    /// GraphQL mutation to import an artist
    /// </summary>
    public const string ImportArtistMutation =
        @"
        mutation ImportArtist($artistName: String!) {
            importArtist(artistName: $artistName) {
                success
                artistName
                artistId
                musicBrainzId
                spotifyId
                photosDownloaded {
                    thumbs
                    backgrounds
                    banners
                    logos
                }
                errorMessage
            }
        }
    ";

    /// <summary>
    /// GraphQL mutation to import releases for an artist
    /// </summary>
    public const string ImportReleasesMutation =
        @"
        mutation ImportReleases($artistId: String!) {
            importArtistReleases(artistId: $artistId) {
                success
                artistId
                totalReleases
                successfulReleases
                failedReleases
                importedReleases {
                    success
                    title
                    releaseGroupId
                    errorMessage
                }
                errorMessage
            }
        }
    ";

    /// <summary>
    /// GraphQL query to see what was imported
    /// </summary>
    public const string QueryImportedData =
        @"
        query GetImportedData {
            serverLibrary {
                allArtists {
                    id
                    name
                    images {
                        thumbs
                        backgrounds
                        banners
                        logos
                    }
                    releases {
                        title
                        type
                        coverArtUrl
                        tracks {
                            title
                            trackNumber
                            length
                        }
                    }
                }
            }
        }
    ";
}
