# Gateway Ocelot + Microservicio Clientes — TattooStudio (S15) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Un único repositorio `daea-lab15` con un API Gateway (Ocelot) como
único punto de entrada a dos microservicios — TattooStudio.Api (copia
íntegra de S14, sin tocar lógica) y ClientesApi (nuevo, datos en
memoria) — ambos protegidos por JWT, listo para correr local en Visual
Studio y para publicarse en MonsterASP.NET (plan free).

**Architecture:** GatewayApi (puerto 5050) reenvía por Ocelot hacia
TattooStudio.Api (puerto 5225, BD SQL Server) y ClientesApi (puerto 5260,
en memoria), validando JWT "Bearer" antes de reenviar en las rutas
protegidas. Cada downstream valida el JWT de nuevo por su cuenta; el rol
(`Recepcionista`/`Tatuador`) solo lo exige TattooStudio.Api.

**Tech Stack:** .NET 10 / ASP.NET Core, Ocelot 25.x, Microsoft.AspNetCore.Authentication.JwtBearer 10.x, xUnit + Microsoft.AspNetCore.Mvc.Testing, SQL Server (local SQLEXPRESS / MonsterASP MSSQL en nube).

**Spec:** `docs/superpowers/specs/2026-11-25-gateway-ocelot-tattoostudio-design.md`

## Global Constraints

- Puertos fijos: GatewayApi `5050`, TattooStudio.Api `5225`, ClientesApi `5260`.
- `TargetFramework` en todos los proyectos nuevos: `net10.0` (igual que los copiados de S14).
- Paquetes: `Ocelot` versión `25.*`, `Microsoft.AspNetCore.Authentication.JwtBearer` versión `10.*`.
- JWT: mismo `Clave`/`Emisor`/`Audiencia` en los 3 servicios, nunca hardcodeado ni en `appsettings.json` versionado — va en `appsettings.Development.json` (gitignored) local, o configuración en el servidor publicado.
- `TattooStudio.Api`, `TattooStudio.Datos`, `TattooStudio.Entidades`, `TattooStudio.Negocio`: copia íntegra desde `..\LabSemana14\`, **cero cambios de lógica**.
- `ClientesApi` no comparte clases ni base de datos con `TattooStudio.*`.
- El Gateway no valida roles — solo token válido. El rol lo sigue exigiendo cada downstream.
- Nada de secretos (claves, cadenas de conexión, perfiles `.pubxml`) va al repo.

---

## File Structure

```
daea-lab15/
  daea-lab15.slnx
  .gitignore
  README.md
  database/
    TattooStudio.sql                       (copiado tal cual de S14)
  TattooStudio.Api/                        (copia íntegra de S14)
  TattooStudio.Datos/                       (copia íntegra de S14)
  TattooStudio.Entidades/                   (copia íntegra de S14)
  TattooStudio.Negocio/                     (copia íntegra de S14)
  ClientesApi/
    ClientesApi.csproj
    Program.cs
    Properties/launchSettings.json
    appsettings.json
    Modelos/Cliente.cs
    Repositorios/IClienteRepository.cs
    Repositorios/ClienteRepository.cs
    Controllers/ClientesController.cs
  ClientesApi.Tests/
    ClientesApi.Tests.csproj
    ClienteRepositoryTests.cs
    ClientesControllerIntegrationTests.cs
  GatewayApi/
    GatewayApi.csproj
    Program.cs
    Properties/launchSettings.json
    appsettings.json
    ocelot.json
    ocelot.Production.json
  GatewayApi.Tests/
    GatewayApi.Tests.csproj
    GatewayIntegrationTests.cs
  docs/
    superpowers/specs/2026-11-25-gateway-ocelot-tattoostudio-design.md
    superpowers/plans/2026-11-25-gateway-ocelot-tattoostudio-plan.md
    despliegue-monsterasp.md
    pruebas-gateway.http
