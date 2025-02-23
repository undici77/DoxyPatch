# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

# Define the publish targets
$targets = @(
    "win-x64"
    "win-arm64"
    "linux-x64"
    "linux-arm64"
    "osx-x64"
    "osx-arm64"
)

# Project path and models directory
$projectPath = "Src/DoxyPatch.csproj"
$modelsDir = "./Models"

# Get the short hash of the latest Git commit
$gitHash = git rev-parse --short HEAD 2>$null
if (-not $gitHash) {
    Write-Host "Could not retrieve Git commit hash. Exiting..."
    exit 1
}

# Remove and recreate the artifacts directory
$artifactsDir = "artifacts"
if (Test-Path $artifactsDir) {
    Remove-Item -Recurse -Force $artifactsDir
}
New-Item -ItemType Directory -Path $artifactsDir | Out-Null

# Loop through each publish target
foreach ($target in $targets) {
    $outputDir = "output/$target"

    Write-Host "Publishing for target: $target..."
    dotnet publish $projectPath -c Release -r $target --self-contained true -p:PublishSingleFile=true -p:UseAppHost=true -o $outputDir
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error during publishing for $target. Exiting..."
        exit 1
    }
    Write-Host "Publish completed."

    # Create Models directory and copy files
    $modelsOutputDir = "$outputDir/Models"
    New-Item -ItemType Directory -Path $modelsOutputDir -Force | Out-Null
    Copy-Item -Path "$modelsDir\*" -Destination $modelsOutputDir -Recurse -Force
    Write-Host "Files copied into $target's Models folder."

    # Create ZIP archive
    $zipFile = "$artifactsDir/${target}_${gitHash}.zip"
    Compress-Archive -Path $outputDir -DestinationPath $zipFile -Force
    Write-Host "ZIP archive created: $zipFile."
}

Write-Host "All publications and packaging have been successfully completed."
