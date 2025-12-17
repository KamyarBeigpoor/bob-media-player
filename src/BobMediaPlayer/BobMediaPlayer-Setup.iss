#define MyAppName "BOB Media Player"
#define MyAppVersion "2.0-alpha"
#define MyAppPublisher "BOB Media Player"
#define MyAppURL "https://github.com/bobdevv/bob-media-player"
#define MyAppExeName "BobMediaPlayer.exe"
#define MyMusicAppName "BOB Music Player"
#define MyMusicAppExeName "BobMusicPlayer.exe"
#define MyAppIconName "icon.ico"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
AppId={{8B9C2D1E-4F5A-4B3C-9D8E-7F6A5B4C3D2E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\..\License.txt
OutputDir=..\..\Output
OutputBaseFilename=BobMediaPlayer-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=icon.ico
UninstallDisplayIcon={app}\{#MyAppIconName}

; Require Windows 10 or later
MinVersion=10.0

; No .NET runtime check needed - runtime is included in self-contained app
; PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdmin
Name: "associate"; Description: "Associate media files with {#MyAppName} (MP3, MP4, AVI, JPG, etc.)"; GroupDescription: "File Associations"

[Registry]
; File associations for media files
Root: HKCR; Subkey: ".mp3"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.mp3"; Flags: uninsdeletevalue; Tasks: associate

[Files]
Source: "bin\Release\net8.0-windows\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "icon.ico"; DestDir: "{app}"; DestName: "{#MyAppIconName}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppIconName}"; Comment: "Media player for videos and images"
Name: "{group}\{#MyMusicAppName}"; Filename: "{app}\{#MyMusicAppExeName}"; IconFilename: "{app}\{#MyAppIconName}"; Comment: "Music player with library management"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"; IconFilename: "{app}\{#MyAppIconName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppIconName}"; Comment: "Media player for videos and images"
Name: "{autodesktop}\{#MyMusicAppName}"; Filename: "{app}\{#MyMusicAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppIconName}"; Comment: "Music player with library management"

[Registry]
; File associations for media files - audio files go to BobMusicPlayer, video/image files to BobMediaPlayer
Root: HKCR; Subkey: ".mp3"; ValueType: string; ValueName: ""; ValueData: "BobMusicPlayer.mp3"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".wav"; ValueType: string; ValueName: ""; ValueData: "BobMusicPlayer.wav"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".m4a"; ValueType: string; ValueName: ""; ValueData: "BobMusicPlayer.m4a"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".wma"; ValueType: string; ValueName: ""; ValueData: "BobMusicPlayer.wma"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".mp4"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.mp4"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".avi"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.avi"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".mkv"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.mkv"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".mov"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.mov"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".wmv"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.wmv"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".jpg"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.jpg"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".jpeg"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.jpeg"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".png"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.png"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".gif"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.gif"; Flags: uninsdeletevalue; Tasks: associate
Root: HKCR; Subkey: ".bmp"; ValueType: string; ValueName: ""; ValueData: "BobMediaPlayer.bmp"; Flags: uninsdeletevalue; Tasks: associate

; Audio file type definitions for BobMusicPlayer
Root: HKCR; Subkey: "BobMusicPlayer.mp3"; ValueType: string; ValueName: ""; ValueData: "MP3 Audio File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMusicPlayer.mp3\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyMusicAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMusicPlayer.mp3\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyMusicAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMusicPlayer.wav"; ValueType: string; ValueName: ""; ValueData: "WAV Audio File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMusicPlayer.wav\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyMusicAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMusicPlayer.wav\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyMusicAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMusicPlayer.m4a"; ValueType: string; ValueName: ""; ValueData: "M4A Audio File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMusicPlayer.m4a\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyMusicAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMusicPlayer.m4a\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyMusicAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMusicPlayer.wma"; ValueType: string; ValueName: ""; ValueData: "WMA Audio File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMusicPlayer.wma\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyMusicAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMusicPlayer.wma\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyMusicAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.mp4"; ValueType: string; ValueName: ""; ValueData: "MP4 Video File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.mp4\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.mp4\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.avi"; ValueType: string; ValueName: ""; ValueData: "AVI Video File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.avi\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.avi\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.mkv"; ValueType: string; ValueName: ""; ValueData: "MKV Video File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.mkv\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.mkv\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.mov"; ValueType: string; ValueName: ""; ValueData: "QuickTime Video File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.mov\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.mov\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.wmv"; ValueType: string; ValueName: ""; ValueData: "Windows Media Video File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.wmv\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.wmv\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.jpg"; ValueType: string; ValueName: ""; ValueData: "JPEG Image File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.jpg\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.jpg\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.jpeg"; ValueType: string; ValueName: ""; ValueData: "JPEG Image File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.jpeg\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.jpeg\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.png"; ValueType: string; ValueName: ""; ValueData: "PNG Image File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.png\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.png\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.gif"; ValueType: string; ValueName: ""; ValueData: "GIF Image File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.gif\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.gif\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

Root: HKCR; Subkey: "BobMediaPlayer.bmp"; ValueType: string; ValueName: ""; ValueData: "Bitmap Image File"; Flags: uninsdeletekey; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.bmp\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associate
Root: HKCR; Subkey: "BobMediaPlayer.bmp\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent unchecked
Filename: "{app}\{#MyMusicAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyMusicAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent unchecked
