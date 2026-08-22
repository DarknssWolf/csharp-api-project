var builder = DistributedApplication.CreateBuilder(args);

// set the password for the db, this is just for the sample and should be updated to 
//use secrets or appsettings to stop exposing the password.
var sqlPassword = builder.AddParameter("sql-password", "P@ssw0rd!");

var sql = builder.AddSqlServer("sql", sqlPassword) // spins up the SQL Server Docker container
    .WithLifetime(ContainerLifetime.Persistent)    // keep the container (and data) across runs
    .WithEndpoint("tcp", endpoint => //binds to static port to avoid proxy, allows for easy connection to db using Managemenet studio
    {
        endpoint.Port = 1433;
        endpoint.IsProxied = false;
    })
    .WithDataVolume();                             // persist the data files in a Docker volume

var db = sql.AddDatabase("DefaultConnection", "SampleDb");

var apiProject = builder.AddProject<Projects.CSharpAPIProject_API>("api")
    .WithReference(db)                          // injects ConnectionStrings__DefaultConnection
    .WaitFor(db)                                // don't start the Api until SQL is ready
    .WithHttpEndpoint(port: 5000, name: "api-http") // expose HTTP endpoint
    .WithHttpsEndpoint(port: 5001, name: "api-https"); // expose HTTPS endpoint

builder.Build().Run();
