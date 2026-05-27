#!/bin/bash
#
# reset-database.sh: Controlled database reset for Raspberry Pi Docker Compose BootManager installation
#
# Purpose: Reset the BootManager SQLite database to a clean state without destroying
#          Docker volumes, .env, Git checkout, attachments, or capture logs.
#
# Usage:   cd ~/BootManagerV2 && bash scripts/reset-database.sh
#
# WARNING: This script will disconnect the running application from its current database.
#          The active database will be backed up with a timestamp before removal.
#          After reset, BootManager will apply migrations and require new onboarding.
#
# Safety:  - Only the active SQLite database is replaced.
#          - .env, Git repo, Docker images, attachments, capture logs remain untouched.
#          - Containers are stopped cleanly before database manipulation.
#          - Containers are restarted after reset.
#

set -euo pipefail

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# Utility functions
log_info() {
	echo -e "${GREEN}[INFO]${NC} $1" >&2
}

log_warn() {
	echo -e "${YELLOW}[WARN]${NC} $1" >&2
}

log_error() {
	echo -e "${RED}[ERROR]${NC} $1" >&2
}

# Check prerequisites
check_prerequisites() {
	if ! command -v docker &> /dev/null; then
		log_error "Docker is not installed or not in PATH."
		exit 1
	fi

	if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
		log_error "Docker Compose is not installed or not in PATH."
		exit 1
	fi

	if [[ ! -f "docker-compose.yml" ]]; then
		log_error "docker-compose.yml not found. Please run this script from the BootManager repository root."
		exit 1
	fi

	log_info "Prerequisites OK: Docker and Docker Compose available."
}

# Display warning and wait for confirmation
confirm_reset() {
	echo ""
	echo "========================================================================"
	log_warn "DATABASE RESET WARNING"
	echo "========================================================================"
	echo ""
	echo "This script will:"
	echo "  1. Stop BootManager containers cleanly."
	echo "  2. Back up the current SQLite database with a timestamp."
	echo "  3. Remove the active database file."
	echo "  4. Restart containers."
	echo "  5. BootManager will apply migrations and recreate the bootstrap owner."
	echo ""
	echo "The following WILL be preserved:"
	echo "  - .env configuration file"
	echo "  - Git repository state (master branch)"
	echo "  - Docker images and build cache"
	echo "  - Logbook attachments"
	echo "  - Capture logs and application logs"
	echo "  - All Docker volumes except the database content"
	echo ""
	echo "The following WILL be reset:"
	echo "  - Active SQLite database (backed up with timestamp)"
	echo ""
	echo "After reset:"
	echo "  - Login with BOOTMANAGER_BOOTSTRAP_PASSWORD will work."
	echo "  - First login will force onboarding flow."
	echo "  - After onboarding, bootstrap password no longer works."
	echo ""
	echo "========================================================================"
	echo ""
	read -p "Type 'yes' to proceed with database reset: " -r CONFIRM
	echo ""

	if [[ "$CONFIRM" != "yes" ]]; then
		log_info "Reset cancelled by user."
		exit 0
	fi

	log_warn "Proceeding with database reset..."
}

