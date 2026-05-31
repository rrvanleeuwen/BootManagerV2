#!/bin/bash
# /opt/bootmanager/shutdown-helper.sh
# BootManager Shutdown Helper Service
#
# This script is run by systemd template service (bootmanager-shutdown@.service)
# It listens for SHUTDOWN command on a Unix domain socket and executes system shutdown.
#
# SECURITY:
# - Only accepts "SHUTDOWN" command (no arguments, no shell injection)
# - No configuration, no command-line parameters
# - Single purpose: safely shut down the Raspberry Pi
# - Socket is read-only mounted into Docker container

set -e

LOG="/var/log/bootmanager/shutdown-helper.log"
COMMAND=""

# Read the command from the socket (stdin)
# systemd accepts the connection and passes it as stdin to this script
read -r COMMAND < /dev/stdin

# Sanitize: only accept "SHUTDOWN" command (case-insensitive)
COMMAND=$(echo "$COMMAND" | tr -d '\r\n' | tr '[:lower:]' '[:upper:]')

if [ "$COMMAND" = "SHUTDOWN" ]; then
	echo "[$(date +'%Y-%m-%d %H:%M:%S')] Shutdown command received from BootManager Web container" >> "$LOG"
	echo "[$(date +'%Y-%m-%d %H:%M:%S')] Initiating system shutdown..." >> "$LOG"

	# Execute system shutdown immediately (no delay)
	# The Web API already returned a 20-second warning message to the user
	/sbin/shutdown -h +0 "BootManager Pi shutdown initiated from Web Admin interface"

	EXIT_CODE=0
else
	echo "[$(date +'%Y-%m-%d %H:%M:%S')] ERROR: Invalid command received: '$COMMAND'" >> "$LOG"
	echo "ERROR: Invalid command" >&2
	EXIT_CODE=1
fi

exit $EXIT_CODE
