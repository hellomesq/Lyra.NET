using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Lyra.Data;
using Lyra.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Serviços da aplicação
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers();

// 🔹 Swagger
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

// 🔹 Versionamento de API
builder.Services.AddApiVersioning(config =>
{
    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
    config.ReportApiVersions = true;
});

// 🔹 Health checks
builder.Services.AddHealthChecks();

// 🔹 Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 🔹 OpenTelemetry (opcional)
builder
    .Services.AddOpenTelemetry()
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
    });

// 🔹 Banco de dados Oracle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection"))
);

// 🔹 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowDev",
        policy =>
        {
            policy
                .AllowAnyOrigin() // permite qualquer origem para dev
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );

    options.AddPolicy(
        "AllowProd",
        policy =>
        {
            policy
                .WithOrigins("https://seu-dominio.com") // domínio de produção
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

var app = builder.Build();

// CORS
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

// 🔹 Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Lyra API V1");
});

// 🔹 Endpoints
app.MapGet("/", () => "API Lyra rodando com sucesso!");
app.MapHealthChecks("/health");

// 🔹 HTTPS em dev
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 🔹 Controllers
app.MapControllers();

app.Run();
