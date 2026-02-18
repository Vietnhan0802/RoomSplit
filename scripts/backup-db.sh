#!/bin/bash
set -euo pipefail

BACKUP_DIR=/opt/roomsplit/backups
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

mkdir -p "$BACKUP_DIR"

docker exec roomsplit-db pg_dump -U postgres roomsplit | gzip > "$BACKUP_DIR/roomsplit_$TIMESTAMP.sql.gz"

# Keep last 30 days
find "$BACKUP_DIR" -name "*.sql.gz" -mtime +30 -delete

echo "Backup completed: roomsplit_$TIMESTAMP.sql.gz"
