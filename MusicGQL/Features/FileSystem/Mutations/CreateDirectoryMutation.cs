
using System.Security;
using MusicGQL.Types;

namespace MusicGQL.Features.FileSystem.Mutations;

[ExtendObjectType<Mutation>]
public class CreateDirectoryMutation
{
    public FileSystemEntry CreateDirectory(string path)
    {
        var rootPath = "/";

        if (!IsPathAllowed(path, rootPath))
        {
            throw new SecurityException("Cannot create directory at the specified path.");
        }

        var directoryInfo = Directory.CreateDirectory(path);

        return new FileSystemEntry(directoryInfo.Name, directoryInfo.FullName, true, false, true);
    }

    private static bool IsPathAllowed(string path, string rootPath)
    {
        var fullPath = System.IO.Path.GetFullPath(path);
        var fullAllowedBasePath = System.IO.Path.GetFullPath(rootPath);

        return fullPath.StartsWith(fullAllowedBasePath, StringComparison.OrdinalIgnoreCase);
    }
}
