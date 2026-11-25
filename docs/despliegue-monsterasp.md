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