```

Cada proyecto nuevo tiene una sola responsabilidad: `ClientesApi` sirve el
microservicio 2, `ClientesApi.Tests` prueba su lógica y su autenticación,
`GatewayApi` es puro routing/auth de Ocelot, `GatewayApi.Tests` prueba que
el enrutamiento y la autenticación del gateway se comporten sin depender de
que los downstream estén corriendo.

---

### Task 1: Copiar S14 al repo nuevo + esqueleto de solución

**Files:**
- Create: `daea-lab15/.gitignore`
- Create: `daea-lab15/TattooStudio.Api/` (copia de `LabSemana14/TattooStudio.Api/`)
- Create: `daea-lab15/TattooStudio.Datos/` (copia de `LabSemana14/TattooStudio.Datos/`)
- Create: `daea-lab15/TattooStudio.Entidades/` (copia de `LabSemana14/TattooStudio.Entidades/`)
- Create: `daea-lab15/TattooStudio.Negocio/` (copia de `LabSemana14/TattooStudio.Negocio/`)
- Create: `daea-lab15/database/TattooStudio.sql` (copia de `LabSemana14/database/TattooStudio.sql`)
- Create: `daea-lab15/daea-lab15.slnx`

**Interfaces:**
- Produces: proyectos `TattooStudio.Api.csproj`, `TattooStudio.Datos.csproj`, `TattooStudio.Entidades.csproj`, `TattooStudio.Negocio.csproj` disponibles para que `daea-lab15.slnx` (Task 6) los referencie junto con `GatewayApi`/`ClientesApi`.

- [ ] **Step 1: Crear `.gitignore`**

```gitignore
bin/
obj/
.vs/
*.user
*.suo
**/appsettings.Development.json
**/*.pubxml
**/*.pubxml.user
**/PublishProfiles/
```

- [ ] **Step 2: Copiar los 4 proyectos de S14 sin tocar lógica (PowerShell, desde `Semana15/daea-lab15/`)**

```powershell
robocopy "..\LabSemana14\TattooStudio.Api" ".\TattooStudio.Api" /E /XD bin obj
robocopy "..\LabSemana14\TattooStudio.Datos" ".\TattooStudio.Datos" /E /XD bin obj
robocopy "..\LabSemana14\TattooStudio.Entidades" ".\TattooStudio.Entidades" /E /XD bin obj
robocopy "..\LabSemana14\TattooStudio.Negocio" ".\TattooStudio.Negocio" /E /XD bin obj
New-Item -ItemType Directory -Force -Path ".\database" | Out-Null
Copy-Item "..\LabSemana14\database\TattooStudio.sql" ".\database\TattooStudio.sql"
```

Nota: `robocopy` copia también `TattooStudio.Api/appsettings.Development.json`
(con los secretos JWT/cadena de conexión) — queda en disco para poder
correr local, pero el `.gitignore` del Step 1 evita que se suba al repo.

- [ ] **Step 3: Crear `daea-lab15.slnx` con los 4 proyectos copiados**

```xml
<Solution>
  <Project Path="TattooStudio.Api/TattooStudio.Api.csproj" />
  <Project Path="TattooStudio.Datos/TattooStudio.Datos.csproj" />
  <Project Path="TattooStudio.Entidades/TattooStudio.Entidades.csproj" />
  <Project Path="TattooStudio.Negocio/TattooStudio.Negocio.csproj" />
</Solution>
```

- [ ] **Step 4: Verificar que compila**

Run: `dotnet build daea-lab15.slnx`
Expected: `Build succeeded.` para los 4 proyectos, 0 errores.

- [ ] **Step 5: Commit**

```bash
git add .gitignore daea-lab15.slnx TattooStudio.Api TattooStudio.Datos TattooStudio.Entidades TattooStudio.Negocio database
git commit -m "feat: copiar TattooStudio S14 (sin cambios de logica) al repo daea-lab15"
```

---

### Task 2: ClientesApi — modelo y repositorio en memoria (TDD)

**Files:**
- Create: `daea-lab15/ClientesApi/ClientesApi.csproj`
- Create: `daea-lab15/ClientesApi/Modelos/Cliente.cs`
- Create: `daea-lab15/ClientesApi/Repositorios/IClienteRepository.cs`
- Create: `daea-lab15/ClientesApi/Repositorios/ClienteRepository.cs`
- Test: `daea-lab15/ClientesApi.Tests/ClientesApi.Tests.csproj`
- Test: `daea-lab15/ClientesApi.Tests/ClienteRepositoryTests.cs`

**Interfaces:**
- Produces: `ClientesApi.Modelos.Cliente { int Id; string Nombre; string Correo; string Telefono; }`,
  `ClientesApi.Repositorios.IClienteRepository { List<Cliente> ObtenerTodos(); Cliente? ObtenerPorId(int id); }`,
  clase `ClienteRepository : IClienteRepository` con datos semilla fijos (5 clientes, Id 1..5).
- Consumes: nada (es la base del microservicio).

- [ ] **Step 1: Crear el proyecto `ClientesApi` (`dotnet new web`, desde `daea-lab15/`)**

```powershell
dotnet new web -n ClientesApi --framework net10.0
```

- [ ] **Step 2: Crear el proyecto de tests `ClientesApi.Tests`**

```powershell
dotnet new xunit -n ClientesApi.Tests --framework net10.0
dotnet add ClientesApi.Tests reference ClientesApi
dotnet add ClientesApi.Tests package Microsoft.AspNetCore.Mvc.Testing --version "10.*"
```

- [ ] **Step 3: Escribir el test que falla — `ClientesApi.Tests/ClienteRepositoryTests.cs`**

```csharp
using ClientesApi.Repositorios;
using Xunit;

namespace ClientesApi.Tests;

public class ClienteRepositoryTests
{
    [Fact]
    public void ObtenerTodos_DevuelveLosCincoClientesSemilla()
    {
        IClienteRepository repo = new ClienteRepository();

        var clientes = repo.ObtenerTodos();

        Assert.Equal(5, clientes.Count);
    }

    [Fact]
    public void ObtenerPorId_ConIdExistente_DevuelveElCliente()
    {
        IClienteRepository repo = new ClienteRepository();

        var cliente = repo.ObtenerPorId(1);

        Assert.NotNull(cliente);
        Assert.Equal(1, cliente!.Id);
    }

    [Fact]
    public void ObtenerPorId_ConIdInexistente_DevuelveNull()
    {
        IClienteRepository repo = new ClienteRepository();

        var cliente = repo.ObtenerPorId(9999);

        Assert.Null(cliente);
    }
}
```

- [ ] **Step 4: Correr los tests y verificar que fallan (no existe `Cliente`/`ClienteRepository` todavía)**

Run: `dotnet test ClientesApi.Tests`
Expected: FAIL — error de compilación, `ClientesApi.Modelos`/`ClientesApi.Repositorios` no existen.

- [ ] **Step 5: Implementar `Modelos/Cliente.cs`**

```csharp
namespace ClientesApi.Modelos;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Telefono { get; set; } = null!;
}
```

- [ ] **Step 6: Implementar `Repositorios/IClienteRepository.cs`**

```csharp
using ClientesApi.Modelos;

