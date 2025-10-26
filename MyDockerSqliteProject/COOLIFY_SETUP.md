# Configuración de Umbraco en Coolify

## 🔧 Variables de Entorno Requeridas

Para que Umbraco funcione correctamente detrás del proxy reverso de Coolify, necesitas configurar la siguiente variable de entorno en Coolify:

### Variable de Entorno Principal

Tu dominio actual: `http://lsoco8cokk8k8cowks8wog8w.147.93.6.245.sslip.io`

Si quieres sobrescribir el dominio desde Coolify (opcional), agrega esta variable de entorno:

```
PUBLIC_URL=http://lsoco8cokk8k8cowks8wog8w.147.93.6.245.sslip.io
```

⚠️ **Nota:** Estás usando HTTP. Para producción se recomienda usar HTTPS. Si Coolify soporta HTTPS en tu dominio, cambia a `https://` en la URL.

## 📝 Pasos para Configurar en Coolify

1. **Accede a tu aplicación en Coolify**

    - Ve al dashboard de Coolify
    - Selecciona tu aplicación Umbraco

2. **Configura la Variable de Entorno**

    - Ve a la pestaña "Environment Variables"
    - Agrega: `PUBLIC_URL` = `https://tu-dominio-real.com`
    - Guarda los cambios

3. **Re-despliega la Aplicación**

    - Haz clic en "Redeploy" para aplicar los cambios
    - Espera a que el despliegue se complete

4. **Accede al Admin de Umbraco**
    - Ve a: `https://tu-dominio.com/umbraco`
    - El error de OAuth debería estar resuelto

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

**Causa:** La variable `PUBLIC_URL` no está configurada o tiene un valor incorrecto.

**Solución:**

1. Verifica que `PUBLIC_URL` esté configurada en Coolify
2. Asegúrate de que la URL incluya `https://`
3. Verifica que la URL coincida exactamente con tu dominio
4. Re-despliega la aplicación

### Error: "Invalid redirect_uri"

**Causa:** La URL pública no coincide con la configurada en Umbraco.

**Solución:**

1. Limpia la base de datos SQLite (elimina el volumen)
2. Re-despliega con la URL correcta
3. Umbraco se reinstalará con la configuración correcta

## 📚 Referencias

-   [Documentación de Umbraco sobre WebRouting](https://docs.umbraco.com/umbraco-cms/reference/configuration/webroutingsettings)
-   [OpenIddict Error ID2029](https://documentation.openiddict.com/errors/ID2029)
-   [Coolify Documentation](https://coolify.io/docs)
