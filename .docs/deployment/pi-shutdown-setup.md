# BootManager Pi Shutdown Configuration Guide

## Overview

The BootManager Web application includes a safe shutdown mechanism that allows authorized Owner users to gracefully shut down the Raspberry Pi from the Admin page (`/analysis`).

This implementation uses a **Unix domain socket** for secure, bounded communication between the Docker Web container and the host system. This approach:
- ✅ Eliminates shell injection risk (no arbitrary commands)
- ✅ Prevents access to host filesystem (only socket communication)
- ✅ Works cleanly with Docker Compose architecture
- ✅ Provides audit logging of shutdown requests

## How It Works

### Architecture

```
┌─ Host Raspberry Pi ─────────────────────────────────────────────┐
│                                                                  │
│  systemd Socket: bootmanager-shutdown.socket                   │
│    - Listens on Unix socket: /run/bootmanager/shutdown.sock    │
│    - Accept=yes: passes each connection to template service    │
│                                                                  │
│  systemd Template Service: bootmanager-shutdown@.service       │
│    - Runs ExecStart=/opt/bootmanager/shutdown-helper.sh        │
│    - Accepts only "SHUTDOWN" command (no arguments)            │
│    - Executes: /sbin/shutdown -h +0 (immediate shutdown)       │
│                                                                  │
│  ┌─ Docker Container: bootmanager-web ─────────────────────┐  │
│  │                                                           │  │
│  │  POST /api/system/shutdown                              │  │
│  │    ↓                                                      │  │
│  │  ShutdownService (IHostEnvironment.IsProduction)        │  │
│  │    ↓                                                      │  │
│  │  ShutdownHelperExecutor                                 │  │
│  │    - Validates socket exists                            │  │
│  │    - Connects to /run/bootmanager/shutdown.sock        │  │
│  │    - Sends "SHUTDOWN\n" (literally, no shell)          │  │
│  │    - Returns control immediately (fire-and-forget)     │  │
│  │                                                           │  │
│  └─────────────────────────────────────────────────────────┘  │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

### User Flow

1. **Web UI**: Owner clicks "BootManager Pi afsluiten" button on `/analysis` (Admin page)
2. **Confirmation**: Modal dialog asks for confirmation
3. **API Call**: Blazor component sends `POST /api/system/shutdown`
4. **Authorization**: Endpoint verifies `Owner` role
5. **Shutdown Logic**:
   - **Development Mode**: Logs warning, no shutdown
   - **Production Mode**: Connects to socket, sends SHUTDOWN command
6. **Response**: API returns 200 OK with warning message: `De BootManager Pi wordt afgesloten. Wacht 20 seconden voordat je de BootManager Pi uitzet.`
7. **Host Shutdown**: systemd socket accepts connection and activates template service instance, which executes `shutdown -h +0`

## Production Deployment: Host-Side Setup

### Prerequisites

- Raspberry Pi running standard **Raspberry Pi OS** (Bullseye or later)
- systemd installed (standard on all modern Pi OS)
- Docker Compose already running BootManager containers
- `sudo` access on the Pi host

### Step 1: Create Shutdown Helper Script

As root or with `sudo`, create `/opt/bootmanager/shutdown-helper.sh`:

```bash
#!/bin/bash
# /opt/bootmanager/shutdown-helper.sh
# BootManager Shutdown Helper Service
#
# This script is run by systemd template service (bootmanager-shutdown@.service)
# per incoming socket connection. It accepts ONLY the "SHUTDOWN" command.

set -e
LOG="/var/log/bootmanager/shutdown-helper.log"
COMMAND=""

# Read the command from stdin (socket connection)
read -r COMMAND

# Sanitize: only accept "SHUTDOWN" (case-insensitive)
COMMAND=$(echo "$COMMAND" | tr -d '\r\n' | tr '[:lower:]' '[:upper:]')

if [ "$COMMAND" = "SHUTDOWN" ]; then
	echo "[$(date +'%Y-%m-%d %H:%M:%S')] Shutdown command received from BootManager Web container" >> "$LOG"
	echo "[$(date +'%Y-%m-%d %H:%M:%S')] Initiating system shutdown..." >> "$LOG"
	/sbin/shutdown -h +0 "BootManager Pi shutdown initiated from Web Admin interface"
	exit 0
else
	echo "[$(date +'%Y-%m-%d %H:%M:%S')] ERROR: Invalid command: '$COMMAND'" >> "$LOG"
	exit 1
