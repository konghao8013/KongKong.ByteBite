@echo off
setlocal
cd /d "%~dp0.."

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0deploy-server.ps1"
set EXIT_CODE=%ERRORLEVEL%

echo.
if "%EXIT_CODE%"=="0" (
  echo Server Docker deployment completed. Open http://39.97.243.210:8881/
) else (
  echo Server Docker deployment failed. Exit code: %EXIT_CODE%
)
echo.
pause
exit /b %EXIT_CODE%
