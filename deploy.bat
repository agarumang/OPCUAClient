@echo off
echo PDF Data Extractor - Deployment Script
echo =====================================
echo.

REM Build the application
echo Building application...
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

REM Create deployment folder
set DEPLOY_DIR=Deploy
if exist %DEPLOY_DIR% rmdir /s /q %DEPLOY_DIR%
mkdir %DEPLOY_DIR%

REM Copy executable and dependencies
echo Copying files to deployment folder...
copy "bin\Release\net48\*.exe" %DEPLOY_DIR%\
copy "bin\Release\net48\*.dll" %DEPLOY_DIR%\
copy "bin\Release\net48\*.json" %DEPLOY_DIR%\

REM Copy documentation
copy "FirstTimeSetup.md" %DEPLOY_DIR%\
copy "Configuration_Guide.md" %DEPLOY_DIR%\
copy "OPC_UA_Configuration.md" %DEPLOY_DIR%\

echo.
echo ✅ Deployment package created in %DEPLOY_DIR% folder
echo.
echo Files included:
dir %DEPLOY_DIR% /b
echo.
echo To deploy on a new machine:
echo 1. Copy the entire %DEPLOY_DIR% folder to the target machine
echo 2. Edit appsettings.json if needed (change OPC UA endpoint)
echo 3. Run FileReader.exe
echo.
echo For troubleshooting, run:
echo FileReader.exe --diagnostic
echo.
echo For manual setup, run:
echo FileReader.exe --setup
echo.
pause
