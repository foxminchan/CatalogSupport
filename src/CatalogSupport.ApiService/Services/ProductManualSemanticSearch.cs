using System.IO.Compression;

namespace CatalogSupport.ApiService.Services;

public sealed class ProductManualSemanticSearch
{
    public static async Task EnsureSeedDataImportedAsync(
        IServiceProvider services,
        string? initialImportDataDir
    )
    {
        if (!string.IsNullOrEmpty(initialImportDataDir))
        {
            using var scope = services.CreateScope();
            await ImportManualFilesSeedDataAsync(scope, initialImportDataDir);
        }
    }

    private static async Task ImportManualFilesSeedDataAsync(
        IServiceScope scope,
        string initialImportDataDir
    )
    {
        var blobStorage = scope.ServiceProvider.GetRequiredService<IBlobService>();

        var imagesZipPath = Path.Combine(initialImportDataDir, "images.zip");

        using var zipFile = ZipFile.OpenRead(imagesZipPath);

        foreach (var file in zipFile.Entries)
        {
            await using var fileStream = file.Open();
            var formFile = new FormFile(fileStream, 0, file.Length, file.Name, file.Name);
            await blobStorage.UploadFileAsync(formFile);
        }
    }
}
