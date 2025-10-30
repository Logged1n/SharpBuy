using Aspire.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("SQL-Database")
    .WithImage("postgres:17")
    .WithHostPort(5432)
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("clean-architecture");

IResourceBuilder<PapercutSmtpContainerResource> papercut = builder
    .AddPapercutSmtp("SMTP-Papercut", httpPort: 2115, smtpPort: 1337)
    .WithHttpHealthCheck(path: "/");

builder.AddProject<Projects.Web_Api>("Web-API")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(papercut)
    .WithReference(database)
    .WaitFor(papercut)
    .WaitFor(database);

await builder.Build().RunAsync();
