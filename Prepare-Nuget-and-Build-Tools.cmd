@echo off

rem USAGE:
rem    call Prepare-Nuget-and-Build-Tools.cmd 
rem    "%BUILD_TOOLS_ONLINE%\nuget.exe" restore MySolution1.sln
rem    "%BUILD_TOOLS_ONLINE%\nuget.exe" restore MySolution2.sln
rem    "%MS_BUILD_2017%" MySolution1.sln /t:Rebuild /v:m /p:Configuration=Debug
rem    "%MS_BUILD_2017%" MySolution2.sln /t:Rebuild /v:m /p:Configuration=Release
rem    "%NUNIT_CONSOLE_RUNNER%" --labels=On --workers=1 --work=path/to/output path/to-binary.Tests.dll

set BUILD_TOOLS_ONLINE=%LOCALAPPDATA%\Temp\build-tools-online.tmp
mkdir "%BUILD_TOOLS_ONLINE%" 1>nul 2>&1

REM *** DOWNLOAD LATEST NUGET ***
setlocal
set url=https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
set outfile=%BUILD_TOOLS_ONLINE%\nuget.exe
echo Downloading "nuget.exe" into [%BUILD_TOOLS_ONLINE%\nuget.exe]
echo [System.Net.ServicePointManager]::ServerCertificateValidationCallback={$true}; $d=new-object System.Net.WebClient; $d.DownloadFile("$Env:url","$Env:outfile") | powershell -command -
endlocal

REM *** DOWNLOAD LATEST vswhere ***
pushd "%BUILD_TOOLS_ONLINE%"
del nunit.log 1>nul 2>&1
rem for %%p in (vswhere NUnit.ConsoleRunner NUnit.Extension.NUnitProjectLoader NUnit.Extension.NUnitV2Driver NUnit.Extension.NUnitV2ResultWriter NUnit.Extension.TeamCityEventListener NUnit.Extension.VSProjectLoader) DO (
for %%p in (vswhere NUnit.Extension.NUnitV2Driver NUnit.Extension.NUnitV2ResultWriter ReportUnit) DO (
  echo Installing %%p into temp folder
  nuget.exe install %%p 1>nunit.log 2>&1
)
cd vswhere*\tools
xcopy /y *.* ..\.. 1>nul
popd


rem Assign NUNIT_CONSOLE_RUNNER
pushd "%BUILD_TOOLS_ONLINE%"
cd NUnit.ConsoleRunner*\tools
dir /b /s nunit*-console.exe > exe-name.txt
for /F %%v in (exe-name.txt) DO set NUNIT_CONSOLE_RUNNER=%%v
popd
Echo NUnit Console Runner (var NUNIT_CONSOLE_RUNNER): [%NUNIT_CONSOLE_RUNNER%]

REM *** Look for latest MSBUILD ***
pushd "%BUILD_TOOLS_ONLINE%"
echo vswhere Dir: %CD%
vswhere.exe -latest -products * -requires Microsoft.Component.MSBuild -property installationPath > VS_InstallDir.txt
set VS_2017_InstallDir=
for /F "delims=" %%v in (VS_InstallDir.txt) DO set VS_2017_InstallDir=%%v
echo VS Installation Path (var VS_2017_InstallDir): [%VS_2017_InstallDir%]
set MS_BUILD_2017=
if exist "%VS_2017_InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" set MS_BUILD_2017=%VS_2017_InstallDir%\MSBuild\15.0\Bin\MSBuild.exe
popd
echo MS Build (var MS_BUILD_2017): [%MS_BUILD_2017%]

@echo on