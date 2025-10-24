using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Compartilhado.Seguranca;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Serviços adicionados ao contêiner
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configurações de JWT (lidas do appsettings)
builder.Services.Configure<ConfiguracoesJwt>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<ServicoJwt>();

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
            ClockSkew = TimeSpan.FromSeconds(5)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();

app.Run();
