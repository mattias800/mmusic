using System.Security;

namespace MusicGQL.Features.FileSystem;

public class FileSystemSearchRoot
{
    public IEnumerable<FileSystemEntry> BrowseFileSystem(string? path)
    {
        var rootPath = "/"; // For Linux/Docker environments

        var resolvedPath = string.IsNullOrWhiteSpace(path) ? rootPath : path;

        if (!IsPathAllowed(resolvedPath, rootPath))
        {
            throw new SecurityException("Access to the path is not allowed.");
        }

        var directoryInfo = new DirectoryInfo(resolvedPath);

        var entries = new List<FileSystemEntry>();

        foreach (var directory in directoryInfo.EnumerateDirectories())
        {
            var isAccessible = true;
            var hasChildren = false;
            try
            {
                hasChildren = directory.EnumerateFileSystemInfos().Any();
            }
            catch (UnauthorizedAccessException)
            {
                isAccessible = false;
                hasChildren = false; // Cannot determine children if not accessible
            }
            catch (SecurityException)
            {
                isAccessible = false;
                hasChildren = false;
            }

            entries.Add(
                new FileSystemEntry(
                    directory.Name,
                    directory.FullName,
                    true,
                    hasChildren,
                    isAccessible
                )
            );
        }

        foreach (var file in directoryInfo.EnumerateFiles())
        {
            entries.Add(new FileSystemEntry(file.Name, file.FullName, false, false, true));
        }

        return entries.OrderBy(e => !e.IsDirectory).ThenBy(e => e.Name);
    }

    private static bool IsPathAllowed(string path, string rootPath)
    {
        var fullPath = System.IO.Path.GetFullPath(path);
        var fullAllowedBasePath = System.IO.Path.GetFullPath(rootPath);

        return fullPath.StartsWith(fullAllowedBasePath, StringComparison.OrdinalIgnoreCase);
    }
}
