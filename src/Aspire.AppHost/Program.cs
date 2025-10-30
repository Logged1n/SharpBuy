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

builder.AddProject<Projects.Web_Api> ("Web-API")
    .WithReference(papercut)
    .WithReference(database)
    .WaitFor(papercut)
    .WaitFor(database);

await builder.Build().RunAsync();
