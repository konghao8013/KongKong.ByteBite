param(
    [string]$AppName = "bytebite-api",
    [string]$ImageName = "bytebite-api",
    [string]$ImageTag = "local",
    [int]$HostPort = 8881,
    [string]$DockerNetwork = "bytebite-net",
    [string]$DbContainerName = "bytebite-postgres",
    [string]$DbHost = "bytebite-postgres",
    [int]$DbPort = 5432,
    [string]$DbName = "kongkong_bytebite",
    [string]$DbUser = "konghao",
    [string]$DbPassword = "hitek.123",
    [switch]$SkipBuild,
    [switch]$SkipPostgres
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$PublishDir = Join-Path $RepoRoot "artifacts\docker\publish"
$UploadsDir = Join-Path $RepoRoot "artifacts\docker\uploads"
$ImageRef = "${ImageName}:${ImageTag}"

function Invoke-CommandStep {
    param(
        [string]$Title,
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Title"
    & $Action
}

function Invoke-External {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$WorkingDirectory = $RepoRoot
    )

    Push-Location $WorkingDirectory
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "$FilePath failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

function Ensure-DockerNetwork {
    docker network inspect $DockerNetwork *> $null
    if ($LASTEXITCODE -ne 0) {
        Invoke-External "docker" @("network", "create", $DockerNetwork)
    }
}

function Ensure-Postgres {
    docker inspect $DbContainerName *> $null
    if ($LASTEXITCODE -eq 0) {
        $running = docker inspect -f "{{.State.Running}}" $DbContainerName
        if ($running -ne "true") {
            Invoke-External "docker" @("start", $DbContainerName)
        }

        docker network connect $DockerNetwork $DbContainerName *> $null
        return
    }

    Invoke-External "docker" @(
        "run", "-d",
        "--name", $DbContainerName,
        "--restart", "unless-stopped",
        "--network", $DockerNetwork,
        "-e", "POSTGRES_DB=$DbName",
        "-e", "POSTGRES_USER=$DbUser",
        "-e", "POSTGRES_PASSWORD=$DbPassword",
        "-v", "${DbContainerName}-data:/var/lib/postgresql/data",
        "postgres:17-alpine"
    )
}

function Build-PublishOutput {
    if (Test-Path $PublishDir) {
        Remove-Item -LiteralPath $PublishDir -Recurse -Force
    }
    New-Item -ItemType Directory -Force $PublishDir | Out-Null

    Invoke-External "npm.cmd" @("ci") (Join-Path $RepoRoot "web")
    Invoke-External "npm.cmd" @("run", "build") (Join-Path $RepoRoot "web")
    Invoke-External "dotnet" @("restore", "src\ByteBite.Api\ByteBite.Api.csproj")
    Invoke-External "dotnet" @("publish", "src\ByteBite.Api\ByteBite.Api.csproj", "-c", "Release", "-o", $PublishDir, "/p:UseAppHost=false")

    $wwwroot = Join-Path $PublishDir "wwwroot"
    $publishUploads = Join-Path $wwwroot "uploads"
    if (Test-Path $publishUploads) {
        Remove-Item -LiteralPath $publishUploads -Recurse -Force
    }
    New-Item -ItemType Directory -Force $wwwroot | Out-Null
    Copy-Item -Path (Join-Path $RepoRoot "web\dist\*") -Destination $wwwroot -Recurse -Force
}

Invoke-CommandStep "Build local publish output" {
    if (-not $SkipBuild) {
        Build-PublishOutput
    }
}

Invoke-CommandStep "Build Docker image $ImageRef" {
    Invoke-External "docker" @("build", "-f", "Dockerfile.release", "-t", $ImageRef, ".")
}

Invoke-CommandStep "Prepare Docker network and PostgreSQL" {
    Ensure-DockerNetwork
    if (-not $SkipPostgres) {
        Ensure-Postgres
    }
}

Invoke-CommandStep "Run $AppName on port $HostPort" {
    New-Item -ItemType Directory -Force $UploadsDir | Out-Null
    docker rm -f $AppName *> $null

    $connectionString = "Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPassword"
    $uploadsPath = (Resolve-Path $UploadsDir).Path
    Invoke-External "docker" @(
        "run", "-d",
        "--name", $AppName,
        "--restart", "unless-stopped",
        "--network", $DockerNetwork,
        "-p", "${HostPort}:8080",
        "-e", "ASPNETCORE_ENVIRONMENT=Production",
        "-e", "ConnectionStrings__DefaultConnection=$connectionString",
        "-v", "${uploadsPath}:/app/wwwroot/uploads",
        $ImageRef
    )
}

Invoke-CommandStep "Verify local access" {
    Start-Sleep -Seconds 5
    $url = "http://localhost:$HostPort/"
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 20
    Write-Host "OK $($response.StatusCode): $url"
}
