using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgresUser = builder.AddParameter("SqlUser", true);
var postgresPassword = builder.AddParameter("SqlPassword", true);

var postgres = builder
    .AddPostgres("postgres", postgresUser, postgresPassword, 5432)
    .WithPgAdmin(options =>
    {
        options.WithImageTag("9.0");
    })
    .WithDataBindMount("../../mnt/postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalog-db");

var vectorDb = builder
    .AddQdrant("vector-db")
    .WithDataBindMount("../../mnt/qdrant")
    .WithLifetime(ContainerLifetime.Persistent);

var ollama = builder
    .AddOllama("ollama")
    .WithDataVolume()
    .WithGPUSupport()
    .WithOpenWebUI(options =>
    {
        options.WithImageTag("0.5.1");
    });

var deepseek = ollama.AddModel("deepseek-r1:8b");

var llava = ollama.AddModel("llava:7b");

var storage = builder.AddAzureStorage("storage");

if (builder.Environment.IsDevelopment())
{
    storage.RunAsEmulator(config =>
        config.WithDataBindMount("../../mnt/azurite").WithLifetime(ContainerLifetime.Persistent)
    );
}

var blobs = storage.AddBlobs("blobs");

var apiService = builder
    .AddProject<Projects.CatalogSupport_ApiService>("api-service")
    .WithReference(deepseek)
    .WithReference(catalogDb)
    .WithReference(vectorDb)
    .WithReference(blobs)
    .WaitFor(deepseek)
    .WaitFor(catalogDb)
    .WaitFor(vectorDb)
    .WaitFor(blobs);

builder
    .AddProject<Projects.CatalogSupport_DataGenerator>("data-generator")
    .WithReference(llava)
    .WithReference(deepseek)
    .WaitFor(llava)
    .WaitFor(deepseek);

builder
    .AddProject<Projects.CatalogSupport_Website>("website")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
