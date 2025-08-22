#!/usr/bin/env powershell
param (
    [string]$Version,
    [string]$BuildConfig = "Release"
)

if ($Version -eq "") {
    Write-Host "Usage: .\MakeInstaller.ps1 <VERSION_NUMBER> [-BuildConfig <BUILD_CONFIG>]"
    Write-Host "Example: .\MakeInstaller.ps1 2025.823.0 -BuildConfig Debug"
    exit
}

$tmpPub = ".\pub"
if (-not (Test-Path -Path $tmpPub)) {
    New-Item -ItemType Directory -path $tmpPub
}

dotnet publish -c $BuildConfig osu.Desktop --self-contained -r win-x64 -o $tmpPub -verbosity:m /p:Version=$Version
vpk pack --packId jvnkosu.Client --packTitle "jvnkosu!lazer" --packVersion $Version --packDir ./pub --mainExe="osu!.exe"