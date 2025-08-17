using Aspire.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database")
    .WithImage("postgres:17")
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("clean-architecture");

IResourceBuilder<PapercutSmtpContainerResource> papercut = builder
    .AddPapercutSmtp("papercut")
    .WithHttpHealthCheck(path: "/");

builder.AddProject<Projects.Web_Api>("web-api")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(papercut)
    .WithReference(database)
    .WaitFor(papercut)
    .WaitFor(database);

await builder.Build().RunAsync();
