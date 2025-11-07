;NSIS Installer Script for BOB Media Player
;Created by Installer Generator

!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"

;General
Name "BOB Media Player"
OutFile "BobMediaPlayer-Setup-1.3.exe"
Unicode True
InstallDir "$PROGRAMFILES\BOB Media Player"
InstallDirRegKey HKCU "Software\BOB Media Player" ""
RequestExecutionLevel admin

;Modern UI Configuration
!define MUI_ABORTWARNING
!define MUI_ICON "icon.ico"
!define MUI_UNICON "icon.ico"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "header.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "wizard.bmp"

;Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "License.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

;Languages
!insertmacro MUI_LANGUAGE "English"

;Version Information
VIProductVersion "1.3.0.0"
VIAddVersionKey "ProductName" "BOB Media Player"
VIAddVersionKey "CompanyName" "BOB Media Player"
VIAddVersionKey "FileVersion" "1.3.0.0"
VIAddVersionKey "ProductVersion" "1.3.0.0"
VIAddVersionKey "FileDescription" "A modern media player with VLC-like features"

;Installer Sections
Section "BOB Media Player (required)" SecMain
    SectionIn RO

    SetOutPath "$INSTDIR"

    ;Include all application files including .NET runtime
    File /r "bin\Release\net8.0-windows\win-x64\publish\*"

    ;Icon (override the one from publish directory)
    File "icon.ico"

    ;Store installation folder
    WriteRegStr HKCU "Software\BOB Media Player" "" $INSTDIR

    ;Create uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"

    ;Registry entry for uninstaller
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BOB Media Player" "DisplayName" "BOB Media Player"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BOB Media Player" "UninstallString" "$INSTDIR\Uninstall.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BOB Media Player" "DisplayIcon" "$INSTDIR\icon.ico"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BOB Media Player" "Publisher" "BOB Media Player"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BOB Media Player" "DisplayVersion" "1.3"
    WriteRegDWord HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BOB Media Player" "NoModify" 1
    WriteRegDWord HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BOB Media Player" "NoRepair" 1

SectionEnd

Section "Desktop Shortcut" SecDesktop
    CreateShortCut "$DESKTOP\BOB Media Player.lnk" "$INSTDIR\BobMediaPlayer.exe" "" "$INSTDIR\icon.ico" 0
SectionEnd

Section "Start Menu Shortcuts" SecStartMenu
    CreateDirectory "$SMPROGRAMS\BOB Media Player"
    CreateShortCut "$SMPROGRAMS\BOB Media Player\BOB Media Player.lnk" "$INSTDIR\BobMediaPlayer.exe" "" "$INSTDIR\icon.ico" 0
    CreateShortCut "$SMPROGRAMS\BOB Media Player\Uninstall.lnk" "$INSTDIR\Uninstall.exe" "" "$INSTDIR\icon.ico" 0
SectionEnd

Section "File Associations" SecFileAssoc
    ;Audio formats
    WriteRegStr HKCR ".mp3" "" "BobMediaPlayer.mp3"
    WriteRegStr HKCR ".wav" "" "BobMediaPlayer.wav"
    WriteRegStr HKCR ".m4a" "" "BobMediaPlayer.m4a"
    WriteRegStr HKCR ".wma" "" "BobMediaPlayer.wma"

    ;Video formats
    WriteRegStr HKCR ".mp4" "" "BobMediaPlayer.mp4"
    WriteRegStr HKCR ".avi" "" "BobMediaPlayer.avi"
    WriteRegStr HKCR ".mkv" "" "BobMediaPlayer.mkv"
    WriteRegStr HKCR ".mov" "" "BobMediaPlayer.mov"
    WriteRegStr HKCR ".wmv" "" "BobMediaPlayer.wmv"

    ;Image formats
    WriteRegStr HKCR ".jpg" "" "BobMediaPlayer.jpg"
    WriteRegStr HKCR ".jpeg" "" "BobMediaPlayer.jpeg"
    WriteRegStr HKCR ".png" "" "BobMediaPlayer.png"
    WriteRegStr HKCR ".gif" "" "BobMediaPlayer.gif"
    WriteRegStr HKCR ".bmp" "" "BobMediaPlayer.bmp"

    ;File type definitions
    WriteRegStr HKCR "BobMediaPlayer.mp3" "" "MP3 Audio File"
    WriteRegStr HKCR "BobMediaPlayer.mp3\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.mp3\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.wav" "" "WAV Audio File"
    WriteRegStr HKCR "BobMediaPlayer.wav\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.wav\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.m4a" "" "M4A Audio File"
    WriteRegStr HKCR "BobMediaPlayer.m4a\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.m4a\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.wma" "" "WMA Audio File"
    WriteRegStr HKCR "BobMediaPlayer.wma\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.wma\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.mp4" "" "MP4 Video File"
    WriteRegStr HKCR "BobMediaPlayer.mp4\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.mp4\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.avi" "" "AVI Video File"
    WriteRegStr HKCR "BobMediaPlayer.avi\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.avi\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.mkv" "" "MKV Video File"
    WriteRegStr HKCR "BobMediaPlayer.mkv\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.mkv\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.mov" "" "QuickTime Video File"
    WriteRegStr HKCR "BobMediaPlayer.mov\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.mov\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.wmv" "" "Windows Media Video File"
    WriteRegStr HKCR "BobMediaPlayer.wmv\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.wmv\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.jpg" "" "JPEG Image File"
    WriteRegStr HKCR "BobMediaPlayer.jpg\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.jpg\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.jpeg" "" "JPEG Image File"
    WriteRegStr HKCR "BobMediaPlayer.jpeg\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.jpeg\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.png" "" "PNG Image File"
    WriteRegStr HKCR "BobMediaPlayer.png\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.png\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.gif" "" "GIF Image File"
    WriteRegStr HKCR "BobMediaPlayer.gif\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.gif\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'

    WriteRegStr HKCR "BobMediaPlayer.bmp" "" "Bitmap Image File"
    WriteRegStr HKCR "BobMediaPlayer.bmp\DefaultIcon" "" "$INSTDIR\BobMediaPlayer.exe,0"
    WriteRegStr HKCR "BobMediaPlayer.bmp\shell\open\command" "" '"$INSTDIR\BobMediaPlayer.exe" "%1"'
