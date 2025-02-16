using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace CatalogSupport.DataGenerator.Shared;

public abstract class Generator<T>(IServiceProvider services, bool isVision = false)
    where T : class
{
    protected IChatClient ChatClient { get; } =
        isVision
            ? services.GetRequiredKeyedService<IChatClient>("ollama-llava")
            : services.GetRequiredKeyedService<IChatClient>("ollama-deepseek-r1");

    protected ILogger Logger { get; } = services.GetRequiredService<ILogger<Generator<T>>>();

    protected abstract string DirectoryName { get; }

    protected abstract Guid GetId(T item);

    public static string OutputDirRoot => Path.Combine(Directory.GetCurrentDirectory(), "output");

    protected string OutputDirPath => Path.Combine(OutputDirRoot, DirectoryName);

    protected JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    protected abstract IAsyncEnumerable<T> GenerateCoreAsync();

    protected virtual string FilenameExtension => ".json";

    protected string GetItemOutputPath(string id) =>
        Path.Combine(OutputDirPath, $"{id}{FilenameExtension}");

    public async Task<IReadOnlyList<T>> GenerateAsync()
    {
        if (!Directory.Exists(OutputDirPath))
        {
            Directory.CreateDirectory(OutputDirPath);
        }

        var sw = Stopwatch.StartNew();
        await foreach (var item in GenerateCoreAsync())
        {
            sw.Stop();
            Logger.LogInformation(
                "Writing {itemType} {itemId} [generated in {elapsedSeconds}s]",
                item.GetType().Name,
                GetId(item),
                sw.Elapsed.TotalSeconds
            );
            var path = GetItemOutputPath(GetId(item).ToString());
            await WriteAsync(path, item);
            sw.Restart();
        }

        var existingFiles = Directory.GetFiles(OutputDirPath);
        return existingFiles.Select(Read).ToList();
    }

    protected virtual Task WriteAsync(string path, T item)
    {
        var itemJson = JsonSerializer.Serialize(item, SerializerOptions);
        return File.WriteAllTextAsync(path, itemJson);
    }

    protected virtual T Read(string path)
    {
        try
        {
            using var existingJson = File.OpenRead(path);
            return JsonSerializer.Deserialize<T>(existingJson, SerializerOptions)!;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reading {path}", path);
            throw;
        }
    }

    protected async Task<TResponse> GetAndParseJsonChatCompletion<TResponse>(
        string prompt,
        int? maxTokens = null,
        IList<AITool>? tools = null
    )
    {
        var options = new ChatOptions
        {
            MaxOutputTokens = maxTokens,
            Temperature = 0.9f,
            ResponseFormat = ChatResponseFormat.Json,
            Tools = tools,
        };

        var response = await RunWithRetries(() => ChatClient.CompleteAsync(prompt, options));
        var responseString = response.Message.Text ?? string.Empty;

        // Due to what seems like a server-side bug, when asking for a json_object response and with tools enabled,
        // it often replies with two or more JSON objects concatenated together (duplicates or slight variations).
        // As a workaround, just read the first complete JSON object from the response.
        var parsed = ReadAndDeserializeSingleValue<TResponse>(responseString, SerializerOptions)!;
        return parsed;
    }

    private static async Task<TResult> RunWithRetries<TResult>(Func<Task<TResult>> operation)
    {
        var delay = TimeSpan.FromSeconds(5);
        const int maxAttempts = 5;
        for (var attemptIndex = 1; ; attemptIndex++)
        {
            try
            {
                return await operation();
            }
            catch (Exception e) when (attemptIndex < maxAttempts)
            {
                Console.WriteLine(
                    $"Exception on attempt {attemptIndex}: {e.Message}. Will retry after {delay}"
                );
                await Task.Delay(delay);
                delay += TimeSpan.FromSeconds(15);
            }
        }
    }

    private static TResponse? ReadAndDeserializeSingleValue<TResponse>(
        string json,
        JsonSerializerOptions options
    )
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
        return JsonSerializer.Deserialize<TResponse>(ref reader, options);
    }

    protected IAsyncEnumerable<TV> MapParallel<TU, TV>(
        IEnumerable<TU> source,
        Func<TU, Task<TV>> map
    )
    {
        var outputs = Channel.CreateUnbounded<TV>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
        var mapTask = Parallel.ForEachAsync(
            source,
            parallelOptions,
            async (sourceItem, ct) =>
            {
                try
                {
                    var mappedItem = await map(sourceItem);
                    await outputs.Writer.WriteAsync(mappedItem, ct);
                }
                catch (Exception ex)
                {
                    outputs.Writer.TryComplete(ex);
                }
            }
        );

        mapTask.ContinueWith(_ => outputs.Writer.TryComplete());

        return outputs.Reader.ReadAllAsync();
    }
}
