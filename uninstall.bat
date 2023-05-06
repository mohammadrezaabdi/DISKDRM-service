@echo off
SET NAME=SSDDRM

:uninstall
sc.exe query %NAME%
IF %ERRORLEVEL% == 1060 GOTO end
sc.exe stop %NAME%
sc.exe delete %NAME%

:end
IF EXIST %PROGRAMDATA%\%NAME%\ RMDIR /S /Q %PROGRAMDATA%\%NAME%\
ECHO DONE!!!
PAUSE