using CSharpAPIProject.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();
await app.InitialiseDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseCors("ReactClient");
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseExceptionHandler(options => { });
app.MapEndpoints();

app.Run();
