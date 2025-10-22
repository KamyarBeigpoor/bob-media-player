# Bob Media Player

A modern, dark-themed media player for Windows built with C# and WPF. Plays audio, video, and images with a clean UI and handy keyboard shortcuts.

## Features

- **Audio playback**: MP3, WAV, M4A, WMA
- **Video playback**: MP4, AVI, MKV, MOV, WMV
- **Image viewing**: JPG, PNG, GIF, BMP
- **Compact audio window**: Separate audio player with title/artist, album art (via TagLib), and playlist controls
- **Playlist navigation**: Auto-detects media in the current directory, Previous/Next buttons
- **Seeking & timeline**: Click/drag to seek with a responsive progress slider
- **Subtitles (.srt)**: Auto-load matching `.srt` files and manual load option
- **Fullscreen video**: Auto-hide controls and cursor, clean overlay UI (OSD for feedback)
- **Playback controls**: Play/Pause, Stop, Jump ±10s, Speed control (0.25x–2.0x)
- **Volume & mute**: Slider and quick mute toggle
- **Keyboard shortcuts**: Space, Left/Right, Up/Down, M, F11, +/−, Esc
- **Drag & drop**: Drop a media file onto the window to play
- **File association friendly**: Opening an audio file launches the compact audio window directly (no main window flash)

## How to Run

```bash
dotnet restore
dotnet build
dotnet run
```

## Usage

1. Click "Open Media File" or drag and drop a media file
2. Audio files open in the compact audio window (with metadata and album art if available)
3. Video files play in the main window; images display in the main window
4. Use Play/Pause, Stop, timeline seeking, volume/mute, and Next/Previous (for directory playlists)
5. For videos, press F11 for fullscreen; subtitles are auto-detected from matching `.srt`

## Requirements

- .NET 8.0 or higher
- Windows OS

## Keyboard Shortcuts

- **Space**: Play/Pause (video)
- **Left/Right**: Seek −/+10s (video)
- **Up/Down**: Volume −/+5% (video)
- **M**: Toggle mute
- **F11**: Toggle fullscreen (video/image)
- **Esc**: Exit fullscreen
- **+ / −**: Increase/Decrease playback speed (video)

## File Associations

- When launched by double-clicking an audio file (e.g., `.mp3`), the app opens directly in the compact audio window without flashing the main window.
- For other media types (video/image), the main window is used.

## Tech Stack

- **Framework**: .NET 8, WPF
- **Audio**: NAudio (`WaveOutEvent`, `AudioFileReader`)
- **Metadata/Album Art**: TagLib#

## Troubleshooting

- If subtitles do not appear, ensure an `.srt` file with the same base name as the video is in the same folder, or load it manually from the Subtitles button.
- For playlist navigation, files are sourced from the current file's directory using common media extensions.