fi
```

Make it executable:

```bash
sudo chmod 755 /opt/bootmanager/shutdown-helper.sh
```

### Step 2: Create systemd Socket Unit

Create `/etc/systemd/system/bootmanager-shutdown.socket`:

```ini
[Unit]
Description=BootManager Shutdown Helper Socket
Documentation=file:///opt/bootmanager/docs/pi-shutdown-setup.md

[Socket]
# Unix domain socket for shutdown control
ListenStream=/run/bootmanager/shutdown.sock
SocketMode=0666
RemoveOnStop=yes

# Socket activation: pass each incoming connection to a template service instance
Accept=yes

[Install]
WantedBy=sockets.target
```

### Step 3: Create systemd Template Service

Create `/etc/systemd/system/bootmanager-shutdown@.service`:

```ini
[Unit]
Description=BootManager Shutdown Helper Service %i
Documentation=file:///opt/bootmanager/docs/pi-shutdown-setup.md
After=bootmanager-shutdown.socket

[Service]
ExecStart=/opt/bootmanager/shutdown-helper.sh
StandardInput=socket
StandardOutput=journal
StandardError=journal

# Security hardening
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/run/bootmanager /var/log/bootmanager
```

### Step 4: Enable and Start systemd

```bash
# Reload systemd configuration to load new units
sudo systemctl daemon-reload

# Ensure the helper log directory exists on the host
sudo mkdir -p /var/log/bootmanager

# Enable socket (ensures it starts on boot)
sudo systemctl enable bootmanager-shutdown.socket

# Start socket (template service instances are started automatically per connection)
sudo systemctl start bootmanager-shutdown.socket

# Verify socket is active
sudo systemctl status bootmanager-shutdown.socket

# Confirm the socket file exists and is world-writable
ls -la /run/bootmanager/shutdown.sock
# Expected output: srw-rw-rw- 1 root root 0 ... shutdown.sock
```

**Note**: You do NOT need to manually enable or start the template service (`bootmanager-shutdown@.service`).
The systemd socket activation mechanism automatically starts a new instance for each incoming connection.

**Important**: Do not add `Type=accept`, `Restart=always`, or `RestartSec=...` to the template service.
`Accept=yes` belongs only in the `.socket` unit. Restarting failed socket-activated service instances can keep test connections open and make `nc -U` appear to hang.

### Step 5: Docker Compose Configuration

Ensure `docker-compose.yml` is configured to mount the socket (this should already be set up):

```yaml
services:
  bootmanager-web:
	# ... other config ...
	environment:
	  # ... other env vars ...
	  Shutdown__HelperSocketPath=/run/bootmanager/shutdown.sock
	volumes:
	  # ... other volumes ...
	  # Mount shutdown helper socket (read-only)
	  - /run/bootmanager/shutdown.sock:/run/bootmanager/shutdown.sock:ro
```

### Step 6: Restart Docker Containers

```bash
cd /path/to/bootmanager
docker-compose down
docker-compose up -d

# Verify containers are running
docker-compose ps
docker-compose logs bootmanager-web | tail -20
```

## Testing

### Local Testing (Development Mode)

Development mode is **safe**—it logs the shutdown request but does NOT shut down the machine.

1. Build and run locally:
   ```bash
   dotnet build BootManager.sln
   dotnet run
   ```
2. Log in as Owner
3. Navigate to `/analysis` (Admin page)
4. Click "BootManager Pi afsluiten"
5. Confirm in modal
6. Observe warning message appears on screen
7. Check application logs for:
   ```
   DEVELOPMENT MODE: Shutdown requested but not executed...
   ```
8. **Expected**: Your machine stays on

### Production Testing (Socket Communication)

#### Test 1: Verify Socket Connectivity

From within the Docker container, test socket connection:

```bash
# Enter the running Web container
docker-compose exec bootmanager-web bash

# Inside the container, send SHUTDOWN command to socket
echo "SHUTDOWN" | nc -U /run/bootmanager/shutdown.sock

# Exit container and check host logs for socket-activated service instances
exit
sudo journalctl -u 'bootmanager-shutdown@*' -n 20
```

#### Test 2: Full UI Flow

1. Set environment: `ASPNETCORE_ENVIRONMENT=Production`
2. Deploy to Pi
3. Ensure socket is running: `sudo systemctl status bootmanager-shutdown.socket`
4. Log in as Owner
5. Navigate to `/analysis`
6. Click "BootManager Pi afsluiten"
7. Confirm in modal
8. Observe warning message: `De BootManager Pi wordt afgesloten. Wacht 20 seconden voordat je de BootManager Pi uitzet.`
9. Observe Pi shuts down after ~20 seconds
10. Verify shutdown in logs:
	```bash
	sudo journalctl -u 'bootmanager-shutdown@*' -n 20
	# Should show: Shutdown command received
	# Should show: Initiating system shutdown
	```

## Error Responses & Troubleshooting

### 503 Service Unavailable

**Cause**: Shutdown socket not found or not accessible.

**Solution**:
```bash
# Verify socket is running and enabled
sudo systemctl is-active bootmanager-shutdown.socket
sudo systemctl is-enabled bootmanager-shutdown.socket