# Get database file path from docker-compose.yml volume
get_database_path() {
	# The volume mount in docker-compose.yml defines bootmanager-db:/var/lib/bootmanager
	# Docker Compose prefixes the volume name with the project name, e.g., bootmanagev2_bootmanager-db
	# 
	# Strategy:
	# 1. Get the actual Compose project name from 'docker compose config'
	# 2. Construct the expected volume name: <project>_bootmanager-db
	# 3. Verify it exists and get its mountpoint
	# 4. Output only the path to stdout (no log lines)

	# Step 1: Get docker compose project name
	local PROJECT_NAME
	PROJECT_NAME=$(docker compose config -q >/dev/null 2>&1 && \
		docker compose config 2>/dev/null | grep -m1 "name:" | sed 's/.*name: //' | xargs)

	if [[ -z "$PROJECT_NAME" ]]; then
		# Fallback: use directory name as project name (Docker Compose default behavior)
		PROJECT_NAME=$(basename "$(pwd)" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z0-9')
	fi

	# Step 2: Construct the expected volume name
	local EXPECTED_VOLUME_NAME="${PROJECT_NAME}_bootmanager-db"

	# Step 3: Verify volume exists
	if ! docker volume inspect "$EXPECTED_VOLUME_NAME" >/dev/null 2>&1; then
		log_error "Docker volume '$EXPECTED_VOLUME_NAME' not found."
		log_error "Is Docker Compose running? Try: docker compose up -d"
		exit 1
	fi

	# Step 4: Get the mountpoint for the volume
	local VOLUME_PATH
	VOLUME_PATH=$(docker volume inspect "$EXPECTED_VOLUME_NAME" -f '{{.Mountpoint}}' 2>/dev/null)

	if [[ -z "$VOLUME_PATH" ]] || [[ ! -d "$VOLUME_PATH" ]]; then
		log_error "Could not determine mountpoint for Docker volume '$EXPECTED_VOLUME_NAME'."
		exit 1
	fi

	# Log (to stderr, not captured)
	log_info "Using Docker volume: $EXPECTED_VOLUME_NAME at $VOLUME_PATH"

	# Output only the path to stdout (no log lines)
	echo "$VOLUME_PATH"
}

# Stop containers
stop_containers() {
	log_info "Stopping containers..."

	if ! docker compose stop; then
		log_error "Failed to stop containers."
		exit 1
	fi

	log_info "Containers stopped successfully."
	sleep 2  # Give filesystem time to sync
}

# Backup and remove database
backup_and_reset_database() {
	local DB_PATH="$1"
	local DB_FILE="$DB_PATH/bootmanager.db"

	if [[ ! -f "$DB_FILE" ]]; then
		log_warn "Database file not found at $DB_FILE. Skipping backup (fresh installation?)."
		return
	fi

	# Create backup with timestamp
	local TIMESTAMP
	TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
	local BACKUP_FILE="$DB_PATH/bootmanager.db.backup.$TIMESTAMP"

	log_info "Backing up database to: $BACKUP_FILE"

	if ! cp "$DB_FILE" "$BACKUP_FILE"; then
		log_error "Failed to backup database."
		exit 1
	fi

	if [[ ! -f "$BACKUP_FILE" ]]; then
		log_error "Backup file was not created successfully."
		exit 1
	fi

	log_info "Backup created successfully. Size: $(du -h "$BACKUP_FILE" | cut -f1)"

	# Remove the active database
	log_info "Removing active database file..."

	if ! rm "$DB_FILE"; then
		log_error "Failed to remove database file. Backup exists at: $BACKUP_FILE"
		exit 1
	fi

	log_info "Active database file removed."
	log_info "Database will be recreated on next application start."
}

# Start containers
start_containers() {
	log_info "Starting containers..."

	if ! docker compose up -d; then
		log_error "Failed to start containers."
		exit 1
	fi

	log_info "Containers started. Waiting for health check..."
	sleep 5

	# Wait for health check (up to 60 seconds)
	local RETRY_COUNT=0
	local MAX_RETRIES=12

	while [[ $RETRY_COUNT -lt $MAX_RETRIES ]]; do
		if docker compose exec -T bootmanager-web curl -fsS http://localhost:5000/health &> /dev/null; then
			log_info "Health check passed. Application is ready."
			return 0
		fi

		log_info "Waiting for health check... ($((RETRY_COUNT + 1))/$MAX_RETRIES)"
		sleep 5
		((RETRY_COUNT++))
	done

	log_warn "Health check did not pass within timeout. Containers may still be starting up."
	log_warn "Run 'curl -i http://localhost:5000/health' manually to verify status."
}

# Display final status
display_status() {
	echo ""
	echo "========================================================================"
	log_info "DATABASE RESET COMPLETE"
	echo "========================================================================"
	echo ""
	log_info "Container status:"
	docker compose ps
	echo ""
	echo "Next steps:"
	echo "  1. Access the application at http://localhost:5000 (or http://<pi-hostname>:5000)"
	echo "  2. Log in with BOOTMANAGER_BOOTSTRAP_PASSWORD"
	echo "  3. Complete the onboarding flow"
	echo "  4. Set a new password during onboarding"
	echo "  5. Bootstrap password will no longer work after onboarding completes"
	echo ""
	echo "To verify health check manually:"
	echo "  curl -i http://localhost:5000/health"
	echo ""
	echo "To view backed up database files:"
	local DB_PATH
	DB_PATH=$(get_database_path)
	echo "  ls -lh $DB_PATH/bootmanager.db*"
	echo ""
	echo "========================================================================"
}

# Main flow
main() {
	log_info "BootManager Database Reset Tool"
	log_info "Repository: BootManagerV2 on Raspberry Pi"
	log_info ""

	check_prerequisites
	confirm_reset

	local DB_PATH
	DB_PATH=$(get_database_path)

	stop_containers
	backup_and_reset_database "$DB_PATH"
	start_containers
	display_status

	log_info "Reset operation completed successfully."
}

main "$@"
