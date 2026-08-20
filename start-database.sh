#!/usr/bin/env bash
set -a

# Resolve root directory relative to this script's location
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="$SCRIPT_DIR"
BOT_PROJECT="$ROOT_DIR/Finder.Bot"
WEB_PROJECT="$ROOT_DIR/Finder.Web"
DB_PROJECT="$ROOT_DIR/Finder.Db"

if ! grep -q "UserSecretsId" "$BOT_PROJECT"/*.csproj 2>/dev/null; then
  dotnet user-secrets init --project "$BOT_PROJECT" > /dev/null 2>&1
fi

if ! grep -q "UserSecretsId" "$WEB_PROJECT"/*.csproj 2>/dev/null; then
  dotnet user-secrets init --project "$WEB_PROJECT" > /dev/null 2>&1
fi

CONN_STR=$(dotnet user-secrets list --project "$BOT_PROJECT" 2>/dev/null | sed -n 's/^ConnectionStrings:PostgreSQL = //p')

# If missing, try reading from the Web project
if [ -z "$CONN_STR" ]; then
  CONN_STR=$(dotnet user-secrets list --project "$WEB_PROJECT" 2>/dev/null | sed -n 's/^ConnectionStrings:PostgreSQL = //p')
fi

if [ -z "$CONN_STR" ]; then
  echo "No 'ConnectionStrings:PostgreSQL' found in User Secrets. Initializing default..."
  CONN_STR="Host=localhost;Port=5432;Database=finderbot;Username=postgres;Password=myPassword;"
fi

dotnet user-secrets set "ConnectionStrings:PostgreSQL" "$CONN_STR" --project "$BOT_PROJECT" > /dev/null 2>&1
dotnet user-secrets set "ConnectionStrings:PostgreSQL" "$CONN_STR" --project "$WEB_PROJECT" > /dev/null 2>&1

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

if [ "$DB_PASSWORD" = "myPassword" ] || [ -z "$DB_PASSWORD" ]; then
  echo "Default or missing password detected in User Secrets."
  printf "Generate a random secure password and update User Secrets? [Y/n]: "
  read -r REPLY
  REPLY=${REPLY:-Y}
  if [[ $REPLY =~ ^[Yy]$ ]]; then
    NEW_PASSWORD=$(openssl rand -base64 16 | tr -dc 'a-zA-Z0-9')
    if [ -z "$DB_PASSWORD" ]; then
      CONN_STR="${CONN_STR}Password=${NEW_PASSWORD};"
    else
      CONN_STR="${CONN_STR//Password=$DB_PASSWORD/Password=$NEW_PASSWORD}"
    fi
    DB_PASSWORD=$NEW_PASSWORD
    dotnet user-secrets set "ConnectionStrings:PostgreSQL" "$CONN_STR" --project "$BOT_PROJECT" > /dev/null 2>&1
    dotnet user-secrets set "ConnectionStrings:PostgreSQL" "$CONN_STR" --project "$WEB_PROJECT" > /dev/null 2>&1
    echo "Updated .NET User Secrets with new password."
  fi
fi

# Check Docker availability
if ! [ -x "$(command -v docker)" ]; then
  echo "Docker is not installed."
  exit 1
fi

if ! docker info > /dev/null 2>&1; then
  echo "docker daemon is not running. Please start docker."
  exit 1
fi

# Handle container lifecycle without premature exits
if [ "$(docker ps -q -f name=^/"${DB_CONTAINER_NAME}"$)" ]; then
  echo "Database container '$DB_CONTAINER_NAME' is already running."
elif [ "$(docker ps -q -a -f name=^/"${DB_CONTAINER_NAME}"$)" ]; then
  echo "Starting existing container '$DB_CONTAINER_NAME'..."
  docker start "$DB_CONTAINER_NAME" > /dev/null
else
  # Check if host port is in use before creating container
  if command -v nc >/dev/null 2>&1; then
    if nc -z localhost "$DB_PORT" 2>/dev/null; then
      echo "Error: Port $DB_PORT is already in use."
      exit 1
    fi
  fi

  # Spin up PostgreSQL container
  docker run -d \
    --name "$DB_CONTAINER_NAME" \
    -e POSTGRES_USER="$DB_USER" \
    -e POSTGRES_PASSWORD="$DB_PASSWORD" \
    -e POSTGRES_DB="$DB_NAME" \
    -p "$DB_PORT":5432 \
    docker.io/postgres > /dev/null
  echo "Container '$DB_CONTAINER_NAME' created."
fi

# Wait for PostgreSQL readiness
echo "Waiting for PostgreSQL to be ready..."
until docker exec "$DB_CONTAINER_NAME" pg_isready -U "$DB_USER" -d "$DB_NAME" > /dev/null 2>&1; do
  sleep 1
done

# Pass connection string to EF Core tooling dynamically
export ConnectionStrings__PostgreSQL="$CONN_STR"

# Run EF Core migrations
echo "Applying EF Core migrations..."
if dotnet ef --version >/dev/null 2>&1; then
  dotnet ef database update --project "$DB_PROJECT" --startup-project "$BOT_PROJECT" 2>&1 | grep -v "Failed executing DbCommand"
elif [ -f "$ROOT_DIR/.config/dotnet-tools.json" ]; then
  echo "Restoring local dotnet tools..."
  dotnet tool restore
  dotnet ef database update --project "$DB_PROJECT" --startup-project "$BOT_PROJECT" 2>&1 | grep -v "Failed executing DbCommand"
else
  echo "Error: 'dotnet-ef' is not installed or available on PATH."
  echo "Run 'dotnet tool install --global dotnet-ef' to enable automatic database migrations."
  exit 1
fi

echo "Database is online and migrations applied!"