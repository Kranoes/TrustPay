<#
  One-shot local secret rotation (run once from the folder that contains docker-compose.yml):
      .\scripts\rotate-secrets.ps1
  What it does (the new values are never printed):
   1. generates a new PostgreSQL password
   2. changes it INSIDE the running container (data is kept)  -> ALTER USER
   3. writes it to .env (docker compose reads it; .env is git-ignored)
   4. writes the connection string and a new BankOptions:SecretKey to dotnet user-secrets
#>
param([string]$Container = "trustpay_postgres", [string]$Project = "src/TrustPay.Api")
$ErrorActionPreference = "Stop"

function New-HexSecret([int]$bytes) {
    $b = New-Object byte[] $bytes
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b)
    return (($b | ForEach-Object { $_.ToString("x2") }) -join '')
}

if (-not (Test-Path "docker-compose.yml")) { throw "Run this from the folder that contains docker-compose.yml" }
$running = docker ps --filter "name=$Container" --filter "status=running" --format "{{.Names}}"
if ($running -ne $Container) { throw "Container $Container is not running. Start it: docker compose up -d" }

$pw = New-HexSecret 24
docker exec $Container psql -U postgres -c "ALTER USER postgres PASSWORD '$pw';"
if ($LASTEXITCODE -ne 0) { throw "ALTER USER failed - nothing else was changed." }

"POSTGRES_PASSWORD=$pw" | Out-File -Encoding ascii .env
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=trustpay_db;Username=postgres;Password=$pw" --project $Project
dotnet user-secrets set "BankOptions:SecretKey" (New-HexSecret 32) --project $Project

# sanity check: can we log in with the new password over TCP?
docker exec -e PGPASSWORD=$pw $Container psql -h 127.0.0.1 -U postgres -d trustpay_db -c "SELECT 1;" | Out-Null
if ($LASTEXITCODE -eq 0) { Write-Host "OK: new password works, .env and user-secrets updated." -ForegroundColor Green }
else { Write-Host "WARNING: login with the new password failed." -ForegroundColor Red }
Write-Host "Now restart the API. Keep .env and user-secrets out of git."
