<#
.SYNOPSIS
    Builds the portable single-file QwertzBridge.exe into ./dist.
.EXAMPLE
    ./publish.ps1
    ./publish.ps1 -SkipTests
#>
[CmdletBinding()]
param(
    [string]$Runtime = 'win-x64',
    [string]$OutputDir = 'dist',
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if (-not $SkipTests) {
        dotnet test -c Release
        if ($LASTEXITCODE -ne 0) { throw 'Tests failed - publish aborted.' }
    }

    dotnet publish src/QwertzBridge.App -c Release -r $Runtime --self-contained `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -o $OutputDir

    if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }

    $exe = Join-Path $OutputDir 'QwertzBridge.exe'
    Write-Host ''
    Write-Host "Done: $exe ($([math]::Round((Get-Item $exe).Length / 1MB, 1)) MB)"
}
finally {
    Pop-Location
}
