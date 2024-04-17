# WieBot 2.0

Discord bot with some random features.

Successor to [WieBot](https://github.com/Doppie23/WieBot)

## Dependencies

- Dotnet 8.0
- If running on a system that is not windows, change binaries in `lib/` to binaries compiled for your system.
- FFmpeg needs to be available on path

## Running

1. Rename `config.json.example` to `config.json` and fill in the values

1. DB needs to be created by either running `createDb.bat` or the following commands:

   ```bash
   cd WieBot2.0
   ```

   ```bash
   dotnet ef migrations add init
   ```

   ```bash
   dotnet ef database update
   ```

1. Run `dotnet run` to start the bot
