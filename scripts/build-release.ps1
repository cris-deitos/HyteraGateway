#!/usr/bin/env pwsh
# Build script for HyteraGateway
# Creates single-file EXE for Windows x64

param(
    [string]$OutputDir = ".\publish",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "🔨 Building HyteraGateway..." -ForegroundColor Cyan

# Clean output directory
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

# Restore dependencies
Write-Host "📦 Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build solution
Write-Host "🏗️ Building solution..." -ForegroundColor Yellow
dotnet build -c $Configuration --no-restore

# Run tests
Write-Host "🧪 Running tests..." -ForegroundColor Yellow
dotnet test -c $Configuration --no-build --verbosity minimal

# Publish UI
Write-Host "📦 Publishing UI..." -ForegroundColor Yellow
dotnet publish src\HyteraGateway.UI\HyteraGateway.UI.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o "$OutputDir\UI"

# Publish API
Write-Host "📦 Publishing API..." -ForegroundColor Yellow
dotnet publish src\HyteraGateway.Api\HyteraGateway.Api.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o "$OutputDir\API"

# Publish Service
Write-Host "📦 Publishing Service..." -ForegroundColor Yellow
dotnet publish src\HyteraGateway.Service\HyteraGateway.Service.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o "$OutputDir\Service"

Write-Host ""
Write-Host "✅ Build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Output files:" -ForegroundColor Cyan
Write-Host "  UI:      $OutputDir\UI\HyteraGateway.UI.exe"
Write-Host "  API:     $OutputDir\API\HyteraGateway.Api.exe"
Write-Host "  Service: $OutputDir\Service\HyteraGateway.Service.exe"
