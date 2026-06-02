@echo off
setlocal
cd /d "%~dp0.."

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0deploy-local.ps1"
set EXIT_CODE=%ERRORLEVEL%

echo.
if "%EXIT_CODE%"=="0" (
  echo Local Docker deployment completed. Open http://localhost:8881/
) else (
  echo Local Docker deployment failed. Exit code: %EXIT_CODE%
)
echo.
pause
exit /b %EXIT_CODE%
