#!/bin/bash
set -e

# Script de inicialización de la base de datos de Umbraco
# Este script se ejecuta antes de iniciar la aplicación
# Si el volumen está vacío, copia una base de datos seed

DB_PATH="/app/umbraco/Data/Umbraco.sqlite.db"
SEED_DB_PATH="/app/seed-db/Umbraco.sqlite.db"

echo "========================================"
echo "Inicializando base de datos de Umbraco..."
echo "========================================"

# Crear directorio si no existe
mkdir -p /app/umbraco/Data

# Debug: mostrar contenido del directorio Data
echo "📂 Contenido de /app/umbraco/Data:"
ls -lah /app/umbraco/Data/ || echo "   (directorio vacío)"

# Debug: verificar si existe seed database
echo ""
echo "🔍 Verificando seed database:"
if [ -f "$SEED_DB_PATH" ]; then
    echo "   ✅ Seed DB encontrada: $SEED_DB_PATH"
    ls -lh "$SEED_DB_PATH"
else
    echo "   ❌ No se encontró seed database en: $SEED_DB_PATH"
fi

echo ""
# Verificar si se debe forzar la copia de la seed database
if [ "$FORCE_SEED_DB" = "true" ]; then
    echo "🔄 FORCE_SEED_DB=true - Sobrescribiendo base de datos..."
    if [ -f "$SEED_DB_PATH" ]; then
        echo "📦 Copiando base de datos seed (forzado)..."
        cp -f "$SEED_DB_PATH" "$DB_PATH"
        echo "✅ Base de datos seed copiada exitosamente (sobrescrita)"
        ls -lh "$DB_PATH"
    else
        echo "❌ No se encontró seed database para copiar"
    fi
# Verificar si la base de datos ya existe
elif [ -f "$DB_PATH" ]; then
    echo "✅ Base de datos existente encontrada: $DB_PATH"
    ls -lh "$DB_PATH"
    echo "   Usando base de datos actual (no se sobrescribe)"
    echo "   💡 Usa FORCE_SEED_DB=true para sobrescribir"
else
    echo "⚠️  Base de datos no encontrada en: $DB_PATH"

    # Verificar si existe una seed database
    if [ -f "$SEED_DB_PATH" ]; then
        echo "📦 Copiando base de datos seed..."
        cp "$SEED_DB_PATH" "$DB_PATH"
        echo "✅ Base de datos seed copiada exitosamente"
        ls -lh "$DB_PATH"
    else
        echo "ℹ️  No hay seed database disponible"
        echo "   Umbraco creará una nueva base de datos"
    fi
fi

echo "========================================"
echo "Iniciando aplicación Umbraco..."
echo "========================================"

# Ejecutar la aplicación
exec dotnet MyDockerSqliteProject.dll
