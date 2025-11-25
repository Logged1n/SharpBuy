using Aspire.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("SQL-Database")
    .WithImage("postgres:17")
    .WithHostPort(5002)
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("SharpBuy");

IResourceBuilder<PapercutSmtpContainerResource> papercut = builder
    .AddPapercutSmtp("papercut");

IResourceBuilder<ProjectResource> webApi = builder.AddProject<Projects.Web_Api>("Web-API")
    .WaitFor(papercut)
    .WaitFor(database)
    .WithReference(papercut)
    .WithReference(database);

builder.AddNpmApp("SharpBuy-Client", "../../client")
    .WithReference(webApi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

await builder.Build().RunAsync();
