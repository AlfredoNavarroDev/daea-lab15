# Despliegue en MonsterASP.NET (plan free)

Panel: https://admin.monsterasp.net/app/dashboard

## 1. Crear la base de datos

1. En el panel, crear una base de datos MSSQL gratuita.
2. Anotar el servidor, nombre de base, usuario y clave que entrega el panel.
3. Conectar con SSMS a esa base y ejecutar `database/TattooStudio.monsterasp.sql`
   (no el original `database/TattooStudio.sql`). El panel de MonsterASP.NET
   bloquea directamente `CREATE DATABASE` — por eso existe esta variante sin
   ese bloque ni el `USE TattooStudio` inicial; es el mismo contenido que el
   script original de S14, solo recortado arriba, conectado directamente a la
   base que ya te entrega el panel.
   `database/TattooStudio.monsterasp.sql` usa el nombre de columna `SeñaPagada` (con
   `ñ`) y está guardado en UTF-8 sin BOM — abrirlo en SSMS forzando
   codificación UTF-8 (o ejecutar con `sqlcmd -f 65001`) para que esa `ñ` no
   se corrompa por un fallback a la codepage ANSI de la máquina/locale del
   servidor.
4. Verificar con:
   ```sql
   SELECT name FROM sys.tables;
   SELECT COUNT(*) FROM dbo.Tatuador;
   ```
   (Este proyecto es database-first — `TattooStudio.sql` es un script DDL
   plano, no hay migraciones de EF Core, así que no existe tabla
   `__EFMigrationsHistory`; esa consulta siempre falla aquí.)

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

- `Jwt:Clave` de producción: generar un valor NUEVO (32+ caracteres
  aleatorios, p.ej. `openssl rand -base64 32`) específico para el
  servidor. **Nunca debe ser igual a la clave de desarrollo local** ni
  subirse al repo — `appsettings.Production.json` ya está en
  `.gitignore`. `Jwt:Emisor` y `Jwt:Audiencia` sí pueden mantenerse
  iguales entre entornos (no son secretos).
- `TattooStudio.Api`: cadena de conexión a la base del paso 1, y la
  sección `Jwt` (misma Clave/Emisor/Audiencia de producción en los 3
  sitios — ver punto anterior).
- `GatewayApi`: sección `Jwt` (misma Clave/Emisor/Audiencia de producción).
- `ClientesApi`: sección `Jwt` (misma Clave/Emisor/Audiencia de producción).

## 5. Publicar los 3 proyectos

En Visual Studio, para cada uno de `GatewayApi`, `TattooStudio.Api`,
`ClientesApi`: clic derecho -> Publicar -> importar el perfil `.pubxml`
que descarga el panel de MonsterASP.NET para ese sitio -> Publicar.

## 6. Probar

Repetir la matriz de `docs/pruebas-gateway.http` reemplazando
`http://localhost:5050` por `https://GATEWAY-SUBDOMINIO.runasp.net`. En
el plan free el primer request puede tardar (cold start del pool de IIS).
