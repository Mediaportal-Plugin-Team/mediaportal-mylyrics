@echo off
cls
Title Building MediaPortal MyLyrics (RELEASE)
cd ..

cd source

setlocal enabledelayedexpansion

:: Prepare version
for /f "tokens=*" %%a in ('git rev-list HEAD --count') do set REVISION=%%a 
set REVISION=%REVISION: =%
"..\Tools\Tools\sed.exe" -i "s/\.[0-9]\+\")/.%REVISION%\")/" LyricsEngine\Properties\AssemblyInfo.cs
"..\Tools\Tools\sed.exe" -i "s/\.[0-9]\+\")/.%REVISION%\")/" "My Lyrics\Properties\AssemblyInfo.cs"
"..\Tools\Tools\sed.exe" -i "s/\.[0-9]\+\")/.%REVISION%\")/" TranslateProvider\Properties\AssemblyInfo.cs

:: Build
FOR %%p IN ("%PROGRAMFILES(x86)%" "%PROGRAMFILES%") DO (
  FOR %%s IN (2019 2022) DO (
    FOR %%e IN (Community Professional Enterprise BuildTools) DO (
      SET PF=%%p
      SET PF=!PF:"=!
      SET MSBUILD_PATH="!PF!\Microsoft Visual Studio\%%s\%%e\MSBuild\Current\Bin\MSBuild.exe"
      IF EXIST "!MSBUILD_PATH!" GOTO :BUILD
    )
  )
)

:BUILD

:: Build
%MSBUILD_PATH% /target:Rebuild /property:Configuration=RELEASE /fl /flp:logfile=MyLyrics.log;verbosity=diagnostic MyLyrics.sln

:: Revert version
git checkout LyricsEngine\Properties\AssemblyInfo.cs
git checkout "My Lyrics\Properties\AssemblyInfo.cs"
git checkout TranslateProvider\Properties\AssemblyInfo.cs

cd ..\build

pause

