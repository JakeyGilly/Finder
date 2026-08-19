#!/usr/bin/env bash
set -a

if [ ! -f "appsettings.json" ]; then
  echo "Error: Could not find appsettings.json"
  exit 1
fi

# Extract ConnectionStrings.PostgreSQL value
CONN_STR=$(python3 -c "import json; data=json.load(open('appsettings.json')); print(data.get('ConnectionStrings', {}).get('PostgreSQL', ''))" 2>/dev/null)

if [ -z "$CONN_STR" ]; then
  echo "Error: Could not find 'ConnectionStrings:PostgreSQL' in appsettings.json"
  exit 1
fi

# Helper to extract key-value pairs from EF Core connection string format
get_conn_param() {
  local key=$1
  echo "$CONN_STR" | grep -o -i "${key}=[^;]*" | cut -d'=' -f2 | tr -d ' "'
}

DB_USER=$(get_conn_param "Username")
[ -z "$DB_USER" ] && DB_USER=$(get_conn_param "User Id")
[ -z "$DB_USER" ] && DB_USER="postgres"

DB_PASSWORD=$(get_conn_param "Password")
[ -z "$DB_PASSWORD" ] && DB_PASSWORD=$(get_conn_param "Pwd")

DB_PORT=$(get_conn_param "Port")
[ -z "$DB_PORT" ] && DB_PORT="5432"

DB_NAME=$(get_conn_param "Database")
[ -z "$DB_NAME" ] && DB_NAME=$(get_conn_param "Db")
[ -z "$DB_NAME" ] && DB_NAME="finderbot"

DB_CONTAINER_NAME="$DB_NAME-postgres"

# Check Docker
if ! [ -x "$(command -v docker)" ]; then
  echo "Docker is not installed."
  exit 1
fi

if ! docker info > /dev/null 2>&1; then
  echo "docker daemon is not running. Please start docker."
  exit 1
fi

# Check if container is already running or existing
if [ "$(docker ps -q -f name=$DB_CONTAINER_NAME)" ]; then
  echo "Database container '$DB_CONTAINER_NAME' is already running."
  exit 0
fi

if [ "$(docker ps -q -a -f name=$DB_CONTAINER_NAME)" ]; then
  docker start "$DB_CONTAINER_NAME"
  echo "Existing container '$DB_CONTAINER_NAME' started."
  exit 0
fi

# Check if host port is in use
if command -v nc >/dev/null 2>&1; then
  if nc -z localhost "$DB_PORT" 2>/dev/null; then
    echo "Error: Port $DB_PORT is already in use."
    exit 1
  fi
fi

# Auto-generate password if default is detected
if [ "$DB_PASSWORD" = "password" ] || [ "$DB_PASSWORD" = "yourpassword" ]; then
  echo "Default password detected in appsettings.json."
  read -p "Generate a random secure password and update appsettings.json? [Y/n]: " -r REPLY
  REPLY=${REPLY:-Y}
  if [[ $REPLY =~ ^[Yy]$ ]]; then
    NEW_PASSWORD=$(openssl rand -base64 16 | tr -dc 'a-zA-Z0-9')
    if [[ "$(uname)" == "Darwin" ]]; then
      sed -i '' "s#Password=$DB_PASSWORD#Password=$NEW_PASSWORD#g" "appsettings.json"
    else
      sed -i "s#Password=$DB_PASSWORD#Password=$NEW_PASSWORD#g" "appsettings.json"
    fi
    DB_PASSWORD=$NEW_PASSWORD
    echo "Updated appsettings.json with new password."
  fi
fi

# Spin up PostgreSQL container
docker run -d \
  --name "$DB_CONTAINER_NAME" \
  -e POSTGRES_USER="$DB_USER" \
  -e POSTGRES_PASSWORD="$DB_PASSWORD" \
  -e POSTGRES_DB="$DB_NAME" \
  -p "$DB_PORT":5432 \
  postgres:16-alpine && echo "Container '$DB_CONTAINER_NAME' created."

# Wait for PostgreSQL to complete initialization
echo "Waiting for PostgreSQL to ready..."
until docker exec "$DB_CONTAINER_NAME" pg_isready -U "$DB_USER" -d "$DB_NAME" > /dev/null 2>&1; do
  sleep 1
done

# Run EF Core migrations
if command -v dotnet-ef >/dev/null 2>&1 || dotnet tool list -g | grep -q "dotnet-ef"; then
  echo "Applying EF Core migrations..."
  dotnet ef database update --project Finder.Bot
fi

echo "Database is online and migrations applied!"