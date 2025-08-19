@echo off
setlocal

:: Cambia al directorio donde está el script si es necesario
cd /d %~dp0

:: Ejecutar el script PowerShell con bypass de políticas
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "run_bot.ps1"


