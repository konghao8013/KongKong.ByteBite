param(
    [Parameter(Mandatory = $true)]
    [string]$ServerHost,
    [string]$ServerUser = "root",
    [int]$ServerPort = 22,
    [string]$SshKeyPath = "",
    [ValidateSet("LocalDocker", "RemoteDocker", "RemoteSourceDocker")]
    [string]$BuildMode = "LocalDocker",
    [string]$AppName = "bytebite-api",
    [string]$ImageName = "bytebite-api",
    [string]$ImageTag = (Get-Date -Format "yyyyMMddHHmmss"),
    [int]$HostPort = 8881,
    [string]$RemoteDir = "/opt/bytebite",
    [string]$DockerNetwork = "bytebite-net",
    [string]$DbContainerName = "bytebite-postgres",
    [string]$DbHost = "bytebite-postgres",
    [int]$DbPort = 5432,
    [string]$DbName = "kongkong_bytebite",
    [string]$DbUser = "konghao",
    [string]$DbPassword = "hitek.123",
    [string]$PostgresImage = "docker.1panel.live/library/postgres:17-alpine",
    [string]$NodeImage = "docker.1panel.live/library/node:24-alpine",
    [string]$DotnetSdkImage = "mcr.microsoft.com/dotnet/sdk:10.0",
    [string]$DotnetAspNetImage = "mcr.microsoft.com/dotnet/aspnet:10.0",
    [switch]$SkipBuild,
    [switch]$SkipDockerInstall
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$PublishDir = Join-Path $RepoRoot "artifacts\docker\publish"
$ImageDir = Join-Path $RepoRoot "artifacts\docker\images"
$ImageRef = "${ImageName}:${ImageTag}"
$Remote = "${ServerUser}@${ServerHost}"

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

function Invoke-NpmRestore {
    $webDir = Join-Path $RepoRoot "web"
    Push-Location $webDir
    try {
        & npm.cmd ci
        if ($LASTEXITCODE -eq 0) {
            return
        }

        Write-Warning "npm ci failed. Falling back to npm install so locked local node_modules files do not block deployment."
        & npm.cmd install
        if ($LASTEXITCODE -ne 0) {
            throw "npm install failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

function Get-SshOptions {
    $args = @(
        "-p", "$ServerPort",
        "-o", "StrictHostKeyChecking=accept-new",
        "-o", "ServerAliveInterval=30",
        "-o", "ServerAliveCountMax=4"
    )

    if ($SshKeyPath -and (Test-Path $SshKeyPath)) {
        $args += @("-i", $SshKeyPath)
    }

    return $args
}

function Get-ScpOptions {
    $args = @(
        "-P", "$ServerPort",
        "-o", "StrictHostKeyChecking=accept-new"
    )

    if ($SshKeyPath -and (Test-Path $SshKeyPath)) {
        $args += @("-i", $SshKeyPath)
    }

    return $args
}

function Invoke-RemoteScript {
    param([string]$Script)

    $sshArgs = Get-SshOptions
    $Script | & ssh @sshArgs $Remote "bash -s"
    if ($LASTEXITCODE -ne 0) {
        throw "Remote command failed with exit code $LASTEXITCODE"
    }
}

function Copy-ToServer {
    param(
        [string]$LocalPath,
        [string]$RemotePath
    )

    $scpArgs = Get-ScpOptions
    & scp @scpArgs $LocalPath "${Remote}:$RemotePath"
    if ($LASTEXITCODE -ne 0) {
        throw "scp failed with exit code $LASTEXITCODE"
    }
}

function BashQuote {
    param([string]$Value)
    return "'" + $Value.Replace("'", "'\''") + "'"
}

function Build-PublishOutput {
    if (Test-Path $PublishDir) {
        Remove-Item -LiteralPath $PublishDir -Recurse -Force
    }
    New-Item -ItemType Directory -Force $PublishDir | Out-Null

    Invoke-NpmRestore
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

function Ensure-RemoteEnvironment {
    $installDocker = if ($SkipDockerInstall) { "0" } else { "1" }
    $remoteDirQ = BashQuote $RemoteDir
    $networkQ = BashQuote $DockerNetwork
    $dbContainerQ = BashQuote $DbContainerName
    $dbNameQ = BashQuote $DbName
    $dbUserQ = BashQuote $DbUser
    $dbPasswordQ = BashQuote $DbPassword
    $postgresImageQ = BashQuote $PostgresImage
    $hostPortQ = BashQuote "$HostPort"

    Invoke-RemoteScript @"
set -euo pipefail

REMOTE_DIR=$remoteDirQ
DOCKER_NETWORK=$networkQ
DB_CONTAINER=$dbContainerQ
DB_NAME=$dbNameQ
DB_USER=$dbUserQ
DB_PASSWORD=$dbPasswordQ
POSTGRES_IMAGE=$postgresImageQ
HOST_PORT=$hostPortQ

if [ "$installDocker" = "1" ] && ! command -v docker >/dev/null 2>&1; then
  if command -v dnf >/dev/null 2>&1; then
    dnf install -y dnf-plugins-core yum-utils ca-certificates curl || true
    dnf config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo || true
    dnf install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin || dnf install -y docker
  elif command -v yum >/dev/null 2>&1; then
    yum install -y yum-utils ca-certificates curl || true
    yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo || true
    yum install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin || yum install -y docker
  elif command -v apt-get >/dev/null 2>&1; then
    apt-get update
    apt-get install -y ca-certificates curl gnupg lsb-release
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg || true
    chmod a+r /etc/apt/keyrings/docker.gpg || true
    . /etc/os-release
    echo "deb [arch=`$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu `${VERSION_CODENAME:-jammy} stable" > /etc/apt/sources.list.d/docker.list
    apt-get update || true
    apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin || apt-get install -y docker.io
  else
    echo "Unsupported Linux distribution: cannot install Docker automatically" >&2
    exit 1
  fi
fi

systemctl enable --now docker >/dev/null 2>&1 || systemctl start docker >/dev/null 2>&1 || service docker start

mkdir -p "`$REMOTE_DIR/releases" "`$REMOTE_DIR/uploads" "`$REMOTE_DIR/build"

docker network inspect "`$DOCKER_NETWORK" >/dev/null 2>&1 || docker network create "`$DOCKER_NETWORK"

if docker inspect "`$DB_CONTAINER" >/dev/null 2>&1; then
  [ "`$(docker inspect -f '{{.State.Running}}' "`$DB_CONTAINER")" = "true" ] || docker start "`$DB_CONTAINER"
  docker network connect "`$DOCKER_NETWORK" "`$DB_CONTAINER" >/dev/null 2>&1 || true
else
  docker run -d \
    --name "`$DB_CONTAINER" \
    --restart unless-stopped \
    --network "`$DOCKER_NETWORK" \
    -e POSTGRES_DB="`$DB_NAME" \
    -e POSTGRES_USER="`$DB_USER" \
    -e POSTGRES_PASSWORD="`$DB_PASSWORD" \
    -v "`$REMOTE_DIR/postgres-data:/var/lib/postgresql/data" \
    "`$POSTGRES_IMAGE"
fi

if command -v firewall-cmd >/dev/null 2>&1 && systemctl is-active --quiet firewalld; then
  firewall-cmd --permanent --add-port="`${HOST_PORT}/tcp" >/dev/null 2>&1 || true
  firewall-cmd --reload >/dev/null 2>&1 || true
fi

if command -v ufw >/dev/null 2>&1; then
  ufw allow "`${HOST_PORT}/tcp" >/dev/null 2>&1 || true
fi

docker version
docker ps --filter "name=`$DB_CONTAINER"
"@
}

function Build-LocalDockerImage {
    New-Item -ItemType Directory -Force $ImageDir | Out-Null
    $tarPath = Join-Path $ImageDir "$ImageName-$ImageTag.tar"

    Invoke-External "docker" @("build", "--build-arg", "DOTNET_ASPNET_IMAGE=$DotnetAspNetImage", "-f", "Dockerfile.release", "-t", $ImageRef, ".")
    Invoke-External "docker" @("save", $ImageRef, "-o", $tarPath)

    return $tarPath
}

function Publish-WithLocalImageTar {
    param([string]$TarPath)

    $remoteTar = "$RemoteDir/releases/$(Split-Path $TarPath -Leaf)"
    Copy-ToServer $TarPath $remoteTar

    $remoteTarQ = BashQuote $remoteTar
    Invoke-RemoteScript @"
set -euo pipefail
docker load -i $remoteTarQ
"@
}

function Publish-WithRemoteDockerBuild {
    New-Item -ItemType Directory -Force $ImageDir | Out-Null
    $archivePath = Join-Path $ImageDir "$ImageName-$ImageTag-publish.tgz"
    if (Test-Path $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
    }

    Invoke-External "tar" @("-czf", $archivePath, "-C", $PublishDir, ".")

    $remoteArchive = "$RemoteDir/releases/$(Split-Path $archivePath -Leaf)"
    $remoteDockerfile = "$RemoteDir/releases/Dockerfile.release.$ImageTag"
    Copy-ToServer $archivePath $remoteArchive
    Copy-ToServer (Join-Path $RepoRoot "Dockerfile.release") $remoteDockerfile

    $remoteDirQ = BashQuote $RemoteDir
    $remoteArchiveQ = BashQuote $remoteArchive
    $remoteDockerfileQ = BashQuote $remoteDockerfile
    $imageRefQ = BashQuote $ImageRef
    $imageTarQ = BashQuote "$RemoteDir/releases/$ImageName-$ImageTag.tar"
    $imageTagQ = BashQuote $ImageTag
    $dotnetAspNetImageQ = BashQuote $DotnetAspNetImage

    Invoke-RemoteScript @"
set -euo pipefail
REMOTE_DIR=$remoteDirQ
IMAGE_TAG=$imageTagQ
DOTNET_ASPNET_IMAGE=$dotnetAspNetImageQ
WORK_DIR="`$REMOTE_DIR/build/`$IMAGE_TAG"
rm -rf "`$WORK_DIR"
mkdir -p "`$WORK_DIR/publish"
tar -xzf $remoteArchiveQ -C "`$WORK_DIR/publish"
cp $remoteDockerfileQ "`$WORK_DIR/Dockerfile.release"
docker build --build-arg PUBLISH_DIR=publish --build-arg DOTNET_ASPNET_IMAGE="`$DOTNET_ASPNET_IMAGE" -f "`$WORK_DIR/Dockerfile.release" -t $imageRefQ "`$WORK_DIR"
docker save $imageRefQ -o $imageTarQ
"@
}

function Publish-WithRemoteSourceBuild {
    New-Item -ItemType Directory -Force $ImageDir | Out-Null
    $archivePath = Join-Path $ImageDir "$ImageName-$ImageTag-source.tgz"
    if (Test-Path $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
    }

    Invoke-External "tar" @(
        "--exclude=.git",
        "--exclude=.vs",
        "--exclude=.agent",
        "--exclude=.trae",
        "--exclude=.codex",
        "--exclude=.codex-temp",
        "--exclude=.codex-push-worktree-*",
        "--exclude=artifacts",
        "--exclude=web/node_modules",
        "--exclude=web/dist",
        "--exclude=src/ByteBite.Api/wwwroot/uploads",
        "-czf", $archivePath,
        "."
    )

    $remoteArchive = "$RemoteDir/releases/$(Split-Path $archivePath -Leaf)"
    Copy-ToServer $archivePath $remoteArchive

    $remoteDirQ = BashQuote $RemoteDir
    $remoteArchiveQ = BashQuote $remoteArchive
    $imageRefQ = BashQuote $ImageRef
    $imageTarQ = BashQuote "$RemoteDir/releases/$ImageName-$ImageTag.tar"
    $imageTagQ = BashQuote $ImageTag
    $nodeImageQ = BashQuote $NodeImage
    $dotnetSdkImageQ = BashQuote $DotnetSdkImage
    $dotnetAspNetImageQ = BashQuote $DotnetAspNetImage

    Invoke-RemoteScript @"
set -euo pipefail
REMOTE_DIR=$remoteDirQ
IMAGE_TAG=$imageTagQ
NODE_IMAGE=$nodeImageQ
DOTNET_SDK_IMAGE=$dotnetSdkImageQ
DOTNET_ASPNET_IMAGE=$dotnetAspNetImageQ
WORK_DIR="`$REMOTE_DIR/build/source-`$IMAGE_TAG"
rm -rf "`$WORK_DIR"
mkdir -p "`$WORK_DIR"
tar -xzf $remoteArchiveQ -C "`$WORK_DIR"
docker build \
  --build-arg NODE_IMAGE="`$NODE_IMAGE" \
  --build-arg DOTNET_SDK_IMAGE="`$DOTNET_SDK_IMAGE" \
  --build-arg DOTNET_ASPNET_IMAGE="`$DOTNET_ASPNET_IMAGE" \
  -f "`$WORK_DIR/Dockerfile.remote" \
  -t $imageRefQ \
  "`$WORK_DIR"
docker save $imageRefQ -o $imageTarQ
"@
}

function Restart-RemoteApp {
    $remoteDirQ = BashQuote $RemoteDir
    $appNameQ = BashQuote $AppName
    $networkQ = BashQuote $DockerNetwork
    $hostPortQ = BashQuote "$HostPort"
    $imageRefQ = BashQuote $ImageRef
    $connectionString = "Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPassword"
    $connectionStringQ = BashQuote $connectionString

    Invoke-RemoteScript @"
set -euo pipefail
REMOTE_DIR=$remoteDirQ
APP_NAME=$appNameQ
DOCKER_NETWORK=$networkQ
HOST_PORT=$hostPortQ
IMAGE_REF=$imageRefQ
CONNECTION_STRING=$connectionStringQ

docker rm -f "`$APP_NAME" >/dev/null 2>&1 || true

docker run -d \
  --name "`$APP_NAME" \
  --restart unless-stopped \
  --network "`$DOCKER_NETWORK" \
  -p "`${HOST_PORT}:8080" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="`$CONNECTION_STRING" \
  -v "`$REMOTE_DIR/uploads:/app/wwwroot/uploads" \
  "`$IMAGE_REF"

sleep 8
docker ps --filter "name=`$APP_NAME"
curl -fsS "http://127.0.0.1:`$HOST_PORT/" >/tmp/bytebite-home.html
test -s /tmp/bytebite-home.html
"@
}

Invoke-CommandStep "Build local publish output" {
    if ((-not $SkipBuild) -and $BuildMode -ne "RemoteSourceDocker") {
        Build-PublishOutput
    }
}

Invoke-CommandStep "Prepare server Docker and PostgreSQL" {
    Ensure-RemoteEnvironment
}

if ($BuildMode -eq "LocalDocker") {
    Invoke-CommandStep "Build and export local Docker image $ImageRef" {
        $script:LocalTarPath = Build-LocalDockerImage
    }

    Invoke-CommandStep "Upload and load image tar" {
        Publish-WithLocalImageTar $script:LocalTarPath
    }
}
elseif ($BuildMode -eq "RemoteDocker") {
    Invoke-CommandStep "Upload publish output and build Docker image on server" {
        Publish-WithRemoteDockerBuild
    }
}
else {
    Invoke-CommandStep "Upload source and build Docker image on server" {
        Publish-WithRemoteSourceBuild
    }
}

Invoke-CommandStep "Restart remote app on port $HostPort" {
    Restart-RemoteApp
}

Write-Host ""
Write-Host "Deployment completed: http://$ServerHost`:$HostPort/"
