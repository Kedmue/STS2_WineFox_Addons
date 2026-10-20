param(
    [int]$GameProcessId = 7796,
    [datetime]$GameStartTime = [datetime]'2026-10-20T11:36:03',
    [string]$Source = (Join-Path $PSScriptRoot '..\output\staged\STS2_WineFox_Addons'),
    [string]$Destination = 'D:\SteamLibrary\steamapps\common\Slay the Spire 2\mods\STS2_WineFox_Addons',
    [int]$TimeoutMinutes = 240
)

$logPath = Join-Path $PSScriptRoot '..\output\deploy.log'

function Write-DeployLog([string]$Message) {
    $line = '{0:yyyy-MM-dd HH:mm:ss} {1}' -f (Get-Date), $Message
    Add-Content -LiteralPath $logPath -Value $line -Encoding utf8
}

Write-DeployLog "Waiting for game process $GameProcessId (started $GameStartTime) to exit."

$deadline = (Get-Date).AddMinutes($TimeoutMinutes)
while ((Get-Date) -lt $deadline) {
    $process = Get-Process -Id $GameProcessId -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        break
    }

    try {
        # StartTime keeps milliseconds, while the parameter may not.
        if ([math]::Abs(($process.StartTime - $GameStartTime).TotalSeconds) -gt 5) {
            break
        }
    }
    catch {
        break
    }

    Start-Sleep -Seconds 2
}

if ($null -ne (Get-Process -Id $GameProcessId -ErrorAction SilentlyContinue)) {
    Write-DeployLog 'Timed out while waiting for the game to exit; nothing was deployed.'
    exit 1
}

$files = @(
    'STS2_WineFox_Addons.pck',
    'STS2_WineFox_Addons.json',
    'STS2_WineFox_Addons.dll'
)

foreach ($name in $files) {
    if (-not (Test-Path -LiteralPath (Join-Path $Source $name))) {
        Write-DeployLog "Missing staged file: $name"
        exit 2
    }
}

if (-not (Test-Path -LiteralPath $Destination)) {
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
}

$copyDeadline = (Get-Date).AddSeconds(60)
foreach ($name in $files) {
    $sourcePath = Join-Path $Source $name
    $destinationPath = Join-Path $Destination $name

    while ($true) {
        try {
            Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
            Write-DeployLog "Copied $name"
            break
        }
        catch {
            if ((Get-Date) -ge $copyDeadline) {
                Write-DeployLog "Failed to copy ${name}: $($_.Exception.Message)"
                exit 3
            }

            Start-Sleep -Seconds 1
        }
    }
}

Write-DeployLog 'Deployment completed successfully.'
