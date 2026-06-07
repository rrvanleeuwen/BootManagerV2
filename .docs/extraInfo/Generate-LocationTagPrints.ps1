param(
    [string]$ExcelPath = "BootManager_LocationTags_Pilot.xlsx",
    [string]$OutputPath = "generated-location-tags",
    [double]$LabelSizeMm = 40
)

$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$pythonScript = Join-Path $scriptDirectory "generate-location-tag-prints.py"
$requirements = Join-Path $scriptDirectory "requirements-location-tags.txt"

if (-not (Test-Path $pythonScript)) {
    throw "Python-script niet gevonden: $pythonScript"
}

$pythonCommand = Get-Command py -ErrorAction SilentlyContinue
if ($null -eq $pythonCommand) {
    $pythonCommand = Get-Command python -ErrorAction SilentlyContinue
}

if ($null -eq $pythonCommand) {
    throw "Python 3 is niet gevonden. Installeer Python 3 en vink 'Add Python to PATH' aan."
}

$venvPath = Join-Path $scriptDirectory ".venv-location-tags"
$venvPython = Join-Path $venvPath "Scripts\python.exe"

if (-not (Test-Path $venvPython)) {
    Write-Host "Virtuele Python-omgeving wordt aangemaakt..."
    & $pythonCommand.Source -m venv $venvPath
}

if (-not (Test-Path $venvPython)) {
    throw "De virtuele Python-omgeving kon niet worden aangemaakt."
}

Write-Host "Benodigde Python-pakketten worden gecontroleerd..."
& $venvPython -m pip install --disable-pip-version-check --quiet --upgrade pip
& $venvPython -m pip install --disable-pip-version-check --quiet -r $requirements

$resolvedExcel = $ExcelPath
if (-not [System.IO.Path]::IsPathRooted($resolvedExcel)) {
    $resolvedExcel = Join-Path $scriptDirectory $resolvedExcel
}

$resolvedOutput = $OutputPath
if (-not [System.IO.Path]::IsPathRooted($resolvedOutput)) {
    $resolvedOutput = Join-Path $scriptDirectory $resolvedOutput
}

Write-Host "QR-labels worden gegenereerd..."
& $venvPython $pythonScript `
    --excel $resolvedExcel `
    --output $resolvedOutput `
    --label-size-mm $LabelSizeMm

if ($LASTEXITCODE -ne 0) {
    throw "Genereren van de QR-labels is mislukt."
}

$printPdf = Join-Path $resolvedOutput "BootManager_LocationTags_Print.pdf"
Write-Host ""
Write-Host "Klaar. Printbestand: $printPdf"

if (Test-Path $printPdf) {
    Start-Process $printPdf
}
