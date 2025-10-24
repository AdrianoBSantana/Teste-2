using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Compartilhado.Seguranca;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicoEstoque.Dados;
using MassTransit;
using ServicoEstoque.Consumidores;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços básicos da aplicação
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Informe o token JWT no formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Configuração da autenticação JWT (usando configurações compartilhadas)
builder.Services.Configure<ConfiguracoesJwt>(builder.Configuration.GetSection("Jwt"));
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<ConfiguracoesJwt>() ?? new ConfiguracoesJwt();
var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Chave ?? "chave-dev"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Emissor,
            ValidAudience = jwtConfig.Audiencia,
            IssuerSigningKey = chaveSimetrica,
            ClockSkew = TimeSpan.FromSeconds(5) // Tolerância para expiração do token
        };
    });

// Configuração do DbContext com SQLite para persistência de dados de estoque
builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EstoqueDb"))
);

builder.Services.AddAuthorization();

// Configuração do MassTransit com RabbitMQ para consumo de eventos assíncronos
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BaixarEstoqueQuandoVendaConfirmadaConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration.GetValue<string>("RabbitMq:Host") ?? "localhost";
        var vhost = builder.Configuration.GetValue<string>("RabbitMq:VirtualHost") ?? "/";
        var user = builder.Configuration.GetValue<string>("RabbitMq:Username") ?? "guest";
        var pass = builder.Configuration.GetValue<string>("RabbitMq:Password") ?? "guest";

        cfg.Host(host, vhost, h =>
        {
            h.Username(user);
            h.Password(pass);
        });

        // Endpoint para consumir eventos de venda confirmada e baixar estoque
        cfg.ReceiveEndpoint("estoque-baixar-quantidade", e =>
        {
            e.ConfigureConsumer<BaixarEstoqueQuandoVendaConfirmadaConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplicar migrações do EF Core automaticamente na inicialização para garantir o schema do banco
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
