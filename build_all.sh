#!/bin/bash

# Array of publish targets
targets=(
    "win-x64"
    "win-arm64"
    "linux-x64"
    "linux-arm64"
    "osx-x64"
    "osx-arm64"
)

# Path to the project and Models directory
project_path="Src/DoxyPatch.csproj"
models_dir="./Models"

# Get current Git commit hash
git_hash=$(git rev-parse --short HEAD 2>/dev/null)
if [ -z "$git_hash" ]; then
    echo "Could not retrieve Git commit hash. Exiting..."
    exit 1
fi

# Remove existing output directory if it exists
if [ -d "output" ]; then
  rm -rf output
fi

# Remove existing artifacts directory and create a new one
artifacts_dir="artifacts"
if [ -d "$artifacts_dir" ]; then
  rm -rf "$artifacts_dir"
fi
mkdir -p "$artifacts_dir"

# Loop over each publish target
for target in "${targets[@]}"; do
    # Determine the correct output directory options
    case $target in
        win-x64|win-arm64)
            publish_args="-r $target --self-contained true -p:PublishSingleFile=true -p:UseAppHost=true"
            ;;
        linux-x64|linux-arm64)
            publish_args="-r $target --self-contained true -p:PublishSingleFile=true -p:UseAppHost=true"
            ;;
        osx-x64|osx-arm64)
            publish_args="-r $target --self-contained true -p:PublishSingleFile=true"
            ;;
    esac

    # Define the output directory
    output_dir="output/$target"

    # Execute dotnet publish
    echo "Publishing for target: $target..."
    dotnet publish "$project_path" -c Release $publish_args -o "$output_dir"
    echo "Publish completed."

    # Create Models folder in output if it doesn't exist
    mkdir -p "$output_dir/Models"

    # Copy files from the Model directory to output
    cp -r "$models_dir/"* "$output_dir/Models/"
    echo "Files copied into $target's Models folder."

    # Define the ZIP file name with Git hash and create the archive in artifacts directory
    zip_file="$artifacts_dir/${target}_${git_hash}.zip"
    zip -r "$zip_file" "$output_dir"
    echo "ZIP archive created: $zip_file."
done

echo "All publications and packaging have been successfully completed."
