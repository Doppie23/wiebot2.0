@echo off

if [%1] == [] (
    @echo Error: Missing argument. Please provide a migration name.
    @echo Usage: "createDb.bat <migrationName>"
    exit /b 1
)

cd WieBot2.0
dotnet ef migrations add %1
dotnet ef database update