namespace ClientesApi.Repositorios;

public interface IClienteRepository
{
    List<Cliente> ObtenerTodos();
    Cliente? ObtenerPorId(int id);
}
```

- [ ] **Step 7: Implementar `Repositorios/ClienteRepository.cs`**

```csharp
using ClientesApi.Modelos;

namespace ClientesApi.Repositorios;

public class ClienteRepository : IClienteRepository
{
    private readonly List<Cliente> _clientes = new()
    {
        new Cliente { Id = 1, Nombre = "Lucia Fernandez", Correo = "lucia.fernandez@example.com", Telefono = "987654321" },
        new Cliente { Id = 2, Nombre = "Marco Rivas", Correo = "marco.rivas@example.com", Telefono = "987654322" },
        new Cliente { Id = 3, Nombre = "Elena Quispe", Correo = "elena.quispe@example.com", Telefono = "987654323" },
        new Cliente { Id = 4, Nombre = "Diego Salazar", Correo = "diego.salazar@example.com", Telefono = "987654324" },
        new Cliente { Id = 5, Nombre = "Valeria Nunez", Correo = "valeria.nunez@example.com", Telefono = "987654325" },
    };

    public List<Cliente> ObtenerTodos() => _clientes;

    public Cliente? ObtenerPorId(int id) => _clientes.FirstOrDefault(c => c.Id == id);
}
```

- [ ] **Step 8: Correr los tests y verificar que pasan**

Run: `dotnet test ClientesApi.Tests`
Expected: PASS — 3 tests, 0 fallos.

- [ ] **Step 9: Commit**

```bash
git add ClientesApi ClientesApi.Tests
git commit -m "feat: modelo Cliente y repositorio en memoria con tests"
```

---

### Task 3: ClientesApi — JWT, controlador y tests de integración

**Files:**
- Modify: `daea-lab15/ClientesApi/ClientesApi.csproj`
- Create: `daea-lab15/ClientesApi/Program.cs`
- Create: `daea-lab15/ClientesApi/appsettings.json`
- Create: `daea-lab15/ClientesApi/Properties/launchSettings.json`
- Create: `daea-lab15/ClientesApi/Controllers/ClientesController.cs`
- Test: `daea-lab15/ClientesApi.Tests/ClientesControllerIntegrationTests.cs`

**Interfaces:**
- Consumes: `IClienteRepository`/`ClienteRepository` (Task 2), `Cliente { Id, Nombre, Correo, Telefono }` (Task 2).
- Produces: endpoints `GET /api/clientes` → `200 OK` con `List<Cliente>`; `GET /api/clientes/{id:int}` → `200 OK` con `Cliente` o `404 NotFound`. Ambos exigen JWT válido (configuración `Jwt:Clave`/`Jwt:Emisor`/`Jwt:Audiencia`) vía filtro global `AuthorizeFilter`.

- [ ] **Step 1: Agregar el paquete JwtBearer**

```powershell
dotnet add ClientesApi package Microsoft.AspNetCore.Authentication.JwtBearer --version "10.*"
```

- [ ] **Step 2: Escribir el test de integración que falla — `ClientesApi.Tests/ClientesControllerIntegrationTests.cs`**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ClientesApi.Modelos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ClientesApi.Tests;

public class ClientesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Clave = "ClientesApi-Tests-ClaveSecreta-32-Caracteres-Minimo";
    private const string Emisor = "TattooStudio.Api";
    private const string Audiencia = "TattooStudio.Clientes";

    private readonly WebApplicationFactory<Program> _factory;

    public ClientesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Clave"] = Clave,
                    ["Jwt:Emisor"] = Emisor,
                    ["Jwt:Audiencia"] = Audiencia,
                    ["Jwt:MinutosExpiracion"] = "60",
                });
            });
        });
    }

    private static string GenerarTokenDePrueba()
    {
        var claims = new[] { new Claim(ClaimTypes.Email, "test@example.com") };
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Clave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Emisor,
            audience: Audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task GetClientes_SinToken_Devuelve401()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetClientes_ConTokenValido_Devuelve200ConLista()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerarTokenDePrueba());

        var respuesta = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var clientes = await respuesta.Content.ReadFromJsonAsync<List<Cliente>>();
        Assert.NotNull(clientes);
        Assert.NotEmpty(clientes!);
    }

    [Fact]
    public async Task GetClientePorId_ConIdInexistente_Devuelve404()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerarTokenDePrueba());

        var respuesta = await client.GetAsync("/api/clientes/9999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
```

- [ ] **Step 3: Correr los tests y verificar que fallan**

Run: `dotnet test ClientesApi.Tests`
Expected: FAIL — no existe `Program` público, ni `/api/clientes`.

- [ ] **Step 4: Implementar `ClientesApi/appsettings.json` (sin secretos)**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 5: Implementar `ClientesApi/Properties/launchSettings.json` (puerto fijo 5260)**

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5260",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

- [ ] **Step 6: Implementar `ClientesApi/Program.cs`**

