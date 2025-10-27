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

# Verificar si la base de datos ya existe
if [ -f "$DB_PATH" ]; then
    echo "✅ Base de datos existente encontrada: $DB_PATH"
    echo "   Usando base de datos actual"
else
    echo "⚠️  Base de datos no encontrada en: $DB_PATH"

    # Verificar si existe una seed database
    if [ -f "$SEED_DB_PATH" ]; then
        echo "📦 Copiando base de datos seed..."
        cp "$SEED_DB_PATH" "$DB_PATH"
        echo "✅ Base de datos seed copiada exitosamente"
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
