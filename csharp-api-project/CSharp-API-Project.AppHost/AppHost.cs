var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")           // spins up the SQL Server Docker container
    .WithLifetime(ContainerLifetime.Persistent) // keep the container (and data) across runs
    .WithDataVolume();                          // persist the data files in a Docker volume

var db = sql.AddDatabase("DefaultConnection", "SampleDb");

builder.AddProject<Projects.CSharpAPIProject_API>("api")
    .WithReference(db)                          // injects ConnectionStrings__DefaultConnection
    .WaitFor(db);                               // don't start the Api until SQL is ready

builder.Build().Run();
