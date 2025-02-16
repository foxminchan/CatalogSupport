using System.IO.Compression;

namespace CatalogSupport.DataGenerator.Utils;

public sealed class ProductImageArchive(IReadOnlyList<IFormFile> files)
{
    private readonly string _path = Path.Combine(
        Directory.GetCurrentDirectory(),
        "output",
        "images.zip"
    );

    public async Task CreateArchiveAsync()
    {
        await using var stream = new FileStream(_path, FileMode.Create);
        await ZipFiles(stream, files);
    }

    private static async Task ZipFiles(Stream stream, IEnumerable<IFormFile> files)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        foreach (var file in files)
        {
            var entry = archive.CreateEntry(file.FileName);
            await using var entryStream = entry.Open();
            await file.CopyToAsync(entryStream);
        }
    }
}
