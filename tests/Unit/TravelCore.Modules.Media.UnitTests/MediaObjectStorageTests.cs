using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;
using TravelCore.Modules.Media.Infrastructure.Storage;
using Xunit;

namespace TravelCore.Modules.Media.UnitTests;

public sealed class MediaObjectStorageTests
{
    [Fact]
    public async Task InMemory_Put_Read_Exists_Delete_RoundTrip()
    {
        IMediaObjectStorage storage = new InMemoryMediaObjectStorage();
        var key = MediaStorageKeyGenerator.NewObjectKey("image/png");
        await using var payload = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        await storage.PutAsync(new MediaObjectPutRequest(key, payload, "image/png", 4));
        Assert.True(await storage.ExistsAsync(key));

        await using var read = await storage.OpenReadAsync(key);
        Assert.NotNull(read);
        Assert.Equal("image/png", read!.ContentType);
        Assert.Equal(4, read.ContentLength);

        await storage.DeleteAsync(key);
        Assert.False(await storage.ExistsAsync(key));
        Assert.Null(await storage.OpenReadAsync(key));
    }

    [Fact]
    public async Task InMemory_Rejects_Duplicate_Key()
    {
        IMediaObjectStorage storage = new InMemoryMediaObjectStorage();
        var key = "2026/08/16/dup.bin";
        await using var a = new MemoryStream(new byte[] { 1 });
        await using var b = new MemoryStream(new byte[] { 2 });
        await storage.PutAsync(new MediaObjectPutRequest(key, a, "application/octet-stream"));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            storage.PutAsync(new MediaObjectPutRequest(key, b, "application/octet-stream")));
    }

    [Fact]
    public async Task LocalFilesystem_Put_Read_Stays_Under_Root()
    {
        var root = Path.Combine(Path.GetTempPath(), "travelcore-media-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var options = Microsoft.Extensions.Options.Options.Create(new MediaObjectStorageOptions
            {
                LocalRootPath = root
            });
            var env = new FakeHostEnvironment(Path.GetTempPath());
            IMediaObjectStorage storage = new LocalFileSystemMediaObjectStorage(options, env);

            var key = MediaStorageKeyGenerator.NewObjectKey("image/jpeg");
            await using var payload = new MemoryStream(new byte[] { 9, 8, 7 });
            await storage.PutAsync(new MediaObjectPutRequest(key, payload, "image/jpeg", 3));

            Assert.True(await storage.ExistsAsync(key));
            {
                await using var read = await storage.OpenReadAsync(key);
                Assert.NotNull(read);
                Assert.Equal("image/jpeg", read!.ContentType);
            }

            await storage.DeleteAsync(key);
            Assert.False(await storage.ExistsAsync(key));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void StorageKeyGenerator_Produces_Relative_Opaque_Key()
    {
        var key = MediaStorageKeyGenerator.NewObjectKey("image/webp");
        Assert.DoesNotContain("..", key, StringComparison.Ordinal);
        Assert.DoesNotContain("\\", key, StringComparison.Ordinal);
        Assert.False(key.StartsWith("/", StringComparison.Ordinal));
        Assert.EndsWith(".webp", key, StringComparison.Ordinal);
        Assert.NotNull(MediaAsset.NormalizeStorageKey(key));
    }

    private sealed class FakeHostEnvironment : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public FakeHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            ContentRootFileProvider = new Microsoft.Extensions.FileProviders.NullFileProvider();
        }

        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "TravelCore.Tests";
        public string ContentRootPath { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
    }
}