```csharp
using System.Text;
using ClientesApi.Repositorios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddSingleton<IClienteRepository, ClienteRepository>();

var jwtConfig = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
```

- [ ] **Step 7: Implementar `ClientesApi/Controllers/ClientesController.cs`**

```csharp
using ClientesApi.Modelos;
using ClientesApi.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace ClientesApi.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _clienteRepository;

    public ClientesController(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    // GET api/clientes
    [HttpGet]
    public ActionResult<List<Cliente>> ObtenerTodos()
    {
        return Ok(_clienteRepository.ObtenerTodos());
    }

    // GET api/clientes/5
    [HttpGet("{id:int}")]
    public ActionResult<Cliente> ObtenerPorId(int id)
    {
        var cliente = _clienteRepository.ObtenerPorId(id);
        if (cliente is null)
        {
            return NotFound();
        }

        return Ok(cliente);
    }
}
```

- [ ] **Step 8: Correr los tests y verificar que pasan**

Run: `dotnet test ClientesApi.Tests`
Expected: PASS — 6 tests en total (3 de repositorio + 3 de integración), 0 fallos.

- [ ] **Step 9: Crear `appsettings.Development.json` local (no se commitea)**

```json
{
  "Jwt": {
    "Clave": "REEMPLAZAR-CON-CLAVE-LOCAL-NO-COMMITEAR",
    "Emisor": "TattooStudio.Api",
    "Audiencia": "TattooStudio.Clientes",
    "MinutosExpiracion": "60"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Guardar este archivo como `ClientesApi/appsettings.Development.json` —
mismos valores que `LabSemana14/TattooStudio.Api/appsettings.Development.json`.
El `.gitignore` de Task 1 ya lo excluye.

- [ ] **Step 10: Commit (sin el `appsettings.Development.json`)**

```bash
git add ClientesApi/ClientesApi.csproj ClientesApi/Program.cs ClientesApi/appsettings.json ClientesApi/Properties/launchSettings.json ClientesApi/Controllers ClientesApi.Tests/ClientesControllerIntegrationTests.cs
git commit -m "feat: autenticacion JWT y endpoints GET de ClientesApi"
```

---

### Task 4: GatewayApi — scaffold, JWT y `ocelot.json` (local)

**Files:**
- Create: `daea-lab15/GatewayApi/GatewayApi.csproj`
- Create: `daea-lab15/GatewayApi/Program.cs`
- Create: `daea-lab15/GatewayApi/appsettings.json`
- Create: `daea-lab15/GatewayApi/Properties/launchSettings.json`
- Create: `daea-lab15/GatewayApi/ocelot.json`

**Interfaces:**
- Consumes: ninguno de los otros proyectos (el Gateway no tiene `ProjectReference`, solo reenvía HTTP).
- Produces: esquema de autenticación nombrado `"Bearer"` que las rutas de `ocelot.json` referencian vía `AuthenticationOptions.AuthenticationProviderKey`.

- [ ] **Step 1: Crear el proyecto `GatewayApi` (desde `daea-lab15/`)**

```powershell
dotnet new web -n GatewayApi --framework net10.0
dotnet add GatewayApi package Ocelot --version "25.*"
dotnet add GatewayApi package Microsoft.AspNetCore.Authentication.JwtBearer --version "10.*"
```

- [ ] **Step 2: Implementar `GatewayApi/appsettings.json` (sin secretos)**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 3: Implementar `GatewayApi/Properties/launchSettings.json` (puerto fijo 5050)**

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5050",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

- [ ] **Step 4: Implementar `GatewayApi/ocelot.json` (6 rutas, versión local)**

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/gateway/auth/{everything}",
      "UpstreamHttpMethod": [ "POST" ],
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [ { "Host": "localhost", "Port": 5225 } ]
    },
    {
      "UpstreamPathTemplate": "/gateway/tatuadores{everything}",
      "UpstreamHttpMethod": [ "GET" ],
      "DownstreamPathTemplate": "/api/tatuadores{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [ { "Host": "localhost", "Port": 5225 } ]
    },
    {
      "UpstreamPathTemplate": "/gateway/tatuadores{everything}",
      "UpstreamHttpMethod": [ "POST", "PUT", "DELETE" ],
      "DownstreamPathTemplate": "/api/tatuadores{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [ { "Host": "localhost", "Port": 5225 } ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    },
    {
      "UpstreamPathTemplate": "/gateway/citastatuaje{everything}",
      "UpstreamHttpMethod": [ "GET" ],
      "DownstreamPathTemplate": "/api/citastatuaje{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [ { "Host": "localhost", "Port": 5225 } ]
    },
    {
      "UpstreamPathTemplate": "/gateway/citastatuaje{everything}",
      "UpstreamHttpMethod": [ "POST", "PUT", "DELETE" ],
      "DownstreamPathTemplate": "/api/citastatuaje{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [ { "Host": "localhost", "Port": 5225 } ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    },
    {
      "UpstreamPathTemplate": "/gateway/clientes{everything}",
      "UpstreamHttpMethod": [ "GET" ],
      "DownstreamPathTemplate": "/api/clientes{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [ { "Host": "localhost", "Port": 5260 } ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5050"
  }
}
```

- [ ] **Step 5: Asegurar que `ocelot.json` se publique junto al `.dll` — agregar a `GatewayApi.csproj`**

