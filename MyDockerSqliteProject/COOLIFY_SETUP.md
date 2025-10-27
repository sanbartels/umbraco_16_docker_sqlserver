# Configuración de Umbraco en Coolify

## 🔧 Variables de Entorno Requeridas

Para que Umbraco funcione correctamente detrás del proxy reverso de Coolify, necesitas configurar la siguiente variable de entorno en Coolify:

### Variable de Entorno Principal

⚠️ **IMPORTANTE:** Debes usar **HTTPS** en la URL pública para que OAuth/OpenIddict funcione correctamente.

Tu dominio actual configurado: `https://lsoco8cokk8k8cowks8wog8w.147.93.6.245.sslip.io`

Para configurar un dominio diferente, agrega esta variable de entorno en Coolify:

```
PUBLIC_URL=https://tu-dominio.com
```

### ¿Por qué HTTPS es obligatorio?

Umbraco 14+ usa OpenIddict para autenticación del backoffice, que **requiere HTTPS** por seguridad. Aunque el contenedor Docker usa HTTP internamente, el proxy reverso de Coolify maneja HTTPS y los headers `X-Forwarded-Proto` indican al contenedor que la petición original era HTTPS.

### Configuración Actual

El proyecto está configurado con:
- **UmbracoApplicationUrl**: Define la URL pública del sitio
- **BackOfficeHost**: Define la URL del backoffice (debe ser HTTPS)
- **ForwardedHeaders**: Middleware que procesa headers del proxy reverso

## 📝 Pasos para Configurar en Coolify

1. **Accede a tu aplicación en Coolify**
    - Ve al dashboard de Coolify
    - Selecciona tu aplicación Umbraco

2. **Configura el Dominio con SSL/HTTPS**
    - Ve a la pestaña "Domains"
    - Asegúrate de que tu dominio tenga SSL habilitado (Let's Encrypt automático)
    - Si usas el dominio de Coolify (*.sslip.io), HTTPS debería estar habilitado automáticamente

3. **Configura la Variable de Entorno (si necesitas un dominio diferente)**
    - Ve a la pestaña "Environment Variables"
    - Agrega: `PUBLIC_URL` = `https://tu-dominio-real.com` (⚠️ **DEBE incluir https://**)
    - Guarda los cambios

4. **Re-despliega la Aplicación**
    - Haz clic en "Redeploy" para aplicar los cambios
    - Espera a que el despliegue se complete
    - Verifica los logs para confirmar la configuración

5. **Accede al Admin de Umbraco**
    - Ve a: `https://tu-dominio.com/umbraco`
    - El error de OAuth debería estar resuelto
    - Si es la primera vez, completa la instalación de Umbraco

## ⚙️ Configuración Adicional

### Dominio Personalizado

Si usas un dominio personalizado (no el de Coolify):

1. Configura el dominio en Coolify
2. Asegúrate de que el DNS esté apuntando correctamente
3. Actualiza la variable `PUBLIC_URL` con tu dominio personalizado
4. Re-despliega la aplicación

### SSL/HTTPS

Coolify maneja automáticamente SSL con Let's Encrypt. Asegúrate de que:

-   Tu dominio esté apuntando a Coolify
-   SSL esté habilitado en la configuración de la aplicación
-   Usa siempre `https://` en la variable `PUBLIC_URL`

## 🐛 Solución de Problemas

### Error: "The mandatory 'code_challenge' parameter is missing"

**Causa:** La URL pública está configurada con HTTP en lugar de HTTPS, o los headers del proxy reverso no se están procesando correctamente.

**Solución:**

1. ✅ **Verifica que uses HTTPS**:
   - La URL debe ser `https://tu-dominio.com` (no `http://`)
   - Verifica en los logs que aparezca: `BackOffice Host: https://...`

2. ✅ **Verifica la configuración en Coolify**:
   - Variable de entorno `PUBLIC_URL` debe incluir `https://`
   - El dominio debe tener SSL habilitado en Coolify

3. ✅ **Limpia la base de datos si es necesario**:
   - Si cambiaste de HTTP a HTTPS después de instalar, necesitas reinstalar
   - En Coolify, elimina el volumen `umbraco-data`
   - Re-despliega la aplicación

4. ✅ **Verifica los logs**:
   ```bash
   # En Coolify, ve a la pestaña "Logs" y busca:
   ========================================
   Umbraco Public URL: https://...
   BackOffice Host: https://...
   Environment: Development
   ========================================
   ```

### Error: "Invalid redirect_uri"

**Causa:** La URL pública configurada en la base de datos no coincide con la URL desde la que accedes.

**Solución:**

1. Limpia la base de datos SQLite (elimina el volumen en Coolify)
2. Configura correctamente `PUBLIC_URL` con HTTPS
3. Re-despliega la aplicación
4. Umbraco se reinstalará con la configuración correcta

### Error: "This server only accepts HTTPS requests"

**Causa:** El middleware de ForwardedHeaders no está procesando correctamente el header `X-Forwarded-Proto`.

**Solución:**

1. Verifica que `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` esté configurado (ya lo está)
2. Verifica que el proxy reverso de Coolify esté enviando los headers correctos
3. Revisa los logs de Coolify para confirmar que la petición original era HTTPS

## 📚 Referencias

-   [Documentación de Umbraco sobre WebRouting](https://docs.umbraco.com/umbraco-cms/reference/configuration/webroutingsettings)
-   [OpenIddict Error ID2029](https://documentation.openiddict.com/errors/ID2029)
-   [Coolify Documentation](https://coolify.io/docs)
