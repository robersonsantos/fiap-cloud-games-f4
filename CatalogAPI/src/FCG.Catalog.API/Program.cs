using System.Text;
using FCG.Catalog.API.Middlewares;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Application.Services;
using FCG.Catalog.Infrastructure.Data;
using FCG.Catalog.API.Extensions;
using FCG.Catalog.API.HostedServices;
using FCG.Catalog.Infrastructure.Messaging;
using FCG.Catalog.Infrastructure.Repositories;
using FCG.Catalog.Infrastructure.Search;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger com suporte a JWT Bearer
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FCG Catalog API",
        Version = "v1",
        Description = "API de catálogo de jogos — FIAP Cloud Games Fase 4 (SQL Server + MongoDB + Redis + OpenSearch)"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe: Bearer {seu_token_jwt}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// JWT Bearer — apenas valida tokens (gerados pela UsersAPI)
var jwtChave = builder.Configuration["Jwt:Chave"] ?? throw new InvalidOperationException("Jwt:Chave não configurada.");
var jwtEmissor = builder.Configuration["Jwt:Emissor"] ?? "FCG-UsersAPI";
var jwtAudiencia = builder.Configuration["Jwt:Audiencia"] ?? "FCG-Microsservicos";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtEmissor,
            ValidAudience = jwtAudiencia,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtChave))
        };
    });

builder.Services.AddAuthorization();

// EF Core + SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(connectionString));

// MongoDB — persistência poliglota (avaliações de jogos) — Fase 3
// O driver 3.x exige registro explícito da representação de Guid
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var mongoSettings = builder.Configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>()
    ?? new MongoDbSettings();

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.Database));

// Redis — cache distribuído (IDistributedCache) — Fase 3
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "fcg-catalog:";
});

// OpenSearch — busca avançada (fuzzy + relevância) independente do SQL Server — Fase 4
var openSearchSettings = builder.Configuration.GetSection(OpenSearchSettings.SectionName).Get<OpenSearchSettings>()
    ?? new OpenSearchSettings();

builder.Services.AddSingleton(openSearchSettings);
builder.Services.AddSingleton(_ => OpenSearchJogoIndex.CriarCliente(openSearchSettings));
builder.Services.AddSingleton<IJogoSearchIndex, OpenSearchJogoIndex>();
builder.Services.AddHostedService<SearchIndexInitializer>();

// MassTransit + RabbitMQ (credenciais injetadas via Secret do Kubernetes / .env)
var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMQ:Username"]
    ?? throw new InvalidOperationException("RabbitMQ:Username não configurado.");
var rabbitPassword = builder.Configuration["RabbitMQ:Password"]
    ?? throw new InvalidOperationException("RabbitMQ:Password não configurado.");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentProcessedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPassword);
        });

        cfg.ReceiveEndpoint("fcg.payment.processed", e =>
        {
            e.ConfigureConsumer<PaymentProcessedConsumer>(ctx);
        });

        cfg.Message<FCG.Catalog.Application.Events.OrderPlacedEvent>(m =>
            m.SetEntityName("fcg.order.placed"));
    });
});

// DI — Repositories
builder.Services.AddScoped<IJogoRepository, JogoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IBibliotecaRepository, BibliotecaRepository>();
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoMongoRepository>();

// DI — Services
builder.Services.AddScoped<IJogoService, JogoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IBibliotecaService, BibliotecaService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<IBuscaService, BuscaService>();

// DI — Event Publisher
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

// Health Check
builder.Services.AddHealthChecks();

var app = builder.Build();

// Observabilidade — métricas HTTP no formato Prometheus (Fase 3).
// Registrado antes dos demais middlewares para capturar o status code final.
app.UseHttpMetrics();

// Middlewares
app.UseMiddleware<LogMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Catalog API v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics(); // GET /metrics — scrape do Prometheus

// Executar migrations ao iniciar
app.MigrateDatabase();

app.Run();
