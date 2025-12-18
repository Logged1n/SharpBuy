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

IResourceBuilder<RedisResource> redis = builder
    .AddRedis("redis")
    .WithImage("redis:7-alpine")
    .WithHostPort(6379);

builder.AddProject<Projects.Web_Api>("Web-API")
    .WaitFor(papercut)
    .WaitFor(database)
    .WaitFor(redis)
    .WithReference(papercut)
    .WithReference(database)
    .WithReference(redis);

await builder.Build().RunAsync();
