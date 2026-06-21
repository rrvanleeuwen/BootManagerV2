param(
    [string]$ReadmePath = "README.md",
    [string]$LegacyCoveragePath = ".docs/legacy-analysis/legacy-coverage-register.md",
    [string]$HolidayPilotPath = ".docs/releases/holiday-pilot-2026.md"
)

$ErrorActionPreference = "Stop"

function New-StatusBar {
    param(
        [double]$Percent,
        [int]$Width = 20
    )

    $bounded = [Math]::Max(0, [Math]::Min(100, $Percent))
    $filled = [int][Math]::Round(($bounded / 100) * $Width)
    $empty = $Width - $filled
    return "[" + ("#" * $filled) + ("-" * $empty) + "]"
}

function Format-Percent {
    param([double]$Value)

    if ([Math]::Abs($Value - [Math]::Round($Value)) -lt 0.05) {
        return ([Math]::Round($Value)).ToString("0", [Globalization.CultureInfo]::InvariantCulture) + "%"
    }

    return $Value.ToString("0.0", [Globalization.CultureInfo]::InvariantCulture) + "%"
}

function Get-Progress {
    param(
        [int]$Done,
        [int]$Replaced,
        [int]$Partial,
        [int]$Open,
        [int]$Parked,
        [int]$Obsolete
    )

    $active = $Done + $Replaced + $Partial + $Open
    if ($active -eq 0) {
        return 0
    }

    return [Math]::Round((($Done + $Replaced + ($Partial * 0.5)) / $active) * 100, 1)
}

function Get-LegacyEpicStatus {
    param([string]$Path)

    $rows = @()
    $currentEpic = $null

    foreach ($line in Get-Content $Path) {
        if ($line -match '^## Epic (\d+):\s*(.+)$') {
            $currentEpic = "Epic $($matches[1]): $($matches[2])"
            continue
        }

        if ($line -match '^\|\s*(US\d+\.\d+)\s+([^|]+?)\s*\|\s*(Done|Partial|Open|Parked|Replaced|Obsolete)\s*\|') {
            $rows += [pscustomobject]@{
                Epic = $currentEpic
                Status = $matches[3]
            }
        }
    }

    $rows |
        Group-Object Epic |
        Sort-Object {
            if ($_.Name -match '^Epic (\d+):') { [int]$matches[1] } else { 999 }
        } |
        ForEach-Object {
            $group = $_.Group
            $done = @($group | Where-Object Status -eq "Done").Count
            $replaced = @($group | Where-Object Status -eq "Replaced").Count
            $partial = @($group | Where-Object Status -eq "Partial").Count
            $open = @($group | Where-Object Status -eq "Open").Count
            $parked = @($group | Where-Object Status -eq "Parked").Count
            $obsolete = @($group | Where-Object Status -eq "Obsolete").Count
            $progress = Get-Progress -Done $done -Replaced $replaced -Partial $partial -Open $open -Parked $parked -Obsolete $obsolete

            [pscustomobject]@{
                Epic = $_.Name
                Done = $done
                Replaced = $replaced
                Partial = $partial
                Open = $open
                Parked = $parked
                Obsolete = $obsolete
                Active = $done + $replaced + $partial + $open
                Progress = $progress
            }
        }
}

