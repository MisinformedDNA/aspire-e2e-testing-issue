using Microsoft.Playwright;

namespace AspireApp.E2E.Tests.Infrastructure;

public static class PlaywrightStorageStateHelper
{
    private static readonly string StorageDirectory = Path.Combine(
        Path.GetTempPath(), "aspireapp-e2e-storage");

    public static string GetStorageStatePath(string identifier)
    {
        Directory.CreateDirectory(StorageDirectory);
        return Path.Combine(StorageDirectory, $"{identifier}.json");
    }

    public static async Task SaveStorageStateAsync(IBrowserContext context, string identifier)
    {
        var path = GetStorageStatePath(identifier);
        await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = path });
    }

    public static bool StorageStateExists(string identifier)
    {
        return File.Exists(GetStorageStatePath(identifier));
    }

    public static void ClearStorageState(string identifier)
    {
        var path = GetStorageStatePath(identifier);
        if (File.Exists(path))
            File.Delete(path);
    }
}
