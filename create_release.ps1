# PowerShell script to create and archive a versioned folder

param (
    [string]$Version
)

if (-not $Version) {
    Write-Host "Please provide a version parameter."
    Write-Host "Usage: $PSCommandPath -Version <version>"
    exit 1
}
$ModName = "FinFix"

# Prevent overwriting an existing build
$ZipPath = "releases/$ModName-$Version.zip"
if (Test-Path $ZipPath) {
     Write-Host "Release $Version already exists."
     exit 1
}

# Update version in manifest.json
$ManifestPath = "Thunderstore\manifest.json"
if (Test-Path $ManifestPath) {
    $ManifestContent = Get-Content -Path $ManifestPath -Raw | ConvertFrom-Json
    $ManifestContent.version_number = $Version

    if ($PSVersionTable.PSVersion.Major -ge 7) { 
      # PowerShell (Core) 7 - no workaround needed.
      $ManifestContent | ConvertTo-Json -Depth 6 | Set-Content -Path $ManifestPath
    }
    elseif ((Get-Command -ErrorAction Ignore ConvertTo-AdvancedJson)) {
      # Windows PowerShell: Use the PSAdvancedJsonCmdlet module, if available
      $ManifestContent | ConvertTo-AdvancedJson -Depth 6 | Set-Content -Path $ManifestPath
    } else {
      Write-Host "Please install PSAdvancedJsonCmdlet for cleaner json formatting"
      Write-Host "https://www.powershellgallery.com/packages/PSAdvancedJsonCmdlet/"
      exit 1
    }

    Write-Host "Updated manifest.json with version: $Version"
} else {
    Write-Host "manifest.json not found at path: $ManifestPath"
    exit 1
}

# Update version in $ModNamePlugin.cs
$PluginPath = "src\$($ModName)Plugin.cs"
if (Test-Path $PluginPath) {
    $PluginContent = Get-Content -Path $PluginPath
    $UpdatedPluginContent = $PluginContent -replace '^(\s*public const string PluginVersion = )"[^"]*";', "`$1`"$Version`";"
    $UpdatedPluginContent | Set-Content -Path $PluginPath
    Write-Host "Updated $($ModName)Plugin.cs with version: $Version"
} else {
    Write-Host "$($ModName)Plugin.cs not found at path: $PluginPath"
    exit 1
}

# Build in Release mode
Write-Host "Building in Release mode..."
dotnet build --configuration Release

# Create the folders
$Tmp = "tmp"
New-Item -ItemType Directory -Path $Tmp -Force | Out-Null
New-Item -ItemType Directory -Path "$Tmp\plugins\$ModName" -Force | Out-Null
New-Item -ItemType Directory -Path "releases" -Force | Out-Null

# Copy files to tmp
Copy-Item -Path "$ManifestPath" -Destination "$Tmp\" -Force
Copy-Item -Path "README.md" -Destination "$Tmp\" -Force
Copy-Item -Path "fix_showcase.png" -Destination "$Tmp\" -Force
Copy-Item -Path "icon.png" -Destination "$Tmp\" -Force
Copy-Item -Path "CHANGELOG.md" -Destination "$Tmp\" -Force
Copy-Item -Path "LICENSE" -Destination "$Tmp\" -Force
Copy-Item -Path "bin\Release\netstandard2.1\$ModName.dll" "$Tmp\plugins\$ModName\" -Force

# Zip the contents of tmp
Compress-Archive -Path "$Tmp\*" -DestinationPath $ZipPath

# Clean tmp
Remove-Item $Tmp -Force -Recurse

# Completion message
Write-Host "Archive created: $ZipPath"