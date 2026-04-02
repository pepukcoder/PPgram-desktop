# PPgram-desktop

## App Infrastructure

This section defines where PPgram stores user-visible and internal files on each OS.

### Linux

- App data root: `~/.local/share/PPgram`
- Downloads root: `~/Downloads/PPgram`
- Logs: `~/.local/share/PPgram/logs`
- Temp/cache: `~/.local/share/PPgram/cache`
- User settings: `~/.local/share/PPgram/settings.json`

### Windows

- App data root: `%LOCALAPPDATA%\PPgram`
- Downloads root: `%USERPROFILE%\Downloads\PPgram`
- Logs: `%LOCALAPPDATA%\PPgram\logs`
- Temp/cache: `%LOCALAPPDATA%\PPgram\cache`
- User settings: `%LOCALAPPDATA%\PPgram\settings.json`

### macOS

- App data root: `~/Library/Application Support/PPgram`
- Downloads root: `~/Downloads/PPgram`
- Logs: `~/Library/Application Support/PPgram/logs`
- Temp/cache: `~/Library/Application Support/PPgram/cache`
- User settings: `~/Library/Application Support/PPgram/settings.json`

## Conventions

- Create directories lazily on first use.
- Keep all app-owned files under the app data root except user downloads.
- Do not store persistent files in temporary OS directories.
- Logs are plain text and rotate by size/time.
- Settings are stored as a single JSON file at the path above.
