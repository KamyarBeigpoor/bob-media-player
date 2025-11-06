@echo off
echo BOB Media Player - Installer Builder
echo ====================================

echo.
echo Building application...
dotnet restore
if errorlevel 1 goto :error

dotnet build --configuration Release
if errorlevel 1 goto :error

dotnet publish --configuration Release --self-contained true
if errorlevel 1 goto :error

echo.
echo Application built successfully!
echo.

echo Choose installer type:
echo 1. Inno Setup (Recommended)
echo 2. NSIS
echo.

set /p choice="Enter your choice (1 or 2): "

if "%choice%"=="1" goto :inno
if "%choice%"=="2" goto :nsis

:inno
echo.
echo Building with Inno Setup...
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "BobMediaPlayer-Setup.iss"
    if errorlevel 1 goto :error
    echo Inno Setup installer created successfully!
    goto :success
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    "C:\Program Files\Inno Setup 6\ISCC.exe" "BobMediaPlayer-Setup.iss"
    if errorlevel 1 goto :error
    echo Inno Setup installer created successfully!
    goto :success
) else (
    echo ERROR: Inno Setup not found. Please install it first.
    echo Download from: https://jrsoftware.org/download.php/is.exe
    goto :error
)

:nsis
echo.
echo Building with NSIS...
if exist "C:\Program Files (x86)\NSIS\makensis.exe" (
    "C:\Program Files (x86)\NSIS\makensis.exe" "BobMediaPlayer-NSIS.nsi"
    if errorlevel 1 goto :error
    echo NSIS installer created successfully!
    goto :success
) else if exist "C:\Program Files\NSIS\makensis.exe" (
    "C:\Program Files\NSIS\makensis.exe" "BobMediaPlayer-NSIS.nsi"
    if errorlevel 1 goto :error
    echo NSIS installer created successfully!
    goto :success
) else (
    echo ERROR: NSIS not found. Please install it first.
    echo Download from: https://sourceforge.net/projects/nsis/
    goto :error
)

:success
echo.
echo Build completed successfully!
echo Installer created: BobMediaPlayer-Setup-1.0.0.exe
echo.
pause
goto :end

:error
echo.
echo Build failed!
echo.
pause
goto :end

:end
