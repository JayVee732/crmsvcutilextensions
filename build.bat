@echo off
cls

if not exist tools\nuget.exe (
	echo Downloading 'NuGet'
	mkdir tools
    PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '.\download-nuget.ps1'"
)

dotnet tool install fake-cli --tool-path "packages\fake-cli"
"packages\fake-cli\fake.exe" run build.fsx