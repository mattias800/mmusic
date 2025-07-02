namespace MusicGQL.Features.FileSystem;

public record FileSystemEntry(string Name, string Path, bool IsDirectory, bool HasChildren, bool IsAccessible);
