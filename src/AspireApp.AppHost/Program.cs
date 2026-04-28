const string ApiResourceName = "aspireapp-api";
const string WebResourceName = "aspireapp-web";
const string FunctionsResourceName = "aspireapp-functions";
const string StorageResourceName = "storage";
const string CosmosDbResourceName = "cosmosdb";

var builder = DistributedApplication.CreateBuilder(args);

// Azure Storage emulator with persistent lifetime
var storage = builder.AddAzureStorage(StorageResourceName)
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));

var queues = storage.AddQueues("queues");

// CosmosDB emulator with persistent lifetime
var cosmosDb = builder.AddAzureCosmosDB(CosmosDbResourceName)
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));

var cosmosDatabase = cosmosDb.AddCosmosDatabase("aspireapp");
cosmosDatabase.AddContainer("items", "/id");

// API - references cosmosdb and queues
var api = builder.AddProject<Projects.AspireApp_Api>(ApiResourceName)
    .WithReference(cosmosDb)
    .WithReference(queues)
    .WaitFor(cosmosDb);

// Functions - references cosmosdb and queues; waits for api
var functions = builder.AddAzureFunctionsProject<Projects.AspireApp_Functions>(FunctionsResourceName)
    .WithReference(queues)
    .WithReference(cosmosDb)
    .WaitFor(api);

// Web - references api and functions; external HTTP endpoints
builder.AddProject<Projects.AspireApp_Web>(WebResourceName)
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WithReference(functions)
    .WaitFor(api)
    .WaitFor(functions);

builder.Build().Run();
