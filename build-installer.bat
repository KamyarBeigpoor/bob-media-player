@echo off
echo BOB Media Player - Installer Builder
echo ====================================

echo.
rem Settings
set "PROJECT=BobMediaPlayer.csproj"
set "PUBLISH_DIR=bin\Release\net8.0-windows\win-x64\publish"

echo Building application...
dotnet restore "%PROJECT%"
if errorlevel 1 goto :error

dotnet build "%PROJECT%" --configuration Release
if errorlevel 1 goto :error

dotnet publish "%PROJECT%" --configuration Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%"
if errorlevel 1 goto :error

echo.
echo Application built successfully!
echo.

rem Choice handling: accept first arg or default to Inno
set "CHOICE=%1"
if "%CHOICE%"=="" (
    echo Choose installer type:
    echo 1. Inno Setup (Recommended)
    echo 2. NSIS
    echo.
    set /p CHOICE="Enter your choice (1 or 2): "
)
if "%CHOICE%"=="1" goto :inno
if "%CHOICE%"=="2" goto :nsis
echo Invalid or no choice provided. Defaulting to Inno Setup...
goto :inno

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
echo Installer created. Check the Output folder for the generated setup file.
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
