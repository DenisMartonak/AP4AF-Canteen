using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.Environment.IsEnvironment("Testing")
    ? builder.AddPostgres("postgres-testing").WithContainerName("postgres-testing-Minute")
    : builder.AddPostgres("postgres").WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("database");

var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .WithRealmImport("import")
                      .WithHttpsEndpoint(port: 8443, name: "https")
                      .WithDataVolume("utb-minute-keycloak-data")
                      .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
                      .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
                      .WithLifetime(ContainerLifetime.Persistent);

var api = builder.AddProject<Projects.UTB_Minute_WebApi>("webapi")
       .WithReference(database)
       .WithReference(keycloak)
       .WaitFor(database)
       .WaitFor(keycloak);

builder.AddProject<Projects.UTB_Minute_DbManager>("dbmanager")
       .WithReference(database)
       .WithHttpCommand("/dev/seed", "Seed Database")
       .WaitFor(database);

builder.AddProject<Projects.UTB_Minute_AdminClient>("adminclient")
       .WithReference(api)
       .WithReference(keycloak)
       .WaitFor(api)
       .WaitFor(keycloak);

builder.AddProject<Projects.UTB_Minute_CanteenClient>("canteenclient")
       .WithReference(api)
       .WithReference(keycloak)
       .WaitFor(api)
       .WaitFor(keycloak);
builder.Build().Run();
