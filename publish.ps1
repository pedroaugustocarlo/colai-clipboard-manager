<#
    Publica o Colaí como um único .exe self-contained, lendo o nome do
    executável e a versão diretamente de ClipboardManager/AppInfo.cs —
    evita ter que digitar/lembrar esses valores toda vez.
#>

$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$projectPath = Join-Path $repoRoot "ClipboardManager"
$appInfoPath = Join-Path $projectPath "AppInfo.cs"
$outputDir = Join-Path $repoRoot "Publish"

if (-not (Test-Path $appInfoPath)) {
    throw "Não encontrei $appInfoPath"
}

$appInfoContent = Get-Content -Path $appInfoPath -Raw

$executableNameMatch = [regex]::Match($appInfoContent, 'ExecutableName\s*=\s*"([^"]+)"')
$versionMatch = [regex]::Match($appInfoContent, '\bVersion\s*=\s*"([^"]+)"')

if (-not $executableNameMatch.Success) {
    throw "Não encontrei a constante ExecutableName em AppInfo.cs"
}

if (-not $versionMatch.Success) {
    throw "Não encontrei a constante Version em AppInfo.cs"
}

$executableName = $executableNameMatch.Groups[1].Value
$version = $versionMatch.Groups[1].Value

Write-Host "Publicando $executableName.exe (versão $version)..."

dotnet publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:AssemblyName=$executableName `
    -p:Version=$version `
    -o $outputDir

Write-Host "Pronto: $outputDir\$executableName.exe"
