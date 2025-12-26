@echo off
setlocal EnableExtensions EnableDelayedExpansion

:: Repository root (directory of this script)
set "ROOT=%~dp0"
:: Normalize root without trailing backslash
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"
echo === start.bat ===
echo Root: "%ROOT%"

:: Clear vars
set "CONSOLE="
set "UI="
set "BUILDFAILED=0"

echo Searching for existing executables under "%ROOT%"...
for /r "%ROOT%" %%F in (RqSimConsole.exe) do (
    echo Found possible console: "%%~fF"
    set "CONSOLE=%%~fF"
    goto :foundConsole
)
:foundConsole
for /r "%ROOT%" %%F in (RqSimUI.exe) do (
    echo Found possible UI: "%%~fF"
    set "UI=%%~fF"
    goto :foundUI
)
:foundUI

:: If both found, skip build
if defined CONSOLE if defined UI (
    echo Both executables found. Skipping build.
) else (
    echo One or more executables missing. Building projects in Release...

    :: List of project files to try building (absolute or relative to repo root)
    set "PROJECTS[0]=%ROOT%\RqSimUI\RqSimUI.csproj"
    set "PROJECTS[1]=%ROOT%\Dx12WinForm\Dx12WinForm.csproj"
    set "PROJECTS[2]=%ROOT%\RqSim.PluginManager.UI\PhysxPluginsForm.csproj"
    set "PROJECTS[3]=%ROOT%\RqSimConsole\RqSimConsole.csproj"
    set "PROJECTS[4]=%ROOT%\RqSimEngineApi\RqSimEngineApi.csproj"
    set "PROJECTS[5]=%ROOT%\RqSimGraphEngine\RqSimGraphEngine.csproj"
    set "PROJECTS[6]=%ROOT%\RqSimRenderingEngine\RqSimRenderingEngine.csproj"
    set "PROJECTS[7]=%ROOT%\RqSimRenderingEngine.Abstractions\RqSimRenderingEngine.Abstractions.csproj"

    rem Try to build each project if it exists
    for /L %%i in (0,1,7) do (
        call set "P=%%PROJECTS[%%i]%%"
        if defined P (
            if exist "%%~P" (
                echo Building project "%%~P"...
                dotnet build "%%~P" -c Release || set "BUILDFAILED=1"
            ) else (
                echo Project not found: "%%~P"
            )
        )
    )

    if "%BUILDFAILED%"=="1" (
        echo Build failed. Aborting.
        pause
        endlocal
        exit /b 1
    )

    echo Build finished. Re-searching for executables...
    if not defined CONSOLE (
        for /r "%ROOT%" %%F in (RqSimConsole.exe) do (set "CONSOLE=%%~fF" & goto :afterSearchConsole)
    )
    :afterSearchConsole
    if not defined UI (
        for /r "%ROOT%" %%F in (RqSimUI.exe) do (set "UI=%%~fF" & goto :afterSearchUI)
    )
    :afterSearchUI
)

echo Final paths:
echo   CONSOLE=%CONSOLE%
echo   UI=%UI%

:: Start Console if found
if defined CONSOLE (
    for %%I in ("%CONSOLE%") do set "CONSOLEDIR=%%~dpI"
    echo Starting RqSimConsole in new window, working dir: "%CONSOLEDIR%"
    start "RqSimConsole" /D "%CONSOLEDIR%" "%CONSOLE%"
) else (
    echo RqSimConsole executable not found.
)

:: Start UI and wait for it to exit
if defined UI (
    for %%I in ("%UI%") do set "UIDIR=%%~dpI"
    echo Starting RqSimUI and waiting for exit, working dir: "%UIDIR%"
    start "RqSimUI" /D "%UIDIR%" /WAIT "%UI%"
) else (
    echo RqSimUI executable not found.
)

echo All done.
pause
endlocal
