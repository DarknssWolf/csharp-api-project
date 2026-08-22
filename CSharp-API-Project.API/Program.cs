using CSharpAPIProject.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiApplicationServices(); // Attach Application to API Layer
builder.Services.AddInfrastructureServices(builder.Configuration); // Attach Infrastructure to API Layer
builder.Services.AddApiServices(builder.Configuration); // Attach API Services (from Api project)

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

// Skipped during NSwag client generation, which runs the app without a database.
if (Environment.GetEnvironmentVariable("SKIP_DB_INIT") != "true")
	await app.InitialiseDatabaseAsync();

app.UseCors("ReactClient");

app.UseHealthChecks("/health");
app.UseHttpsRedirection();

app.UseOpenApi(settings =>
{
	settings.Path = "/swagger/specification.json";
});

app.UseSwaggerUi(settings =>
{
	settings.Path = "/swagger";
	settings.DocumentPath = "/swagger/specification.json";
});

app.MapScalarApiReference(options =>
{
	options.WithOpenApiRoutePattern("/swagger/specification.json")
	.WithTitle("CSharpAPIProject API - Scalar UI")
	.WithDarkMode(false);
})
.WithName("Scalar");

// Redirect to Scalar UI by default
app.MapGet("/", context =>
{
	context.Response.Redirect("/scalar/v1");
	return Task.CompletedTask;
}).WithName("RedirectToScalar");

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(options => { });
app.MapEndpoints();

app.Run();
