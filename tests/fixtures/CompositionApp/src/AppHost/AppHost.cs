// SDK-style Aspire AppHost (Sdk="Aspire.AppHost.Sdk/13.3.5"), the shamshir shape. The aspire signal
// must fire from the project SDK, and the AppHost orchestrating only 2 project references must NOT
// tip the whole app into a "Microservices" verdict.
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject("web", "../Web/Web.csproj");

builder.Build().Run();
