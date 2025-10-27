# 💾 Sistema de Base de Datos Seed

Este proyecto usa un sistema de **base de datos seed** para resolver el problema de persistencia de datos en despliegues Docker/Coolify.

## 🎯 Problema que Resuelve

**Antes**:
- Cada despliegue creaba una base de datos nueva y vacía
- Se perdían todos los datos: contenido, usuarios, configuración
- Las vistas no se copiaban correctamente

**Ahora**:
- La base de datos se preserva entre despliegues usando volúmenes
- Si el volumen está vacío (primer despliegue), se copia automáticamente una seed database
- Las vistas se incluyen en la imagen Docker

## 📂 Estructura de Archivos

```
MyDockerSqliteProject/
├── seed-db/
│   └── Umbraco.sqlite.db        # Base de datos seed (incluida en la imagen)
├── umbraco/Data/
│   └── Umbraco.sqlite.db        # Base de datos local (NO se incluye en Docker)
├── Views/                        # Views de Umbraco (incluidas en la imagen)
├── init-db.sh                    # Script de inicialización
└── Dockerfile                    # Configurado para copiar seed-db y script
```

## 🔄 Flujo de Funcionamiento

### 1. Desarrollo Local
```bash
# Trabaja normalmente en tu máquina
dotnet run

# Tu base de datos está en: umbraco/Data/Umbraco.sqlite.db
# Esta NO se copia a Docker (está en .dockerignore)
```

### 2. Preparar para Despliegue
```bash
# Cuando hagas cambios importantes (nuevos content types, usuarios, etc.):
cp umbraco/Data/Umbraco.sqlite.db seed-db/Umbraco.sqlite.db

# Commit y push
git add seed-db/Umbraco.sqlite.db
git commit -m "feat: actualizar seed database"
git push
```

### 3. Primer Despliegue en Coolify
```
1. Docker build copia seed-db/ a la imagen
2. Coolify crea un volumen vacío: umbraco-data
3. init-db.sh se ejecuta:
   ❓ ¿Existe /app/umbraco/Data/Umbraco.sqlite.db?
   ❌ NO → Copiar seed database al volumen
   ✅ Contenido preservado
```

### 4. Despliegues Posteriores
```
1. Docker build copia nueva versión de seed-db/ (si cambió)
2. Coolify usa el volumen existente: umbraco-data
3. init-db.sh se ejecuta:
   ❓ ¿Existe /app/umbraco/Data/Umbraco.sqlite.db?
   ✅ SÍ → Usar base de datos existente
   ⚠️ Seed database NO se copia (la existente prevalece)
```

## 🛠️ Comandos Útiles

### Actualizar Seed Database
```bash
# Copiar tu base de datos actual a seed-db
cp umbraco/Data/Umbraco.sqlite.db seed-db/Umbraco.sqlite.db
```

### Verificar Seed Database
```bash
# Ver tamaño de la seed database
ls -lh seed-db/Umbraco.sqlite.db

# Verificar que se incluye en Docker
docker build -t test . && docker run --rm test ls -lh /app/seed-db/
```

### Empezar de Cero en Coolify
```bash
# En Coolify:
# 1. Ve a "Storage"
# 2. Elimina el volumen "umbraco-data"
# 3. Re-despliega
# 4. Se copiará la seed database nuevamente
```

## 📋 Casos de Uso

### Caso 1: Agregar Nuevo Content Type
```bash
# 1. Crea el content type en tu Umbraco local
# 2. Actualiza la seed database:
cp umbraco/Data/Umbraco.sqlite.db seed-db/Umbraco.sqlite.db

# 3. Commit y push:
git add seed-db/Umbraco.sqlite.db
git commit -m "feat: agregar content type Producto"
git push

# 4. En Coolify, elimina el volumen umbraco-data y re-despliega
#    (Solo si quieres que el nuevo content type se aplique)
```

### Caso 2: Actualizar Views
```bash
# 1. Modifica tus vistas en Views/
# 2. Commit y push:
git add Views/
git commit -m "feat: actualizar vista home"
git push

# 3. Re-despliega en Coolify
#    (NO necesitas eliminar volúmenes)
#    Las vistas se actualizan automáticamente
```

### Caso 3: Desarrollo Continuo (SIN eliminar datos)
```bash
# Si NO quieres perder datos en Coolify:
# - NO actualices la seed database
# - NO elimines el volumen umbraco-data
# - Solo haz cambios en el código (Views, Models, etc.)
# - Re-despliega normalmente

# El contenido en Coolify se mantiene entre despliegues
```

## ⚠️ Advertencias Importantes

1. **NO commits la base de datos de desarrollo**:
   - `umbraco/Data/` está en `.gitignore` y `.dockerignore`
   - Solo commits `seed-db/Umbraco.sqlite.db`

2. **Seed database es un snapshot**:
   - Representa el estado inicial del sitio
   - NO se actualiza automáticamente con cambios en Coolify
   - Si creas contenido en Coolify, NO está en la seed database

3. **Eliminar volumen = perder datos**:
   - Si eliminas `umbraco-data` en Coolify, pierdes todo el contenido creado ahí
   - Solo tendrás lo que esté en la seed database

4. **Sincronización manual**:
   - Si quieres traer datos de Coolify a tu seed database:
   - Debes descargar la base de datos del volumen manualmente
   - O usar herramientas de backup/restore

## 🎓 Mejores Prácticas

1. **Mantén la seed database actualizada con estructura**:
   - Content types
   - Data types
   - Templates
   - Usuarios administrativos

2. **NO incluyas contenido temporal en seed**:
   - Posts de blog de prueba
   - Páginas de testing
   - Media de desarrollo

3. **Usa branches para cambios grandes**:
   - Crea un branch para cambios de estructura
   - Prueba localmente
   - Actualiza seed database
   - Merge y despliega

4. **Backups regulares**:
   - Exporta tu base de datos de Coolify regularmente
   - Guarda backups fuera del repositorio Git

## 🔍 Troubleshooting

### Las Views no se actualizan en Coolify
**Causa**: Las views están en caché o no se copiaron correctamente.

**Solución**:
```bash
# Verificar que las views se copian en el build:
docker build -t test . && docker run --rm test ls -la /app/Views/

# Debe mostrar tus archivos .cshtml
```

### La base de datos siempre está vacía
**Causa**: El script init-db.sh no se está ejecutando o falla.

**Solución**:
```bash
# Ver logs de Coolify para verificar la salida del script
# Debería mostrar:
# ========================================
# Inicializando base de datos de Umbraco...
# ========================================
```

### Quiero reemplazar la base de datos en Coolify
**Solución**:
```bash
# 1. Actualiza seed database localmente
cp umbraco/Data/Umbraco.sqlite.db seed-db/Umbraco.sqlite.db

# 2. Commit y push
git add seed-db/Umbraco.sqlite.db
git commit -m "feat: nueva seed database"
git push

# 3. En Coolify:
#    - Elimina el volumen umbraco-data
#    - Re-despliega
```
