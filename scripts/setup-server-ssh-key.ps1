param(
    [string]$ServerHost = "39.97.243.210",
    [string]$ServerUser = "root",
    [int]$ServerPort = 22,
    [string]$KeyPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

if (-not $KeyPath) {
    $KeyPath = Join-Path $RepoRoot "artifacts\ssh\bytebite_deploy_ed25519"
}

function Get-NativeTool {
    param([string[]]$Names)

    foreach ($name in $Names) {
        $command = Get-Command $name -ErrorAction SilentlyContinue
        if ($command -and $command.Source) {
            return $command.Source
        }
    }

    throw "Required command not found: $($Names -join ', ')"
}

function BashQuote {
    param([string]$Value)
    return "'" + $Value.Replace("'", "'\''") + "'"
}

$keyDir = Split-Path -Parent $KeyPath
New-Item -ItemType Directory -Force $keyDir | Out-Null

if (-not (Test-Path $KeyPath)) {
    $sshKeygen = Get-NativeTool -Names @("ssh-keygen.exe", "ssh-keygen")
    $escapedSshKeygen = $sshKeygen.Replace('"', '\"')
    $escapedKeyPath = $KeyPath.Replace('"', '\"')
    & cmd.exe /c "`"$escapedSshKeygen`" -t ed25519 -N `"`" -C bytebite-deploy -f `"$escapedKeyPath`""
    if ($LASTEXITCODE -ne 0) {
        throw "ssh-keygen failed with exit code $LASTEXITCODE"
    }
}

$publicKeyPath = "$KeyPath.pub"
if (-not (Test-Path $publicKeyPath)) {
    throw "SSH public key not found: $publicKeyPath"
}

$publicKey = (Get-Content -Raw -LiteralPath $publicKeyPath).Trim()
$publicKeyQ = BashQuote $publicKey
$remote = "${ServerUser}@${ServerHost}"
$ssh = Get-NativeTool -Names @("ssh.exe", "ssh")
$sshArgs = @(
    "-p", "$ServerPort",
    "-o", "StrictHostKeyChecking=accept-new"
)

$remoteCommand = "set -euo pipefail; mkdir -p ~/.ssh; chmod 700 ~/.ssh; rm -f ~/.ssh/authorized_keys`$'\r'; touch ~/.ssh/authorized_keys; if ! grep -qxF $publicKeyQ ~/.ssh/authorized_keys; then printf '%s\n' $publicKeyQ >> ~/.ssh/authorized_keys; fi; chmod 600 ~/.ssh/authorized_keys"

& $ssh @sshArgs $remote $remoteCommand
if ($LASTEXITCODE -ne 0) {
    throw "Failed to install SSH public key on server"
}

& $ssh @(
    "-p", "$ServerPort",
    "-i", $KeyPath,
    "-o", "StrictHostKeyChecking=accept-new",
    "-o", "IdentitiesOnly=yes",
    "-o", "BatchMode=yes",
    $remote,
    "echo SSH key login OK"
)
if ($LASTEXITCODE -ne 0) {
    throw "SSH key login test failed"
}

Write-Host "SSH key is ready: $KeyPath"
