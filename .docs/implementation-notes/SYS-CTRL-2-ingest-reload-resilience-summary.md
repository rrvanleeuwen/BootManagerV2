# SYS-CTRL-2: Ingest Reload Resilience Implementation Summary

Status: complete on branch `feature/ingest-reload-config-resilience`.
Date: 2026-05-29.

## Problem

Pi validation showed that the operational settings database could contain a development `ApiBaseUrl` such as `http://localhost:5046`. Inside Docker, `localhost` points to the `bootmanager-ingest` container itself, so reload and API posting could fail even though Compose had the correct bootstrap URL `http://bootmanager-web:5000`.

The same Pi session showed that `CaptureLoggingEnabled=false` was returned by BootManager.Web while ingest startup still logged that capture logging was enabled.

## Implemented

- `IngestControlServer` now fetches settings for `POST /reload-settings` from configured/bootstrap `Ingest__ApiBaseUrl` first.
- The mutable runtime `ApiBaseUrl` is used only as fallback when the configured/bootstrap URL fails and differs from runtime.
- Reload logging now states which URL is used and when fallback is attempted.
- `IngestCaptureLogger` now uses the effective condition `Ingest__CaptureLogging__Enabled && runtime CaptureLoggingEnabled`.
- If runtime/database `CaptureLoggingEnabled=false`, no capture file is created and no capture records are written, even when Compose has capture logging technically enabled.
- `BootManager.Tools.Ingest` still has no Infrastructure/database reference; all settings flow through Web API/control-flow.

## Tests

Added:

- `BootManager.UnitTests/IngestTools/IngestCaptureLoggerTests.cs`
- `BootManager.UnitTests/IngestTools/IngestSettingsReloadFallbackTests.cs`

Verified:

- `dotnet build BootManager.sln` passed with 0 warnings and 0 errors.
- `dotnet test BootManager.UnitTests\BootManager.UnitTests.csproj --filter "FullyQualifiedName~IngestTools|FullyQualifiedName~OperationalSettings"` passed 35/35.

## Manual Validation

The user manually tested the change locally and approved the result on 2026-05-29.

After merge to `master`, the Pi should run a short validation:

- update from `master`;
- rebuild/restart containers;
- confirm `GET /api/operationalsettings/ingest` returns `apiBaseUrl=http://bootmanager-web:5000`;
- confirm ingest reload/startup uses `http://bootmanager-web:5000`;
- confirm capture logging stays disabled when database `CaptureLoggingEnabled=false`;
- confirm ingest-processing disabled still skips incoming lines without `POST /api/networkmessages`.
