#!/usr/bin/env bash
set -euo pipefail

# Usage: ./stop_conflicting_processes.sh [PORT]
# Finds processes listening on a TCP port and kills those that match the project name.

PORT=${1:-5149}
PROJECT_NAME="PF3311.TeleRehab.Api"

echo "Checking for listeners on port $PORT..."
PIDS=$(lsof -ti tcp:${PORT} || true)
if [ -z "$PIDS" ]; then
  echo "No process listening on port $PORT"
  exit 0
fi

for PID in $PIDS; do
  CMDLINE=$(ps -p "$PID" -o args= || true)
  if echo "$CMDLINE" | grep -qF "$PROJECT_NAME"; then
    echo "Killing $PID ($CMDLINE)";
    kill "$PID" || kill -9 "$PID" || true
  else
    echo "Skipping $PID: not matching project name ($CMDLINE)"
  fi
done

echo "Done."