# If not active, start it
sudo systemctl start bootmanager-shutdown.socket

# Check socket file exists and is world-writable
ls -la /run/bootmanager/shutdown.sock
stat /run/bootmanager/shutdown.sock

# Check Docker container can see the socket
docker-compose exec bootmanager-web ls -la /run/bootmanager/shutdown.sock

# If socket mount missing, restart containers
docker-compose down
docker-compose up -d
```

### Socket Connection Timeout

**Cause**: Socket service not accepting connections or template service crashed.

**Solution**:
```bash
# Check socket is listening
sudo systemctl status bootmanager-shutdown.socket

# View recent logs
sudo journalctl -u 'bootmanager-shutdown@*' -n 50

# Restart socket (template service instances start automatically)
sudo systemctl restart bootmanager-shutdown.socket
```

### "Connection refused" or "Socket not found"

```bash
# Check socket is enabled and active
sudo systemctl is-enabled bootmanager-shutdown.socket
sudo systemctl is-active bootmanager-shutdown.socket

# If not active, start it
sudo systemctl start bootmanager-shutdown.socket

# Check socket file permissions
stat /run/bootmanager/shutdown.sock
# Should show: Access: (0666/srw-rw-rw-)
```

### Socket Disappears After Reboot

Add the directory to a systemd tmpfiles config to ensure persistence:

```bash
echo 'd /run/bootmanager 0755 root root - -' | sudo tee /etc/tmpfiles.d/bootmanager.conf
sudo systemd-tmpfiles --create /etc/tmpfiles.d/bootmanager.conf
```

Then verify socket persists after next boot:
```bash
sudo systemctl reboot
# After reboot:
ls -la /run/bootmanager/shutdown.sock
```

### Template Service Instances Not Starting

```bash
# View template service logs (shows logs from all instances)
sudo journalctl -u 'bootmanager-shutdown@*' -n 50

# Check script permissions
sudo ls -la /opt/bootmanager/shutdown-helper.sh

# Test script manually
sudo /opt/bootmanager/shutdown-helper.sh < <(echo "SHUTDOWN")

# Check journal for socket activation issues
sudo journalctl -u bootmanager-shutdown.socket -n 50
```

## Security Considerations

✅ **Authorization**: Only `Owner`-role users can trigger shutdown via Web UI
✅ **No Shell Injection**: Socket-based protocol accepts only literal "SHUTDOWN" command
✅ **Bounded Actions**: Service can ONLY shut down the system, no other commands
✅ **Audit Logging**: All shutdown attempts logged with timestamp and context
✅ **Docker Isolation**: Web container cannot access host filesystem or execute arbitrary processes
✅ **Socket Permissions**: Socket world-writable but owned by root, controlled by systemd
✅ **Service Isolation**: Each socket connection runs in an isolated systemd service instance

## Socket Activation Explained

This setup uses **systemd socket activation**:

1. **Socket Unit** (`bootmanager-shutdown.socket`) declares the Unix domain socket
2. **Template Service Unit** (`bootmanager-shutdown@.service`) is instantiated per connection
3. When a client connects, systemd automatically spawns a new service instance (e.g., `bootmanager-shutdown@1.service`)
4. Each instance inherits stdin from the socket connection and runs the helper script
5. The helper script reads and processes the shutdown command
6. The instance exits; the socket remains ready for the next connection

This pattern provides isolation and safety: each shutdown request runs in its own service sandbox.

## References

- [Systemd Socket Activation](https://www.freedesktop.org/wiki/Software/systemd/SocketActivation/)
- [Systemd Service Manual](https://www.freedesktop.org/software/systemd/man/systemd.service.html)
- [Systemd Unit File Syntax](https://www.freedesktop.org/software/systemd/man/systemd.unit.html)
- [Unix Domain Sockets](https://man7.org/linux/man-pages/man7/unix.7.html)
- [Raspberry Pi Documentation](https://www.raspberrypi.com/documentation/)
