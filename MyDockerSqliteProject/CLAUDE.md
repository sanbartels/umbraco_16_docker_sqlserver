# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Proyecto Overview

Este es un proyecto Umbraco CMS v16.3.0 (.NET 9.0) configurado para ejecutarse en Docker con SQLite como base de datos. El proyecto está optimizado para despliegue en Coolify con soporte para proxy reverso.

## Comandos Principales

### Desarrollo Local
```bash
# Construir y ejecutar con Docker
docker compose up --build

# Ejecutar sin Docker (requiere .NET 9 SDK)
dotnet restore
dotnet build
dotnet run

# Limpiar la base de datos (eliminar volúmenes)
docker compose down -v
```

### Build y Test
```bash
# Compilar el proyecto
dotnet build MyDockerSqliteProject.csproj

# Publicar para producción
dotnet publish MyDockerSqliteProject.csproj -c Release -o ./publish

# Ver información de .NET
dotnet --info
```

### Acceso al CMS
- **Frontend**: `http://localhost:8080`
- **Backoffice**: `http://localhost:8080/umbraco`

## Arquitectura del Proyecto

### Configuración de Proxy Reverso (Coolify)

El proyecto está configurado específicamente para funcionar detrás de un proxy reverso como Coolify:

1. **Forwarded Headers** (`Program.cs:6-15`): Configuración crítica que procesa headers `X-Forwarded-For`, `X-Forwarded-Proto` y `X-Forwarded-Host` del proxy.

2. **URL Pública Dinámica** (`Program.cs:18-31`): Lee las URLs desde variables de entorno o configuración:
   - `UmbracoApplicationUrl`: URL pública del sitio
   - `BackOfficeHost`: URL del backoffice
   - Fallback: `http://localhost:8080`

3. **OAuth/OpenIddict**: Umbraco usa OpenIddict para autenticación del backoffice. Configuración CRÍTICA:
   - `UmbracoApplicationUrl`: URL pública del sitio (debe ser HTTPS en producción)
   - `BackOfficeHost`: URL del backoffice (debe ser HTTPS en producción)
   - OpenIddict requiere HTTPS para autenticación por seguridad

### Variables de Entorno Importantes

⚠️ **CRÍTICO**: La URL pública **DEBE usar HTTPS** para que OAuth/OpenIddict funcione.

En Coolify o producción, configurar:
```bash
# REQUERIDA: Debe ser HTTPS para que OAuth funcione correctamente
PUBLIC_URL=https://tu-dominio.com

# Variables de Umbraco (automáticas desde docker-compose.yml)
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
Umbraco__CMS__WebRouting__UmbracoApplicationUrl=${PUBLIC_URL}
Umbraco__CMS__Security__BackOfficeHost=${PUBLIC_URL}
```

**Notas importantes**:
- El contenedor usa HTTP internamente (puerto 8080)
- El proxy reverso de Coolify maneja HTTPS
- Los headers `X-Forwarded-Proto`, `X-Forwarded-For` y `X-Forwarded-Host` indican al contenedor que la petición original era HTTPS
- Si cambias de HTTP a HTTPS después de instalar, debes eliminar el volumen de la base de datos y reinstalar

### Estructura de Archivos

- **Program.cs**: Entry point con configuración de forwarded headers y Umbraco
- **Models/**:
  - `*.generated.cs`: Modelos generados automáticamente por ModelsBuilder (Umbraco)
  - `DockerChecksRemover.cs`: Composer que desactiva la validación HTTPS (para desarrollo en Docker)
- **Views/**: Razor views de Umbraco
- **wwwroot/**: Archivos estáticos
- **umbraco/Data/**: Base de datos SQLite (persistida en volumen Docker)

### Base de Datos

- **Tipo**: SQLite (para desarrollo/staging)
- **Connection String**: `Data Source=|DataDirectory|/Umbraco.sqlite.db`
- **Ubicación**: `umbraco/Data/Umbraco.sqlite.db`
- **Persistencia Docker**: Volumen `umbraco-data` y `media`

### ModelsBuilder

Configurado en modo `SourceCodeAuto` (appsettings.Development.json):
- Los modelos se generan automáticamente en el directorio `Models/`
- Los archivos `*.generated.cs` NO deben editarse manualmente
- Se regeneran cuando cambias los Document Types en el backoffice

### Unattended Installation

El proyecto usa instalación desatendida de Umbraco:
```json
"Unattended": {
    "InstallUnattended": true,
    "PackageMigrationsUnattended": true,
    "UpgradeUnattended": true
}
```
Esto permite que Umbraco se instale automáticamente en el primer arranque sin asistente manual.

## Solución de Problemas Comunes

### Error OAuth: "The mandatory 'code_challenge' parameter is missing"
**Causa**: La URL pública está configurada con HTTP en lugar de HTTPS.

**Solución**:
1. ✅ Verificar que `PUBLIC_URL` use HTTPS (no HTTP): `https://tu-dominio.com`
2. ✅ Configurar la variable en Coolify si es necesario
3. ✅ Si cambiaste de HTTP a HTTPS después de instalar:
   - Eliminar el volumen de la base de datos en Coolify
   - Re-desplegar la aplicación
   - Umbraco se reinstalará con la configuración correcta
4. ✅ Verificar los logs: deben mostrar `BackOffice Host: https://...`

**Comando local para limpiar**:
```bash
docker compose down -v && docker compose up --build
```

### Error: "Invalid redirect_uri"
**Causa**: La URL pública configurada no coincide con la URL desde la que accedes.

**Solución**: Reiniciar con base de datos limpia y URL correcta configurada desde el inicio.

## Despliegue en Coolify

Ver `COOLIFY_SETUP.md` para instrucciones detalladas. Puntos clave:

1. Configurar `PUBLIC_URL` como variable de entorno en Coolify
2. El dominio debe coincidir exactamente con la URL pública
3. Preferir HTTPS en producción
4. Coolify maneja SSL automáticamente con Let's Encrypt

## Configuración Especial

### Docker Health Checks Desactivados
El composer `DockerChecksRemover` (Models/DockerChecksRemover.cs) desactiva la validación de HTTPS requerida normalmente por Umbraco, ya que en entornos Docker/Coolify, HTTPS se maneja en el proxy reverso, no en la aplicación.

### Logging
- Console logging habilitado para Docker
- OpenIddict logging en nivel "Information" para debugging de OAuth
- Logs disponibles via `docker compose logs -f`
