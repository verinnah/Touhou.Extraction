using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

internal static class Guard
{
	internal static void FailIfFileDoesNotExist(string? path)
	{
		if (!File.Exists(path))
		{
			Assert.Fail($"Test file \"{path}\" does not exist.");
		}
	}

	internal static string[] FailIfDirectoryEmpty(string path)
	{
		string[] paths = Directory.GetFiles(path, "*", FileUtils.RecursiveEnumerationOptions);

		if (paths.Length == 0)
		{
			Assert.Fail($"Directory \"{path}\" is empty.");
		}

		return paths;
	}
}