function Get-CurrentEpicStatus {
    # BootManagerV2 epic documents are not yet uniformly structured enough for
    # reliable automatic parsing. Keep this small dataset explicit and update it
    # whenever an epic/story is administratively completed.
    $items = @(
        [pscustomobject]@{
            Epic = "First-run onboarding & authenticatie"
            Document = ".docs/epics/first-run-onboarding.md"
            Done = 7
            Replaced = 0
            Partial = 0
            Open = 0
            Parked = 0
            Notes = "Kernflow afgerond"
        },
        [pscustomobject]@{
            Epic = "Owner profile & vessel settings"
            Document = ".docs/epics/owner-profile-settings.md"
            Done = 5
            Replaced = 0
            Partial = 0
            Open = 0
            Parked = 0
            Notes = "Settings-basis en actuele tellerstanden afgerond"
        },
        [pscustomobject]@{
            Epic = "NMEA ingest & sensordata"
            Document = ".docs/epics/nmea0183-support.md"
            Done = 14
            Replaced = 0
            Partial = 2
            Open = 3
            Parked = 1
            Notes = "Basis, simulator, Pi-analyse en tankniveau; bronvoorkeuren open"
        },
        [pscustomobject]@{
            Epic = "Digitaal logboek"
            Document = ".docs/epics/digital-logbook.md"
            Done = 9
            Replaced = 0
            Partial = 5
            Open = 1
            Parked = 0
            Notes = "Basis en tellerstandvoorinvulling klaar; routekaart en export open"
        },
        [pscustomobject]@{
            Epic = "Dashboard & live overzicht"
            Document = ".docs/epics/dashboard-overview.md"
            Done = 2
            Replaced = 0
            Partial = 0
            Open = 2
            Parked = 1
            Notes = "Live meters en configureerbare tegels klaar; widgets/push open"
        },
        [pscustomobject]@{
            Epic = "Meetweergave & eenheidsvoorkeuren"
            Document = ".docs/epics/measurement-unit-preferences.md"
            Done = 0
            Replaced = 0
            Partial = 0
            Open = 2
            Parked = 0
            Notes = "Gebruikerskeuze voor nautische eenheden en consistente weergave open"
        },
        [pscustomobject]@{
            Epic = "System operations & recovery"
            Document = ".docs/epics/system-operations.md"
            Done = 7
            Replaced = 0
            Partial = 0
            Open = 7
            Parked = 1
            Notes = "Reset, Pi analyse/control/shutdown klaar; backup/diagnostics open"
        }
    )

    foreach ($item in $items) {
        $progress = Get-Progress -Done $item.Done -Replaced $item.Replaced -Partial $item.Partial -Open $item.Open -Parked $item.Parked -Obsolete 0
        [pscustomobject]@{
            Epic = $item.Epic
            Document = $item.Document
            Done = $item.Done
            Partial = $item.Partial
            Open = $item.Open
            Parked = $item.Parked
            Active = $item.Done + $item.Replaced + $item.Partial + $item.Open
            Progress = $progress
            Notes = $item.Notes
        }
    }
}

function Get-HolidayPilotStatus {
    param([string]$Document)

    # De pilotstories hebben een vaste prioriteitsvolgorde in het releasedocument.
    # Houd deze tellers bij wanneer een pilotstory administratief van status wijzigt.
    $done = 13
    $partial = 0
    $open = 8
    $parked = 0
    $progress = Get-Progress -Done $done -Replaced 0 -Partial $partial -Open $open -Parked $parked -Obsolete 0

    return [pscustomobject]@{
        Name = "Vakantiepilot 2026"
        Document = $Document
        Done = $done
        Partial = $partial
        Open = $open
        Parked = $parked
        Active = $done + $partial + $open
        Progress = $progress
        Next = "PILOT-SCAN-04 - Locatiegerichte scanmodus"
    }
}

function New-MarkdownTable {
    param(
        [object[]]$Rows,
        [switch]$Current
    )

    $output = @()

    if ($Current) {
        $output += "| Epic | Voortgang | Done | Partial | Open | Parked | Bron | Notitie |"
        $output += "|---|---:|---:|---:|---:|---:|---|---|"

        foreach ($row in $Rows) {
            $bar = New-StatusBar -Percent $row.Progress
            $pct = Format-Percent $row.Progress
            $output += "| $($row.Epic) | ``$bar`` $pct | $($row.Done) | $($row.Partial) | $($row.Open) | $($row.Parked) | [$($row.Document)]($($row.Document)) | $($row.Notes) |"
        }
    }
    else {
        $output += "| Legacy epic | Voortgang | Done | Replaced | Partial | Open | Parked |"
        $output += "|---|---:|---:|---:|---:|---:|---:|"

        foreach ($row in $Rows) {
            $bar = New-StatusBar -Percent $row.Progress
            $pct = Format-Percent $row.Progress
            $output += "| $($row.Epic) | ``$bar`` $pct | $($row.Done) | $($row.Replaced) | $($row.Partial) | $($row.Open) | $($row.Parked) |"
        }
    }

    return $output
}

$legacyRows = @(Get-LegacyEpicStatus -Path $LegacyCoveragePath)
$currentRows = @(Get-CurrentEpicStatus)
$holidayPilot = Get-HolidayPilotStatus -Document $HolidayPilotPath