```xml
<ItemGroup>
  <None Include="ocelot.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

- [ ] **Step 6: Implementar `GatewayApi/Program.cs`**

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "ocelot.json"),
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

var app = builder.Build();

await app.UseOcelot();

await app.RunAsync();

public partial class Program { }
```

- [ ] **Step 7: Crear `appsettings.Development.json` local (no se commitea)**

```json
{
  "Jwt": {
    "Clave": "REEMPLAZAR-CON-CLAVE-LOCAL-NO-COMMITEAR",
    "Emisor": "TattooStudio.Api",
    "Audiencia": "TattooStudio.Clientes",
    "MinutosExpiracion": "60"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

- [ ] **Step 8: Verificar que compila y arranca**

Run: `dotnet build GatewayApi`
Expected: `Build succeeded.`

Run (manual, deja corriendo en una terminal): `dotnet run --project GatewayApi`
Expected: log `Now listening on: http://localhost:5050`, sin excepciones de Ocelot al cargar `ocelot.json`.

- [ ] **Step 9: Commit**

```bash
git add GatewayApi
git commit -m "feat: GatewayApi con Ocelot y autenticacion JWT Bearer"
```

---

### Task 5: GatewayApi — tests de integración (401 y 502 sin downstream)

**Files:**
- Create: `daea-lab15/GatewayApi.Tests/GatewayApi.Tests.csproj`
- Create: `daea-lab15/GatewayApi.Tests/GatewayIntegrationTests.cs`

**Interfaces:**
- Consumes: `Program` público de `GatewayApi` (Task 4), `GatewayApi/ocelot.json` (Task 4) — la fábrica de test apunta su `ContentRoot` a la carpeta `GatewayApi/` para que Ocelot lo encuentre.
- Produces: nada que otra tarea consuma — es la última capa de verificación del Gateway.

- [ ] **Step 1: Crear el proyecto de tests**

```powershell
dotnet new xunit -n GatewayApi.Tests --framework net10.0
dotnet add GatewayApi.Tests reference GatewayApi
dotnet add GatewayApi.Tests package Microsoft.AspNetCore.Mvc.Testing --version "10.*"
```

- [ ] **Step 2: Escribir los tests — `GatewayApi.Tests/GatewayIntegrationTests.cs`**

```csharp
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GatewayApi.Tests;

public class GatewayIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GatewayIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var gatewayContentRoot = EncontrarCarpetaGatewayApi();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(gatewayContentRoot);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Clave"] = "GatewayApi-Tests-ClaveSecreta-32-Caracteres-Minimo",
                    ["Jwt:Emisor"] = "TattooStudio.Api",
                    ["Jwt:Audiencia"] = "TattooStudio.Clientes",
                    ["Jwt:MinutosExpiracion"] = "60",
                });
            });
        });
    }

    private static string EncontrarCarpetaGatewayApi()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (directorio is not null && !File.Exists(Path.Combine(directorio.FullName, "GatewayApi", "ocelot.json")))
        {
            directorio = directorio.Parent;
        }

        if (directorio is null)
        {
            throw new InvalidOperationException("No se encontro GatewayApi/ocelot.json subiendo desde " + AppContext.BaseDirectory);
        }

        return Path.Combine(directorio.FullName, "GatewayApi");
    }

    [Fact]
    public async Task PostTatuadores_SinToken_Devuelve401_SinLlegarAlDownstream()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.PostAsJsonAsync("/gateway/tatuadores", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetTatuadores_RutaPublica_SinDownstreamCorriendo_Devuelve502()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/gateway/tatuadores");

        Assert.Equal(HttpStatusCode.BadGateway, respuesta.StatusCode);
    }
}
```

- [ ] **Step 3: Correr los tests**

Run: `dotnet test GatewayApi.Tests`
Expected: PASS — 2 tests. El primero confirma que el Gateway corta en `401`
antes de reenviar; el segundo confirma `502` porque en el entorno de test
no hay nada escuchando en el puerto 5225 (así se prueba el caso del
enunciado sin depender de tener TattooStudio.Api corriendo).

- [ ] **Step 4: Commit**

```bash
git add GatewayApi.Tests
git commit -m "test: verificar 401 sin token y 502 sin downstream en GatewayApi"
```

---

### Task 6: Solución final (6 proyectos) + README

**Files:**
- Modify: `daea-lab15/daea-lab15.slnx`
- Create: `daea-lab15/README.md`

**Interfaces:**
- Consumes: todos los `.csproj` creados en Tasks 1-5.
- Produces: nada — es la capa de organización/documentación final.

- [ ] **Step 1: Actualizar `daea-lab15.slnx` con los 6 proyectos**

```xml
<Solution>
  <Project Path="TattooStudio.Api/TattooStudio.Api.csproj" />
  <Project Path="TattooStudio.Datos/TattooStudio.Datos.csproj" />
  <Project Path="TattooStudio.Entidades/TattooStudio.Entidades.csproj" />
  <Project Path="TattooStudio.Negocio/TattooStudio.Negocio.csproj" />
  <Project Path="GatewayApi/GatewayApi.csproj" />
  <Project Path="ClientesApi/ClientesApi.csproj" />
  <Project Path="GatewayApi.Tests/GatewayApi.Tests.csproj" />
  <Project Path="ClientesApi.Tests/ClientesApi.Tests.csproj" />
</Solution>
```

