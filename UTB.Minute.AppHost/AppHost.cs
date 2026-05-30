using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.Environment.IsEnvironment("Testing")
    ? builder.AddPostgres("postgres-testing").WithContainerName("postgres-testing-Minute")
    : builder.AddPostgres("postgres").WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("database");

var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .AddRealm("UTBMinute");

var api = builder.AddProject<Projects.UTB_Minute_WebApi>("webapi")
       .WithReference(database)
       .WaitFor(database);

builder.AddProject<Projects.UTB_Minute_DbManager>("dbmanager")
       .WithReference(database)
       .WaitFor(database);

builder.AddProject<Projects.UTB_Minute_AdminClient>("adminclient")
       .WithReference(api)
       .WithReference(keycloak);

builder.AddProject<Projects.UTB_Minute_CanteenClient>("canteenclient")
       .WithReference(api)
       .WithReference(keycloak);
builder.Build().Run();