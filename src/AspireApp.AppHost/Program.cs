const string ApiResourceName = "aspireapp-api";
const string WebResourceName = "aspireapp-web";

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.AspireApp_Api>(ApiResourceName);

builder.AddProject<Projects.AspireApp_Web>(WebResourceName)
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
