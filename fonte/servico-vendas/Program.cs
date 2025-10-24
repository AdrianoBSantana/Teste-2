using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Compartilhado.Seguranca;
using Microsoft.EntityFrameworkCore;
using ServicoVendas.Dados;
using ServicoVendas.Servicos;
using Polly;
using Polly.Extensions.Http;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços básicos da aplicação
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configuração do DbContext com SQLite para persistência de dados de vendas
builder.Services.AddDbContext<VendasDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("VendasDb"))
);

builder.Services.AddAuthorization();

// Configuração do HttpClient para comunicação síncrona com o serviço de Estoque, com políticas de resiliência
builder.Services
    .AddHttpClient<IClienteEstoque, ClienteEstoque>(client =>
    {
        var baseUrl = builder.Configuration.GetSection("Servicos:Estoque:BaseUrl").Get<string>() ?? "http://localhost:5201/";
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(15);
    })
    .AddPolicyHandler(ServicoVendas.Servicos.ResiliencePolicies.GetRetryPolicy()) // Retry exponencial para falhas transitórias
    .AddPolicyHandler(ServicoVendas.Servicos.ResiliencePolicies.GetCircuitBreakerPolicy()); // Circuit breaker para proteger contra falhas prolongadas

// Configuração do MassTransit com RabbitMQ para publicação de eventos assíncronos
builder.Services.AddMassTransit(x =>
{
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
    var db = scope.ServiceProvider.GetRequiredService<VendasDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
