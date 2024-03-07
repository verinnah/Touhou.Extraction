namespace Touhou.Extraction.Tests.Utils;

internal static class FileUtils
{
	internal static FileStreamOptions OpenReadFileStreamOptions { get; } = new()
	{
		Mode = FileMode.Open,
		Share = FileShare.Read,
		Access = FileAccess.Read
	};
	internal static FileStreamOptions OpenWriteFileStreamOptions { get; } = new()
	{
		Mode = FileMode.Create,
		Share = FileShare.None,
		Access = FileAccess.ReadWrite
	};
	internal static FileStreamOptions AsyncOpenReadFileStreamOptions { get; } = new()
	{
		Mode = FileMode.Open,
		Share = FileShare.Read,
		Access = FileAccess.Read,
		Options = FileOptions.Asynchronous
	};
	internal static FileStreamOptions AsyncOpenWriteFileStreamOptions { get; } = new()
	{
		Mode = FileMode.Create,
		Share = FileShare.None,
		Access = FileAccess.ReadWrite,
		Options = FileOptions.Asynchronous
	};
	internal static EnumerationOptions RecursiveEnumerationOptions { get; } = new EnumerationOptions
	{
		RecurseSubdirectories = true,
		ReturnSpecialDirectories = false
	};
}
