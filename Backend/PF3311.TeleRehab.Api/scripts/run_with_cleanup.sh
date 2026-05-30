#!/usr/bin/env bash
set -euo pipefail

# Usage: ./run_with_cleanup.sh [PORT]
# Stops conflicting processes (if any) then runs the app.

PORT=${1:-5149}
ROOT_DIR=$(cd "$(dirname "$0")/.." && pwd)
cd "$ROOT_DIR"

echo "Ensuring port $PORT is free..."
bash scripts/stop_conflicting_processes.sh "$PORT"

echo "Starting app..."
dotnet run