- [ ] **Step 2: Verificar que toda la solución compila**

Run: `dotnet build daea-lab15.slnx`
Expected: `Build succeeded.` para los 8 proyectos (6 de app + 2 de test), 0 errores.

- [ ] **Step 3: Escribir `README.md`**

```markdown
# daea-lab15 — API Gateway (Ocelot) + microservicios TattooStudio

Laboratorio S15-E (DAEA, TECSUP). Diseño completo en
`docs/superpowers/specs/2026-11-25-gateway-ocelot-tattoostudio-design.md`.

## Arquitectura

Cliente -> GatewayApi (puerto 5050, valida JWT) -> TattooStudio.Api
(puerto 5225, BD SQL Server, copia intacta de S14) y ClientesApi (puerto
5260, datos en memoria).

## Proyectos

- `TattooStudio.Api` / `.Datos` / `.Entidades` / `.Negocio`: API en capas
  de S14 (tatuadores, citas de tatuaje, autenticacion), sin cambios de
  logica. Repositorio original: https://github.com/AlfredoNavarroDev/daea-lab14
- `GatewayApi`: unico punto de entrada, Ocelot + JWT.
- `ClientesApi`: microservicio 2, clientes del estudio en memoria.

## Correr los 3 servicios en local (Visual Studio)

1. Abrir `daea-lab15.slnx`.
2. Clic derecho en la solucion -> "Configurar proyectos de inicio" ->
   "Varios proyectos de inicio".
3. `Start` en `GatewayApi`, `TattooStudio.Api`, `ClientesApi`. `None` en
   el resto (son librerias o proyectos de test).
4. F5. Puertos: Gateway `5050`, TattooStudio.Api `5225`, ClientesApi `5260`.

## Secretos locales

Antes de correr, crear a mano (no estan en el repo):
- `TattooStudio.Api/appsettings.Development.json`
- `GatewayApi/appsettings.Development.json`
- `ClientesApi/appsettings.Development.json`

Mismo contenido `Jwt` en los 3 (ver seccion "JWT" en el spec), y la cadena
de conexion SQL Server solo en el de `TattooStudio.Api`.

## Tabla de rutas

| Upstream | Metodo | Downstream | Token |
|---|---|---|---|
| `/gateway/auth/{everything}` | POST | `:5225/api/auth/{everything}` | No |
| `/gateway/tatuadores{everything}` | GET | `:5225/api/tatuadores{everything}` | No |
| `/gateway/tatuadores{everything}` | POST,PUT,DELETE | `:5225/api/tatuadores{everything}` | Si |
| `/gateway/citastatuaje{everything}` | GET | `:5225/api/citastatuaje{everything}` | No |
| `/gateway/citastatuaje{everything}` | POST,PUT,DELETE | `:5225/api/citastatuaje{everything}` | Si |
| `/gateway/clientes{everything}` | GET | `:5260/api/clientes{everything}` | Si |

## Pruebas

Ver `docs/pruebas-gateway.http` para la matriz completa (200/201/204/401/403/404/502).

## Despliegue

Ver `docs/despliegue-monsterasp.md`.
```

- [ ] **Step 4: Commit**

```bash
git add daea-lab15.slnx README.md
git commit -m "docs: README del proyecto y solucion final con 8 proyectos"
```

---

### Task 7: Matriz de pruebas manual por el Gateway (`.http`)

**Files:**
- Create: `daea-lab15/docs/pruebas-gateway.http`

**Interfaces:**
- Consumes: los 3 servicios corriendo en local (Tasks 1-6) en los puertos 5050/5225/5260.
- Produces: nada — es evidencia manual, no código consumido por otra tarea.

- [ ] **Step 1: Crear `docs/pruebas-gateway.http` con la matriz completa**

```http
@gateway = http://localhost:5050
@correo = cliente.prueba@example.com
@clave = ClaveSegura123

### 1. Registro (publico) -> 201
POST {{gateway}}/gateway/auth/registro
Content-Type: application/json

{
  "correo": "{{correo}}",
  "clave": "{{clave}}"
}

### 2. Login (publico) -> 200, copiar el "token" de la respuesta para las siguientes llamadas
POST {{gateway}}/gateway/auth/login
Content-Type: application/json

{
  "correo": "{{correo}}",
  "clave": "{{clave}}"
}

### 3. GET tatuadores sin token (publico) -> 200
GET {{gateway}}/gateway/tatuadores

### 4. GET tatuador con id inexistente -> 404
GET {{gateway}}/gateway/tatuadores/999999

### 5. POST tatuador SIN token -> 401
POST {{gateway}}/gateway/tatuadores
Content-Type: application/json

{
  "nombre": "Prueba Gateway",
  "especialidad": "Realismo",
  "aniosExperiencia": 3
}

### 6. POST tatuador CON token (rol Recepcionista, por defecto al registrarse) -> 201
POST {{gateway}}/gateway/tatuadores
Authorization: Bearer PEGAR_TOKEN_DEL_PASO_2
Content-Type: application/json

{
  "nombre": "Prueba Gateway",
  "especialidad": "Realismo",
  "aniosExperiencia": 3
}

### 7. PUT tatuador (usar el id devuelto en el paso 6) -> 204
PUT {{gateway}}/gateway/tatuadores/ID_DEL_PASO_6
Authorization: Bearer PEGAR_TOKEN_DEL_PASO_2
Content-Type: application/json

{
  "nombre": "Prueba Gateway Editado",
  "especialidad": "Old School",
  "aniosExperiencia": 4,
  "activo": true
}

### 8. POST tatuador CON token de rol distinto a Recepcionista -> 403
### Antes de correr este paso: en SSMS, UPDATE Usuarios SET Rol = 'Tatuador' WHERE Correo = '{{correo}}',
### volver a hacer login (paso 2) para obtener un token con Rol=Tatuador, y pegarlo abajo.
POST {{gateway}}/gateway/tatuadores
Authorization: Bearer PEGAR_TOKEN_CON_ROL_TATUADOR
Content-Type: application/json

{
  "nombre": "No deberia crearse",
  "especialidad": "Realismo",
  "aniosExperiencia": 1
}

### 9. GET clientes CON token -> 200
GET {{gateway}}/gateway/clientes
Authorization: Bearer PEGAR_TOKEN_DEL_PASO_2

### 10. GET clientes SIN token -> 401
GET {{gateway}}/gateway/clientes

### 11. GET clientes directo al microservicio (sin pasar por el Gateway), sin token -> 401
GET http://localhost:5260/api/clientes

### 12. GET clientes por el Gateway con ClientesApi APAGADO -> 502
### Antes de correr este paso: detener el proceso de ClientesApi y volver a llamar.
GET {{gateway}}/gateway/clientes
Authorization: Bearer PEGAR_TOKEN_DEL_PASO_2
```

