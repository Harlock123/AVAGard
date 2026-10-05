namespace Telengard.Infrastructure.Storage;

/// <summary>
/// Write-to-temp then replace, so a crash mid-write never leaves a truncated file.
/// </summary>
public static class AtomicFile
{
    public static async Task WriteAllTextAsync(string path, string contents, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Directory.CreateDirectory(dir);
        var temp = Path.Combine(dir, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            await using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(contents.AsMemory(), ct);
                await writer.FlushAsync(ct);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temp, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
    }
}
