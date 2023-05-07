@echo off
SET NAME=SSDDRM

:uninstall
sc.exe query %NAME%
IF %ERRORLEVEL% == 1060 GOTO end
sc.exe stop %NAME%
FOR /F "tokens=3" %%A IN ('sc.exe queryex %NAME% ^| findstr PID') DO (SET pid=%%A)
 IF %pid% NEQ 0 (
  taskkill /F /PID %pid%
 )
sc.exe delete %NAME%
RMDIR /S /Q %PROGRAMDATA%\%NAME%\

:end
ECHO DONE!!!
PAUSE