- [ ] **Step 2: Ejecutar manualmente contra los 3 servicios locales corriendo**

Correr los 12 requests en orden (Visual Studio Editor de `.http` o
extension REST Client), reemplazando los placeholders `PEGAR_TOKEN_...` e
`ID_DEL_PASO_6` con los valores reales de cada respuesta. Capturar pantalla
de cada código de respuesta para el Word de evidencias.

Expected: los códigos de cada bloque coinciden exactamente con el
comentario `-> NNN` de cada uno.

- [ ] **Step 3: Commit**

```bash
git add docs/pruebas-gateway.http
git commit -m "test: matriz manual de pruebas por el gateway (200/201/204/401/403/404/502)"
```

---

### Task 8: `ocelot.Production.json` + guía de despliegue en MonsterASP.NET

**Files:**
- Create: `daea-lab15/GatewayApi/ocelot.Production.json`
- Create: `daea-lab15/docs/despliegue-monsterasp.md`
- Modify: `daea-lab15/GatewayApi/Program.cs` (carga de `ocelot.json` por entorno)
- Modify: `daea-lab15/GatewayApi/GatewayApi.csproj` (copiar `ocelot.Production.json` al output)

**Interfaces:**
- Consumes: estructura de rutas de `GatewayApi/ocelot.json` (Task 4) — mismo set de rutas, cambia solo host/puerto/esquema por entorno.
- Produces: nada — es la última tarea, de documentación/config para el despliegue manual que hace el usuario en el panel de MonsterASP.NET.

- [ ] **Step 1: Crear `GatewayApi/ocelot.Production.json`**

Los valores `TATTOOSTUDIO-API-SUBDOMINIO`, `CLIENTESAPI-SUBDOMINIO` y
`GATEWAY-SUBDOMINIO` son marcadores que el usuario reemplaza por los
subdominios reales que MonsterASP.NET asigna al crear cada sitio — no se
conocen hasta ese momento, por eso quedan explícitamente marcados como
"reemplazar" en vez de inventados.

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/gateway/auth/{everything}",
      "UpstreamHttpMethod": [ "POST" ],
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [ { "Host": "TATTOOSTUDIO-API-SUBDOMINIO.runasp.net", "Port": 443 } ]
    },
    {
      "UpstreamPathTemplate": "/gateway/tatuadores{everything}",
      "UpstreamHttpMethod": [ "GET" ],
      "DownstreamPathTemplate": "/api/tatuadores{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [ { "Host": "TATTOOSTUDIO-API-SUBDOMINIO.runasp.net", "Port": 443 } ]
    },
    {
      "UpstreamPathTemplate": "/gateway/tatuadores{everything}",
      "UpstreamHttpMethod": [ "POST", "PUT", "DELETE" ],
      "DownstreamPathTemplate": "/api/tatuadores{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [ { "Host": "TATTOOSTUDIO-API-SUBDOMINIO.runasp.net", "Port": 443 } ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    },
    {
      "UpstreamPathTemplate": "/gateway/citastatuaje{everything}",
      "UpstreamHttpMethod": [ "GET" ],
      "DownstreamPathTemplate": "/api/citastatuaje{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [ { "Host": "TATTOOSTUDIO-API-SUBDOMINIO.runasp.net", "Port": 443 } ]
    },
    {
      "UpstreamPathTemplate": "/gateway/citastatuaje{everything}",
      "UpstreamHttpMethod": [ "POST", "PUT", "DELETE" ],
      "DownstreamPathTemplate": "/api/citastatuaje{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [ { "Host": "TATTOOSTUDIO-API-SUBDOMINIO.runasp.net", "Port": 443 } ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    },
    {
      "UpstreamPathTemplate": "/gateway/clientes{everything}",
      "UpstreamHttpMethod": [ "GET" ],
      "DownstreamPathTemplate": "/api/clientes{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [ { "Host": "CLIENTESAPI-SUBDOMINIO.runasp.net", "Port": 443 } ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://GATEWAY-SUBDOMINIO.runasp.net"
  }
}
```

- [ ] **Step 2: Cablear la carga del archivo por entorno en `GatewayApi/Program.cs`**

Reemplazar el `AddJsonFile` fijo del Step 6 de Task 4 por una carga que
elige el archivo según el entorno:

```csharp
var ocelotFileName = builder.Environment.IsProduction() ? "ocelot.Production.json" : "ocelot.json";
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, ocelotFileName),
    optional: false,
    reloadOnChange: true);
