using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Lyra.Data;
using Lyra.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

if (FirebaseApp.DefaultInstance == null)
{
    var credential = GoogleCredential.FromFile("/etc/secrets/firebase-key.json");
    FirebaseApp.Create(new AppOptions() { Credential = credential });
}

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HistoricoService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1",
            Title = "Lyra API - V1",
            Description = "API RESTful para o projeto Lyra relacionada ao Futuro do Trabalho",
        }
    );
});

builder.Services.AddApiVersioning(config =>
{
    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
    config.ReportApiVersions = true;
});

builder.Services.AddHealthChecks();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder
    .Services.AddOpenTelemetry()
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection"))
);

var app = builder.Build();

app.UseMiddleware<FirebaseAuthMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Lyra API V1");
});

app.MapGet("/", () => "API Lyra rodando com sucesso!");

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();
