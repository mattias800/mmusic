using MusicGQL.Types;

namespace MusicGQL.Features.FileSystem.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CreateDirectoryMutation
{
    public CreateDirectoryResult CreateDirectory(CreateDirectoryInput input)
    {
        var rootPath = "/";

        if (!IsPathAllowed(input.Path, rootPath))
        {
            return new CreateDirectoryError("Cannot create directory at the specified path.");
        }

        var directoryInfo = Directory.CreateDirectory(input.Path);

        return new CreateDirectorySuccess(
            new FileSystemEntry(directoryInfo.Name, directoryInfo.FullName, true, false, true)
        );
    }

    private static bool IsPathAllowed(string path, string rootPath)
    {
        var fullPath = System.IO.Path.GetFullPath(path);
        var fullAllowedBasePath = System.IO.Path.GetFullPath(rootPath);

        return fullPath.StartsWith(fullAllowedBasePath, StringComparison.OrdinalIgnoreCase);
    }
}

public record CreateDirectoryInput(string Path);

[UnionType("CreateDirectoryResult")]
public abstract record CreateDirectoryResult;

public record CreateDirectorySuccess(FileSystemEntry Entry) : CreateDirectoryResult;

public record CreateDirectoryError(string Message) : CreateDirectoryResult;
