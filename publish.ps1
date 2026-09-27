# PowerShell Publish & Verification Script for SchoolErp.WebAPI
$ErrorActionPreference = "Stop"

$rootPath = $PSScriptRoot
if (-not $rootPath) {
    $rootPath = (Get-Location).Path
}

Set-Location $rootPath

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " 1. Publishing SchoolErp.WebAPI" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

dotnet publish src/SchoolErp.WebAPI/SchoolErp.WebAPI.csproj -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}
Write-Host "`ndotnet publish completed successfully.`n" -ForegroundColor Green

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " 2. Setting Permissions for IIS_IUSRS" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$publishDir = Join-Path $rootPath "publish"
if (-not (Test-Path $publishDir)) {
    Write-Error "Publish directory not found at $publishDir"
    exit 1
}

# Ensure logs folder exists for stdout logging
$logsDir = Join-Path $publishDir "logs"
if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
    Write-Host "Created logs folder at: $logsDir" -ForegroundColor Gray
}

# Grant Modify permissions to IIS_IUSRS with inheritance
icacls "$publishDir" /grant "IIS_IUSRS:(OI)(CI)M" /t /q
if ($LASTEXITCODE -ne 0) {
    Write-Warning "icacls returned exit code $LASTEXITCODE. If permission was denied, please run PowerShell as Administrator."
} else {
    Write-Host "Successfully set permissions on $publishDir for IIS_IUSRS.`n" -ForegroundColor Green
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " 3. Verifying web.config Configuration" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$webConfigFile = Join-Path $publishDir "web.config"
if (-not (Test-Path $webConfigFile)) {
    Write-Error "web.config was not found in $publishDir"
    exit 1
}

[xml]$webConfigXml = Get-Content -Path $webConfigFile -Raw
$aspNetCore = $webConfigXml.SelectSingleNode("//aspNetCore")

if ($null -eq $aspNetCore) {
    Write-Error "<aspNetCore> element not found in $webConfigFile"
    exit 1
}

# Ensure attributes match required settings
$updated = $false
if ($aspNetCore.GetAttribute("processPath") -ne "dotnet") {
    $aspNetCore.SetAttribute("processPath", "dotnet")
    $updated = $true
}
if ($aspNetCore.GetAttribute("arguments") -ne ".\SchoolErp.WebAPI.dll") {
    $aspNetCore.SetAttribute("arguments", ".\SchoolErp.WebAPI.dll")
    $updated = $true
}
if ($aspNetCore.GetAttribute("stdoutLogEnabled") -ne "true") {
    $aspNetCore.SetAttribute("stdoutLogEnabled", "true")
    $updated = $true
}
if ($aspNetCore.GetAttribute("hostingModel").ToLower() -ne "inprocess") {
    $aspNetCore.SetAttribute("hostingModel", "inprocess")
    $updated = $true
}

# Ensure environmentVariables element exists with ASPNETCORE_ENVIRONMENT=Development
$envVarsNode = $aspNetCore.SelectSingleNode("environmentVariables")
if ($null -eq $envVarsNode) {
    $envVarsNode = $webConfigXml.CreateElement("environmentVariables")
    $aspNetCore.AppendChild($envVarsNode) | Out-Null
    $updated = $true
}

$aspnetEnvNode = $envVarsNode.SelectSingleNode("environmentVariable[@name='ASPNETCORE_ENVIRONMENT']")
if ($null -eq $aspnetEnvNode) {
    $aspnetEnvNode = $webConfigXml.CreateElement("environmentVariable")
    $aspnetEnvNode.SetAttribute("name", "ASPNETCORE_ENVIRONMENT")
    $aspnetEnvNode.SetAttribute("value", "Development")
    $envVarsNode.AppendChild($aspnetEnvNode) | Out-Null
    $updated = $true
} elseif ($aspnetEnvNode.GetAttribute("value") -ne "Development") {
    $aspnetEnvNode.SetAttribute("value", "Development")
    $updated = $true
}

if ($updated) {
    $webConfigXml.Save($webConfigFile)
    Write-Host "Updated web.config with required aspNetCore settings." -ForegroundColor Yellow
}

# Read back and verify
[xml]$verifyXml = Get-Content -Path $webConfigFile -Raw
$node = $verifyXml.SelectSingleNode("//aspNetCore")

$procPath = $node.GetAttribute("processPath")
$argsVal = $node.GetAttribute("arguments")
$stdoutVal = $node.GetAttribute("stdoutLogEnabled")
$hostingVal = $node.GetAttribute("hostingModel")

Write-Host "Verified <aspNetCore> configuration:" -ForegroundColor Gray
Write-Host "  processPath      = $procPath"
Write-Host "  arguments        = $argsVal"
Write-Host "  stdoutLogEnabled = $stdoutVal"
Write-Host "  hostingModel     = $hostingVal"

if ($procPath -eq "dotnet" -and $argsVal -eq ".\SchoolErp.WebAPI.dll" -and $stdoutVal -eq "true" -and $hostingVal.ToLower() -eq "inprocess") {
    Write-Host "`nVERIFICATION SUCCESS: web.config contains <aspNetCore processPath=`"dotnet`" arguments=`".\SchoolErp.WebAPI.dll`" stdoutLogEnabled=`"true`" hostingModel=`"inprocess`" />" -ForegroundColor Green
} else {
    Write-Error "VERIFICATION FAILED: web.config does not have expected attributes."
    exit 1
}