```

- [ ] **Step 3: Agregar `ocelot.Production.json` a `GatewayApi.csproj` para que se publique**

```xml
<ItemGroup>
  <None Include="ocelot.json" CopyToOutputDirectory="PreserveNewest" />
  <None Include="ocelot.Production.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

- [ ] **Step 4: Verificar que la app sigue arrancando en local (entorno Development, usa `ocelot.json`)**

Run: `dotnet build GatewayApi`
Expected: `Build succeeded.`

Run (manual): `dotnet run --project GatewayApi`
Expected: sigue levantando en `http://localhost:5050` usando `ocelot.json` (no `ocelot.Production.json`), porque `ASPNETCORE_ENVIRONMENT=Development` en `launchSettings.json`.

- [ ] **Step 5: Escribir `docs/despliegue-monsterasp.md`**

```markdown
# Despliegue en MonsterASP.NET (plan free)

Panel: https://admin.monsterasp.net/app/dashboard

## 1. Crear la base de datos

1. En el panel, crear una base de datos MSSQL gratuita.
2. Anotar el servidor, nombre de base, usuario y clave que entrega el panel.
3. Conectar con SSMS a esa base y ejecutar `database/TattooStudio.sql`.
4. Verificar con:
   ```sql
   SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId;
   ```

## 2. Crear los 3 sitios

Crear 3 sitios gratuitos (uno por servicio) y anotar el subdominio
`*.runasp.net` que asigna cada uno:

- `GatewayApi` -> anotar como GATEWAY-SUBDOMINIO
- `TattooStudio.Api` -> anotar como TATTOOSTUDIO-API-SUBDOMINIO
- `ClientesApi` -> anotar como CLIENTESAPI-SUBDOMINIO

## 3. Reemplazar los marcadores en `ocelot.Production.json`

En `GatewayApi/ocelot.Production.json`, reemplazar
`TATTOOSTUDIO-API-SUBDOMINIO`, `CLIENTESAPI-SUBDOMINIO` y
`GATEWAY-SUBDOMINIO` por los subdominios reales anotados en el paso 2.

## 4. Configurar secretos en cada sitio (NO en el repo)

En el panel de cada sitio, configurar las variables de entorno /
`appsettings.Production.json` generado en el servidor:

- `TattooStudio.Api`: cadena de conexión a la base del paso 1, y la
  sección `Jwt` (misma Clave/Emisor/Audiencia que en local).
- `GatewayApi`: sección `Jwt` (misma Clave/Emisor/Audiencia).
- `ClientesApi`: sección `Jwt` (misma Clave/Emisor/Audiencia).

## 5. Publicar los 3 proyectos

En Visual Studio, para cada uno de `GatewayApi`, `TattooStudio.Api`,
`ClientesApi`: clic derecho -> Publicar -> importar el perfil `.pubxml`
que descarga el panel de MonsterASP.NET para ese sitio -> Publicar.

## 6. Probar

Repetir la matriz de `docs/pruebas-gateway.http` reemplazando
`http://localhost:5050` por `https://GATEWAY-SUBDOMINIO.runasp.net`. En
el plan free el primer request puede tardar (cold start del pool de IIS).
```

- [ ] **Step 6: Commit**

```bash
git add GatewayApi/ocelot.Production.json GatewayApi/GatewayApi.csproj GatewayApi/Program.cs docs/despliegue-monsterasp.md
git commit -m "feat: ocelot.Production.json y guia de despliegue en MonsterASP.NET"
```

---

## Spec coverage check (autorrevisión)

- Microservicio 1 sin cambios -> Task 1.
- Microservicio 2 (Clientes, en memoria, JWT en todos los endpoints) -> Tasks 2-3.
- GatewayApi con Ocelot 25.x + JwtBearer 10.x, `AddJsonFile`/`AddOcelot`/`UseOcelot` -> Task 4.
- Mínimo 4 rutas, prefijo `/gateway/`, `{everything}` -> Task 4 (6 rutas).
- Gateway valida JWT esquema "Bearer" antes de reenviar -> Task 4 + Task 5.
- Cliente solo usa el puerto del Gateway -> Task 7 (todas las pruebas pasan por `:5050`).
- Despliegue en la nube con `ocelot.json` apuntando a hosts públicos / `BaseUrl` del Gateway -> Task 8.
- Secretos fuera del repo -> Task 1 (`.gitignore`), reforzado en Tasks 3/4/8.
- Pruebas 200/201/204/401/403/404/502 + llamada directa al microservicio 2 sin token -> Task 7 (manual) + Task 5 (automatizado para 401/502).
- Un solo repositorio con copia de S14 -> Task 1.
- `.slnx` con proyectos de inicio múltiples (Visual Studio) -> Task 6.
