# Diseño: API Gateway con Ocelot — TattooStudio (S15)

## Contexto

Laboratorio S15-E (DAEA, TECSUP): construir un API Gateway con Ocelot como
único punto de entrada a dos microservicios, usando el mismo tema de la
Semana 14 (TattooStudio). Microservicio 1 es la API en capas de S14 (sin
cambiar su lógica). Microservicio 2 es un servicio nuevo, pequeño, con datos
en memoria. Tarea obligatoria: publicar los tres en la nube con planes
gratuitos y grabar video de sustentación.

El repositorio de S14 (`daea-lab14`, https://github.com/AlfredoNavarroDev/daea-lab14)
ya existe y fue entregado. Para S15 se pidió que todo quede en **un solo
repositorio nuevo**, incluyendo una copia íntegra (sin modificar lógica) del
código de S14.

## Decisiones (confirmadas con el usuario)

1. **Microservicio 2 = Clientes** del estudio de tatuajes, datos en memoria
   (lista estática), coincide con el ejemplo de la tabla del enunciado
   (`/gateway/clientes{everything}`).
2. **Hosting en la nube = Azure**: App Service Free F1 (un App Service Plan
   F1 compartido por los 3 servicios en el mismo grupo de recursos) + Azure
   SQL Database free tier (serverless, auto-pause) solo para TattooStudio.Api.
   Justificación: se mantiene el proveedor `UseSqlServer` de EF Core sin
   cambios (no hace falta migrar a Npgsql ni adaptar el script SQL), y F1
   permite correr varias apps bajo el mismo plan gratuito.
3. **Un solo repositorio** `daea-lab15`, con copia completa de
   `TattooStudio.Api`, `TattooStudio.Datos`, `TattooStudio.Entidades`,
   `TattooStudio.Negocio` (copiados tal cual desde `daea-lab14`, sin tocar
   lógica) más los proyectos nuevos `GatewayApi` y `ClientesApi`.
4. **Editor**: Visual Studio. Un `.slnx` nuevo (`daea-lab15.slnx`) con los 6
   proyectos, usando "varios proyectos de inicio" para correr GatewayApi +
   TattooStudio.Api + ClientesApi simultáneamente desde una sola ventana.

## Arquitectura

```
Cliente (Postman / app)
        |
        v
   GatewayApi  (puerto fijo 5050, valida JWT "Bearer")
        |
        +--> TattooStudio.Api (puerto 5225, BD SQL Server, S14 sin cambios)
        |
        +--> ClientesApi (puerto 5260, datos en memoria)
```

El cliente solo conoce la dirección del Gateway. El Gateway valida el token
JWT (firma, emisor, audiencia, expiración) en las rutas protegidas antes de
reenviar. Cada servicio downstream valida el JWT de nuevo de forma
independiente (defensa en profundidad) y, en el caso de TattooStudio.Api,
además exige roles específicos (`Recepcionista`, `Tatuador`) vía
`[Authorize(Roles = "...")]`, control que el Gateway no replica.

## Proyectos nuevos

### GatewayApi

- `dotnet new web -n GatewayApi`.
- Paquetes: `Ocelot` (25.x), `Microsoft.AspNetCore.Authentication.JwtBearer` (10.x).
- `Program.cs`: `builder.Configuration.AddJsonFile("ocelot.json", ...)`,
  `builder.Services.AddOcelot()`, `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)`
  con esquema nombrado `"Bearer"` y el mismo `TokenValidationParameters` que
  S14 (Clave/Emisor/Audiencia desde configuración/env var), `await app.UseOcelot()`.
- Puerto fijo en `launchSettings.json`: `5050`.
- `ocelot.json` → `GlobalConfiguration.BaseUrl = "http://localhost:5050"` en
  local; en la versión publicada, la URL pública del Gateway en Azure.

### ClientesApi

- `dotnet new web -n ClientesApi`.
- Paquete: `Microsoft.AspNetCore.Authentication.JwtBearer` (10.x).
- Entidad `Cliente` (en el propio proyecto, sin compartir clases con
  TattooStudio): `Id`, `Nombre`, `Correo`, `Telefono`.
- Datos en memoria: lista estática/singleton inicializada con datos semilla
  (sin base de datos, sin EF Core).
- Endpoints (mínimo 2, ambos `GET`, ambos bajo `[Authorize]` global):
  - `GET /api/clientes` → lista completa.
  - `GET /api/clientes/{id}` → por id (404 si no existe).
- JWT: mismo `Clave`/`Emisor`/`Audiencia` que S14 y el Gateway, vía
  configuración/variable de entorno (nunca hardcodeado, nunca en el repo).
- Puerto fijo en `launchSettings.json`: `5260`.

## Tabla de rutas (`ocelot.json`, versión local)

| Upstream (cliente → gateway) | Método | Downstream (gateway → servicio) | Token |
|---|---|---|---|
| `/gateway/auth/{everything}` | POST | `localhost:5225` `/api/auth/{everything}` | No |
| `/gateway/tatuadores{everything}` | GET | `localhost:5225` `/api/tatuadores{everything}` | No |
| `/gateway/tatuadores{everything}` | POST, PUT, DELETE | `localhost:5225` `/api/tatuadores{everything}` | Sí |
| `/gateway/citastatuaje{everything}` | GET | `localhost:5225` `/api/citastatuaje{everything}` | No |
| `/gateway/citastatuaje{everything}` | POST, PUT, DELETE | `localhost:5225` `/api/citastatuaje{everything}` | Sí |
| `/gateway/clientes{everything}` | GET | `localhost:5260` `/api/clientes{everything}` | Sí |

6 rutas (mínimo pedido: 4), cubriendo lectura pública / escritura con token
en los dos controllers de S14 (`TatuadoresController`, `CitasTatuajeController`)
más el microservicio 2 completo.

Las rutas con token usan `"AuthenticationOptions": { "AuthenticationProviderKey": "Bearer", "AllowedScopes": [] }`
en `ocelot.json`; las públicas no llevan esa sección.

## Validación del JWT (dónde y por qué)

- **Gateway**: valida firma/emisor/audiencia/expiración antes de reenviar
  cualquier ruta marcada con `AuthenticationOptions`. Si el token falta o es
  inválido, el Gateway responde `401` sin llegar al microservicio.
- **TattooStudio.Api (S14)**: valida el JWT otra vez (su propio
  `AddJwtBearer`, sin cambios) y además exige rol (`Recepcionista`/`Tatuador`)
  en escritura — devuelve `403` si el rol no coincide.
- **ClientesApi**: valida el JWT otra vez (mismo esquema), sin chequeo de rol
  (no lo pide el enunciado para este microservicio).
- **Desventaja de repetir la validación**: la clave/emisor/audiencia deben
  mantenerse sincronizados en 3 configuraciones distintas (Gateway, S14,
  Clientes); si la clave rota o cambia en un solo sitio, los demás rechazan
  tokens válidos. Es el trade-off típico de "defensa en profundidad" en
  microservicios: más seguridad, más superficie de configuración que
  mantener consistente.

## Despliegue en la nube (Azure)

- 3× **Azure App Service, plan Free F1**, mismo grupo de recursos y mismo
  App Service Plan F1 (permite varias apps bajo un plan compartido):
  `GatewayApi`, `TattooStudio.Api`, `ClientesApi`.
- **Azure SQL Database, free tier** (serverless, auto-pause, hasta 32 GB)
  para TattooStudio.Api. No requiere cambiar el proveedor EF Core
  (`UseSqlServer` se mantiene) ni adaptar el script SQL.
- `ocelot.json` en la versión publicada: `DownstreamScheme: "https"`,
  `DownstreamHostAndPorts` con el host público de cada App Service
  (`<nombre>.azurewebsites.net`, puerto `443`). `GlobalConfiguration.BaseUrl`
  = URL pública del Gateway.
- Secretos (clave JWT, cadena de conexión) **no van al repo**: se configuran
  como "Application settings" (variables de entorno) de cada App Service en
  Azure, igual que localmente se usan `dotnet user-secrets` o
  `appsettings.Development.json` (ya excluido por `.gitignore`).
- El script SQL de S14 se ejecuta contra la base en la nube antes de probar
  (en plan free el primer request puede tardar por cold start).

## Estructura del repositorio `daea-lab15`

```
daea-lab15/
  daea-lab15.slnx              (6 proyectos: 4 copiados de S14 + 2 nuevos)
  GatewayApi/
    ocelot.json
    Program.cs
    Properties/launchSettings.json
  ClientesApi/
    Program.cs
    Properties/launchSettings.json
  TattooStudio.Api/            (copia íntegra de daea-lab14, sin cambios de lógica)
  TattooStudio.Datos/          (copia íntegra)
  TattooStudio.Entidades/      (copia íntegra)
  TattooStudio.Negocio/        (copia íntegra)
  database/                    (script SQL, copiado de daea-lab14)
  docs/
    superpowers/specs/         (este documento)
  .gitignore                   (bin/, obj/, .vs/, *.user, *.suo, appsettings.Development.json)
  README.md
```

El repositorio `daea-lab14` original queda intacto y se referencia en la
documentación como el origen de S14, pero no es necesario entregarlo de
nuevo — todo lo que pide el enunciado de S15 (código, `ocelot.json`, script
SQL) vive en `daea-lab15`.

## Correr los 3 servicios en local (Visual Studio)

1. Abrir `daea-lab15.slnx`.
2. Clic derecho en la solución → "Configurar proyectos de inicio" → "Varios
   proyectos de inicio" → `Start` en `GatewayApi`, `TattooStudio.Api`,
   `ClientesApi`; `None` en `TattooStudio.Datos/Entidades/Negocio`
   (son librerías, no apps).
3. F5 arranca los 3 a la vez, cada uno en su puerto fijo (5050, 5225, 5260).

## Plan de pruebas (local y nube)

Todo por el Gateway (`http://localhost:5050/gateway/...` en local,
`https://<gateway>.azurewebsites.net/gateway/...` en la nube), con un
archivo `.http` o colección Postman:

| Código | Cómo se provoca |
|---|---|
| 200 | `GET /gateway/tatuadores` sin token |
| 201 | `POST /gateway/auth/registro` (público) |
| 204 | `PUT /gateway/tatuadores/{id}` con token válido |
| 401 | `POST /gateway/tatuadores` sin token, o cualquier ruta protegida sin token |
| 403 | `POST /gateway/tatuadores` con token válido pero rol incorrecto |
| 404 | `GET /gateway/tatuadores/9999` (id inexistente) |
| 502 | Apagar `ClientesApi` y llamar `GET /gateway/clientes` |

Además: llamar directamente a `ClientesApi` (puerto 5260) sin pasar por el
Gateway y sin token → debe responder `401`.

Repetir la misma batería de pruebas contra las URLs públicas de Azure.

## Fuera de alcance

- No se modifica la lógica de negocio, entidades ni DTOs de S14.
- No se comparten clases ni base de datos entre TattooStudio.Api y
  ClientesApi.
- No se implementa autenticación/autorización adicional en el Gateway más
  allá de validar el token (el rol sigue siendo responsabilidad exclusiva de
  cada servicio).
- El video de sustentación, las capturas y el documento Word de entregables
  son pasos manuales posteriores a la implementación, fuera de este diseño
  técnico.
