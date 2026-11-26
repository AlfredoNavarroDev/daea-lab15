using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MMLib.SwaggerForOcelot;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var ocelotFileName = builder.Environment.IsProduction() ? "ocelot.Production.json" : "ocelot.json";
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, ocelotFileName),
    optional: false,
    reloadOnChange: true);

var jwtConfig = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtConfig["Emisor"],
        ValidateAudience = true,
        ValidAudience = jwtConfig["Audiencia"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Clave"]!)),
    };
});

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

app.UseSwaggerForOcelotUI();

await app.UseOcelot();

await app.RunAsync();

public partial class Program { }
