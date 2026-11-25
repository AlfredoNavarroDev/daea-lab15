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

`No` = el Gateway no exige token en esta ruta; los GET de
tatuadores/citastatuaje igual requieren JWT porque `TattooStudio.Api` tiene
`[Authorize]` a nivel de clase (heredado de S14).

## Pruebas

Ver `docs/pruebas-gateway.http` para la matriz completa (200/201/204/401/403/404/502).

Pruebas automatizadas con `dotnet test`:
- `ClientesApi.Tests`: repositorio en memoria, controlador y comportamiento
  JWT (401 sin token / 200 con token válido / 404 id inexistente).
- `GatewayApi.Tests`: ruteo y autenticación del Gateway (401 sin token,
  502 sin downstream). Correr con los servicios downstream apagados (al
  menos sin nada escuchando en el puerto 5225) — si no, los tests que
  esperan 502 pueden leerse como falla aunque el código esté bien.

## Despliegue

Ver `docs/despliegue-monsterasp.md`.