SectionEnd

;Component descriptions
LangString DESC_SecMain ${LANG_ENGLISH} "Install the main BOB Media Player application"
LangString DESC_SecDesktop ${LANG_ENGLISH} "Create a desktop shortcut for quick access"
LangString DESC_SecStartMenu ${LANG_ENGLISH} "Add shortcuts to the Start Menu"
LangString DESC_SecFileAssoc ${LANG_ENGLISH} "Associate media files (MP3, MP4, AVI, JPG, etc.) with BOB Media Player"

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecMain} $(DESC_SecMain)
  !insertmacro MUI_DESCRIPTION_TEXT ${SecDesktop} $(DESC_SecDesktop)
  !insertmacro MUI_DESCRIPTION_TEXT ${SecStartMenu} $(DESC_SecStartMenu)
  !insertmacro MUI_DESCRIPTION_TEXT ${SecFileAssoc} $(DESC_SecFileAssoc)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;Uninstaller Section
Section "Uninstall"
    ;Remove all files and directories (including .NET runtime)
    RMDir /r "$INSTDIR"

    ;Remove shortcuts
    Delete "$DESKTOP\BOB Media Player.lnk"
    RMDir /r "$SMPROGRAMS\BOB Media Player"

    ;Remove registry entries
    DeleteRegKey HKCU "Software\BOB Media Player"
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BOB Media Player"

    ;Remove file associations
    DeleteRegKey HKCR ".mp3"
    DeleteRegKey HKCR ".wav"
    DeleteRegKey HKCR ".m4a"
    DeleteRegKey HKCR ".wma"
    DeleteRegKey HKCR ".mp4"
    DeleteRegKey HKCR ".avi"
    DeleteRegKey HKCR ".mkv"
    DeleteRegKey HKCR ".mov"
    DeleteRegKey HKCR ".wmv"
    DeleteRegKey HKCR ".jpg"
    DeleteRegKey HKCR ".jpeg"
    DeleteRegKey HKCR ".png"
    DeleteRegKey HKCR ".gif"
    DeleteRegKey HKCR ".bmp"

    DeleteRegKey HKCR "BobMediaPlayer.mp3"
    DeleteRegKey HKCR "BobMediaPlayer.wav"
    DeleteRegKey HKCR "BobMediaPlayer.m4a"
    DeleteRegKey HKCR "BobMediaPlayer.wma"
    DeleteRegKey HKCR "BobMediaPlayer.mp4"
    DeleteRegKey HKCR "BobMediaPlayer.avi"
    DeleteRegKey HKCR "BobMediaPlayer.mkv"
    DeleteRegKey HKCR "BobMediaPlayer.mov"
    DeleteRegKey HKCR "BobMediaPlayer.wmv"
    DeleteRegKey HKCR "BobMediaPlayer.jpg"
    DeleteRegKey HKCR "BobMediaPlayer.jpeg"
    DeleteRegKey HKCR "BobMediaPlayer.png"
    DeleteRegKey HKCR "BobMediaPlayer.gif"
    DeleteRegKey HKCR "BobMediaPlayer.bmp"
SectionEnd

;Functions
Function .onInit
    ;No .NET runtime check needed - runtime is included in self-contained app
FunctionEnd
