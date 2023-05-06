@echo off
SET NAME=SSDDRM
SET DB=db.bin
SET EXE1=SSDDRM-service.exe
SET EXE2=publish\SSDDRM-service.exe
SET EXE_PATH=%~dp0%EXE1%
IF EXIST %DB% XCOPY %DB% %PROGRAMDATA%\%NAME%\
IF EXIST %EXE_PATH% GOTO install
SET EXE_PATH=%~dp0%EXE2%
IF EXIST %EXE_PATH% GOTO install
GOTO fail

:install
sc.exe query %NAME%
IF %ERRORLEVEL% == 1060 GOTO create
sc.exe stop %NAME%
sc.exe delete %NAME%
:create
sc.exe create %NAME% binpath=%EXE_PATH% start= auto
sc.exe start %NAME%
ECHO DONE!!!
GOTO end

:fail
echo FAILURE -- Could not find service exe file !!!

:end
PAUSE