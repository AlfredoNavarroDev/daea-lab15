# daea-lab15 — API Gateway (Ocelot) + microservicios TattooStudio

Laboratorio S15-E (DAEA, TECSUP). Diseño completo en
`docs/superpowers/specs/2026-11-25-gateway-ocelot-tattoostudio-design.md` —
leerlo antes de tocar código. Este archivo son las reglas duras que no
deben romperse, para cualquier agente de código que trabaje en este repo.

## Qué hay en este repo

- `TattooStudio.Api`, `TattooStudio.Datos`, `TattooStudio.Entidades`,
  `TattooStudio.Negocio` — copia íntegra del laboratorio S14 (repo
  original: `daea-lab14`). **Lógica de negocio congelada: no se edita.** Si
  hace falta un cambio ahí, es señal de que el diseño está mal, no de que
  haya que tocar S14 — pausar y preguntar al usuario.
- `GatewayApi` — Ocelot 25.x + JwtBearer 10.x. Único punto de entrada para
  el cliente.
- `ClientesApi` — microservicio 2, datos en memoria (sin EF Core, sin base
  de datos).

## Reglas duras

- **Puertos fijos, no renegociar**: GatewayApi `5050`, TattooStudio.Api
  `5225`, ClientesApi `5260`.
- **JWT**: misma `Clave`/`Emisor`/`Audiencia` en los 3 servicios. Nunca
  hardcodeada en código ni en `appsettings.json` versionado — va en
  `appsettings.Development.json` (excluido por `.gitignore`) en local, o en
  variable de entorno / `appsettings.Production.json` generado en el
  servidor.
- **Nada de secretos al repo**: cadena de conexión SQL Server, clave JWT,
  perfiles `.pubxml` de publicación. Si una tarea implica commitear algo
  que toque alguno de estos, detenerse y confirmar con el usuario primero.
- **ClientesApi y TattooStudio.Api no comparten clases ni base de datos**
  entre sí. Si una entidad "se parece" entre ambos, se define de nuevo en
  el proyecto que la necesita, no se reutiliza.
- **El Gateway no valida roles.** Solo valida que el token sea válido
  (firma/emisor/audiencia/expiración). El rol (`Recepcionista`, `Tatuador`)
  lo sigue exigiendo cada servicio downstream con `[Authorize(Roles=...)]`
  — no agregar lógica de rol a `ocelot.json`.
- **Las migraciones de EF Core las corre el usuario**, nunca el agente.
  Esto en principio no debería aplicar aquí porque `TattooStudio.Datos` no
  se toca (ver arriba).
- **Hosting**: MonsterASP.NET, plan free (confirmado por el usuario: 5
  websites + 5 databases incluidos). 3 sitios independientes bajo
  `*.runasp.net`, 1 base MSSQL gratuita para TattooStudio.Api. No cambiar
  de proveedor de hosting sin que el usuario lo pida explícitamente.
- **`ocelot.json` tiene dos versiones**: local (`localhost`, http, puertos
  fijos) y publicada (host `*.runasp.net`, `DownstreamScheme: "https"`,
  puerto `443`). No mezclar — la versión publicada no debe apuntar a
  localhost.

## Tabla de rutas (referencia rápida — detalle completo en el spec)

| Upstream | Método | Downstream local | Token |
|---|---|---|---|
| `/gateway/auth/{everything}` | POST | `:5225/api/auth/{everything}` | No |
| `/gateway/tatuadores{everything}` | GET | `:5225/api/tatuadores{everything}` | No |
| `/gateway/tatuadores{everything}` | POST,PUT,DELETE | `:5225/api/tatuadores{everything}` | Sí |
| `/gateway/citastatuaje{everything}` | GET | `:5225/api/citastatuaje{everything}` | No |
| `/gateway/citastatuaje{everything}` | POST,PUT,DELETE | `:5225/api/citastatuaje{everything}` | Sí |
| `/gateway/clientes{everything}` | GET | `:5260/api/clientes{everything}` | Sí |

## Flujo de trabajo

- Cambios de código → correr `dotnet build` antes de dar la tarea por
  terminada.
- Commits/push solo cuando el usuario lo pida explícitamente.
- Antes de implementar algo no cubierto por el spec: preguntar, no asumir.
