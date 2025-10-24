using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Compartilhado.Seguranca;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços básicos da aplicação
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do YARP (Yet Another Reverse Proxy) para roteamento de requisições aos microserviços
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configurações de JWT (lidas da seção "Jwt" no appsettings.json)
builder.Services.Configure<ConfiguracoesJwt>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<ServicoJwt>();

// Configuração da autenticação JWT
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
            ClockSkew = TimeSpan.FromSeconds(5) // Tolerância de 5 segundos para expiração do token
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Mapeamento do proxy reverso para rotear requisições aos serviços downstream
app.MapReverseProxy();

app.Run();
