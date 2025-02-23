# Imposta la modalità di errore per fermarsi in caso di problemi
$ErrorActionPreference = "Stop"

# Definizione dei target di pubblicazione
$targets = @(
    "win-x64"
    "win-arm64"
    "linux-x64"
    "linux-arm64"
    "osx-x64"
    "osx-arm64"
)

# Percorso del progetto e della cartella dei modelli
$projectPath = "Src/DoxyPatch.csproj"
$modelsDir = "./Models"

# Ottieni l'hash dell'ultimo commit Git
$gitHash = git rev-parse --short HEAD 2>$null
if (-not $gitHash) {
    Write-Host "Could not retrieve Git commit hash. Exiting..."
    exit 1
}

# Rimuove e ricrea la directory artifacts
$artifactsDir = "artifacts"
if (Test-Path $artifactsDir) {
    Remove-Item -Recurse -Force $artifactsDir
}
New-Item -ItemType Directory -Path $artifactsDir | Out-Null

# Loop sui target di pubblicazione
foreach ($target in $targets) {
    $outputDir = "output/$target"

    Write-Host "Publishing for target: $target..."
    dotnet publish $projectPath -c Release -r $target --self-contained true -p:PublishSingleFile=true -p:UseAppHost=true -o $outputDir
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error during publishing for $target. Exiting..."
        exit 1
    }
    Write-Host "Publish completed."

    # Creazione della cartella Models e copia dei file
    $modelsOutputDir = "$outputDir/Models"
    New-Item -ItemType Directory -Path $modelsOutputDir -Force | Out-Null
    Copy-Item -Path "$modelsDir\*" -Destination $modelsOutputDir -Recurse -Force
    Write-Host "Files copied into $target's Models folder."

    # Creazione dell'archivio ZIP
    $zipFile = "$artifactsDir/${target}_${gitHash}.zip"
    Compress-Archive -Path $outputDir -DestinationPath $zipFile -Force
    Write-Host "ZIP archive created: $zipFile."
}

Write-Host "All publications and packaging have been successfully completed."
