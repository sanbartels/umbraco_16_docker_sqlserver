# Troubleshooting: Despliegue en Coolify

## Problema: La BD seed no se copia y las vistas no cargan

### 🔍 Diagnóstico

El despliegue en Coolify es exitoso, pero:
- ❌ La base de datos seed NO se copia
- ❌ Las vistas NO cargan en el sitio
- ❌ El administrador puede tener problemas de autenticación

### 📋 Pasos de diagnóstico

#### 1. Verificar logs del contenedor

En Coolify, ve a tu aplicación Umbraco y revisa los **logs del contenedor** (no del build).

**Logs esperados** si todo funciona:
```
========================================
Inicializando base de datos de Umbraco...
========================================
📂 Contenido de /app/umbraco/Data:
   (directorio vacío)

🔍 Verificando seed database:
   ✅ Seed DB encontrada: /app/seed-db/Umbraco.sqlite.db
-rw-r--r-- 1 root root 980K /app/seed-db/Umbraco.sqlite.db

⚠️  Base de datos no encontrada en: /app/umbraco/Data/Umbraco.sqlite.db
📦 Copiando base de datos seed...
✅ Base de datos seed copiada exitosamente
-rw-r--r-- 1 root root 980K /app/umbraco/Data/Umbraco.sqlite.db
========================================
Iniciando aplicación Umbraco...
========================================
```

**Logs que indican problema**:

**Caso A: El script NO se ejecuta**
```
[18:57:16 INF] Acquiring MainDom.
[18:57:16 INF] Acquired MainDom.
```
→ El script `init-db.sh` NO está ejecutándose como ENTRYPOINT

**Caso B: El volumen ya tiene una BD antigua**
```
✅ Base de datos existente encontrada: /app/umbraco/Data/Umbraco.sqlite.db
-rw-r--r-- 1 root root 2.5M /app/umbraco/Data/Umbraco.sqlite.db
   Usando base de datos actual (no se sobrescribe)
```
→ El volumen `umbraco-data` ya existe con una BD antigua

#### 2. Soluciones según el caso

### **Caso A: init-db.sh NO se ejecuta**

Si NO ves los mensajes del script en los logs, significa que Coolify está ignorando el ENTRYPOINT del Dockerfile.

**Solución**: Agregar `entrypoint` explícitamente en `docker-compose.yml`

```yaml
services:
    umbraco:
        build:
            context: .
            dockerfile: Dockerfile
        entrypoint: ["/app/init-db.sh"]  # ← AGREGAR ESTA LÍNEA
        expose:
            - 8080
        environment:
            # ... resto de variables
```

### **Caso B: Volumen con BD antigua**

Si ves que la BD ya existe y tiene un tamaño diferente a 980K, significa que hay una BD antigua en el volumen.

**Opciones de solución**:

#### Opción 1: Eliminar el volumen en Coolify (DESTRUCTIVO)

⚠️ **ADVERTENCIA**: Esto eliminará TODOS los datos actuales.

1. En Coolify, ve a tu aplicación
2. Ve a la sección "Storages" o "Volumes"
3. Elimina el volumen `umbraco-data`
4. Haz un nuevo despliegue

#### Opción 2: Forzar sobrescritura de la BD (DESTRUCTIVO)

Modificar `init-db.sh` para SIEMPRE copiar la seed database:

```bash
# En vez de:
if [ -f "$DB_PATH" ]; then
    echo "✅ Base de datos existente encontrada"
    echo "   Usando base de datos actual"
else
    # copiar seed
fi

# Usar:
if [ -f "$SEED_DB_PATH" ]; then
    echo "📦 Copiando base de datos seed (sobrescribiendo)..."
    cp -f "$SEED_DB_PATH" "$DB_PATH"
    echo "✅ Base de datos seed copiada exitosamente"
fi
```

⚠️ **CUIDADO**: Esto sobrescribirá la BD en CADA despliegue.

#### Opción 3: Variable de entorno para controlar la copia (RECOMENDADO)

Agregar una variable de entorno `FORCE_SEED_DB` que controle si se sobrescribe:

```bash
# En init-db.sh
if [ "$FORCE_SEED_DB" = "true" ]; then
    echo "🔄 FORCE_SEED_DB=true - Sobrescribiendo BD..."
    cp -f "$SEED_DB_PATH" "$DB_PATH"
elif [ -f "$DB_PATH" ]; then
    echo "✅ BD existente - no se sobrescribe"
else
    echo "📦 Copiando seed BD (primera vez)..."
    cp "$SEED_DB_PATH" "$DB_PATH"
fi
```

Luego en Coolify, agregar la variable solo cuando necesites resetear:
- `FORCE_SEED_DB=true` → Sobrescribe la BD
- Sin la variable o `false` → Respeta la BD existente

### 🎯 Verificación final

Después de aplicar la solución:

1. Haz un nuevo despliegue en Coolify
2. Verifica los **logs del contenedor** (no del build)
3. Deberías ver los mensajes del script `init-db.sh`
4. Accede a `https://tu-dominio.com` → Debe cargar las vistas
5. Accede a `https://tu-dominio.com/umbraco` → Debe permitir login

### 📊 Logs de referencia

**Build exitoso** (esto ya funciona):
```
#17 [final 3/5] COPY init-db.sh /app/init-db.sh
#17 DONE 0.1s

#18 [final 4/5] COPY seed-db/ /app/seed-db/
#18 DONE 0.1s

#19 [final 5/5] RUN chmod +x /app/init-db.sh
#19 DONE 0.7s
```

**Contenedor corriendo** (esto es lo que necesitas verificar):
```
Container umbraco-iwgk8wo80skcw8goos04kwsw-004656737941  Started
```

Luego en **logs del contenedor** debes ver:
```
========================================
Inicializando base de datos de Umbraco...
========================================
```

### 🚀 Desarrollo local

El desarrollo local NO se ve afectado porque:
- `docker-compose.override.yml` tiene configuración específica para local
- El archivo override NO está versionado en Git
- Coolify solo ve el `docker-compose.yml` principal

Para trabajar en local:
```bash
docker compose down -v  # Eliminar volúmenes si necesitas reset
docker compose up --build
```

Accede a: http://localhost:8080
