# BOB Media Player - Installer

This directory contains installer setup files for BOB Media Player.

## Files

- `BobMediaPlayer-Setup.iss` - Inno Setup script for creating the installer
- `BobMediaPlayer-NSIS.nsi` - NSIS script for creating the installer (alternative)
- `License.txt` - MIT license file for the installer
- `README.md` - This file

## Building the Installer

### Prerequisites

1. **Build the application first:**
   ```bash
   cd "c:\Users\Alpha\CascadeProjects\bob media player"
   dotnet restore
   dotnet build --configuration Release
   dotnet publish --configuration Release --self-contained true
   ```

2. **Choose your installer tool:**

#### Option A: Inno Setup (Recommended)
- Download Inno Setup from: https://jrsoftware.org/download.php/is.exe
- Install it to default location
- Open `BobMediaPlayer-Setup.iss` in Inno Setup Compiler
- Press F9 to compile
- Output: `BobMediaPlayer-Setup-1.0.0.exe`

#### Option B: NSIS (Alternative)
- Download NSIS from: https://sourceforge.net/projects/nsis/
- Install it to default location
- Open command prompt in the project directory
- Run: `makensis BobMediaPlayer-NSIS.nsi`
- Output: `BobMediaPlayer-Setup-1.0.0.exe`

### Quick Build Script

For convenience, you can use the build script:

```batch
build-installer.bat
```

## Installer Features

- **✅ Self-Contained Application**: Includes .NET 8.0 runtime - no separate installation required!
- **✅ Windows 11 Compatible**: Fixed white bar and fullscreen issues with WindowChrome
- **✅ Modern UI** with custom icons and images
- **✅ File Associations** for MP3, MP4, AVI, JPG, PNG, GIF, and more
- **✅ Desktop Shortcut** (optional)
- **✅ Start Menu Entries**
- **✅ Automatic Uninstallation**
- **✅ Clean Uninstall** that removes all files, registry entries, and file associations

## Supported Media Formats

**Audio**: MP3, WAV, M4A, WMA
**Video**: MP4, AVI, MKV, MOV, WMV
**Images**: JPG, PNG, GIF, BMP

## Installation Requirements

- Windows 10 or later
- **No .NET runtime installation required** - it's included!

## Customization

To customize the installer:

1. Update version numbers in the `.iss` or `.nsi` file
2. Modify the company and product information
3. Add custom images (for Inno Setup: SetupImage.bmp, SetupSmallImage.bmp)
4. Adjust file associations in the [Registry] section (Inno Setup) or registry entries (NSIS)
5. Update the license file if needed

## Troubleshooting

If the installer fails:
1. Ensure Windows 10 or later is being used
2. Check that all source files exist in the publish directory
3. Verify the icon.ico file exists
4. For NSIS: Run `makensis /V4 script.nsi` for verbose output

## Self-Contained vs Framework-Dependent

This installer creates a **self-contained application** that includes:
- Complete .NET 8.0 runtime
- All required system libraries
- Your application and dependencies
- Everything needed to run on Windows 10+

**Benefits:**
- No external dependencies
- Faster startup (no JIT compilation)
- Guaranteed compatibility
- Efficient compression (161MB uncompressed → ~13.6MB installer)

**Previous version** required users to install .NET 8.0 Desktop Runtime separately.
