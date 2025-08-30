# PowerShell script to quickly test the Maestro PowerToys plugin
# 1. Kill running PowerToys instance
# 2. Build the plugin and copy output to PowerToys Run Plugins folder
# 3. Start PowerToys

$ErrorActionPreference = 'Stop'

# Kill PowerToys if running
Get-Process PowerToys -ErrorAction SilentlyContinue | ForEach-Object { $_.Kill() }

# Build the plugin and output to the PowerToys Run Plugins folder
$pluginOutput = "C:\Users\prvysoky\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\Maestro"
dotnet build -c Release -o "$pluginOutput"

# Start PowerToys
Start-Process "C:\Program Files\PowerToys\PowerToys.exe"
