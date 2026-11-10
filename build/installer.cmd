@echo off
cls
Title Creating MediaPortal MyLyrics Installer

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

:: Get version from DLL
FOR /F "tokens=*" %%i IN ('..\Tools\Tools\sigcheck.exe /accepteula /nobanner /n "..\source\My Lyrics\bin\Release\MyLyrics.dll"') DO (SET version=%%i)

:: Temp xmp2 file
copy ..\MPEI\MyLyrics.xmp2 ..\MPEI\MyLyricsTemp.xmp2

:: Build MPE1
CD ..\MPEI
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" MyLyricsTemp.xmp2 /B /V=%version% /UpdateXML
CD ..\build

:: Cleanup
del ..\MPEI\MyLyricsTemp.xmp2