$legacyOverallActive = ($legacyRows | Measure-Object Active -Sum).Sum
$legacyOverallScore = (($legacyRows | ForEach-Object { ($_.Progress / 100) * $_.Active }) | Measure-Object -Sum).Sum
$legacyOverall = if ($legacyOverallActive -gt 0) { [Math]::Round(($legacyOverallScore / $legacyOverallActive) * 100, 1) } else { 0 }

$currentOverallActive = ($currentRows | Measure-Object Active -Sum).Sum
$currentOverallScore = (($currentRows | ForEach-Object { ($_.Progress / 100) * $_.Active }) | Measure-Object -Sum).Sum
$currentOverall = if ($currentOverallActive -gt 0) { [Math]::Round(($currentOverallScore / $currentOverallActive) * 100, 1) } else { 0 }

$generatedAt = (Get-Date).ToString("yyyy-MM-dd")

$statusBlock = @(
    "<!-- PROJECT-STATUS:START -->",
    "## Projectstatus",
    "",
    "_Laatst bijgewerkt: $generatedAt. Gegenereerd met ``scripts/update-readme-status.ps1``._",
    "",
    "De percentages zijn voortgangsindicatoren, geen harde planning. Berekening: ``Done`` en ``Replaced`` tellen als 100%, ``Partial`` telt als 50%, ``Open`` telt als 0%. ``Parked`` en ``Obsolete`` tellen niet mee in de actieve scope.",
    "",
    "Legacy-percentages worden automatisch berekend uit ``.docs/legacy-analysis/legacy-coverage-register.md``. BootManagerV2-epicpercentages en de vakantiepilot worden expliciet onderhouden in het generator-script, omdat de bron-documenten nog niet overal dezelfde statusstructuur hebben.",
    "",
    "### Samenvatting",
    "",
    "| Scope | Voortgang | Actieve items |",
    "|---|---:|---:|",
    "| Vakantiepilot 2026 | ``$(New-StatusBar -Percent $holidayPilot.Progress)`` $(Format-Percent $holidayPilot.Progress) | $($holidayPilot.Active) |",
    "| BootManagerV2 huidige epics | ``$(New-StatusBar -Percent $currentOverall)`` $(Format-Percent $currentOverall) | $currentOverallActive |",
    "| Legacy scope | ``$(New-StatusBar -Percent $legacyOverall)`` $(Format-Percent $legacyOverall) | $legacyOverallActive |",
    "",
    "### Vakantiepilot 2026",
    "",
    "| Voortgang | Done | Partial | Open | Parked | Bron | Eerstvolgende story |",
    "|---:|---:|---:|---:|---:|---|---|",
    "| ``$(New-StatusBar -Percent $holidayPilot.Progress)`` $(Format-Percent $holidayPilot.Progress) | $($holidayPilot.Done) | $($holidayPilot.Partial) | $($holidayPilot.Open) | $($holidayPilot.Parked) | [$($holidayPilot.Document)]($($holidayPilot.Document)) | $($holidayPilot.Next) |",
    "",
    "### BootManagerV2 Epics",
    "",
    (New-MarkdownTable -Rows $currentRows -Current),
    "",
    "### Legacy Epics",
    "",
    (New-MarkdownTable -Rows $legacyRows),
    "",
    "<!-- PROJECT-STATUS:END -->"
) | ForEach-Object {
    if ($_ -is [array]) { $_ } else { $_ }
}

$statusText = ($statusBlock -join [Environment]::NewLine) + [Environment]::NewLine

if (Test-Path $ReadmePath) {
    $existing = Get-Content $ReadmePath -Raw
    if ($existing -match '(?s)<!-- PROJECT-STATUS:START -->.*<!-- PROJECT-STATUS:END -->') {
        $updated = [regex]::Replace($existing, '(?s)<!-- PROJECT-STATUS:START -->.*<!-- PROJECT-STATUS:END -->', $statusText.TrimEnd())
    }
    else {
        $updated = $existing.TrimEnd() + [Environment]::NewLine + [Environment]::NewLine + $statusText
    }
}
else {
    $updated = @(
        "# BootManagerV2",
        "",
        "BootManagerV2 is een lokale, Raspberry Pi-vriendelijke bootmanagementapplicatie met digitaal logboek, NMEA/YDEN-ingest, live dashboard en systeembeheer voor gebruik aan boord.",
        "",
        $statusText.TrimEnd()
    ) -join [Environment]::NewLine
}

$updated = $updated.TrimEnd()
Set-Content -Path $ReadmePath -Value $updated -Encoding utf8
