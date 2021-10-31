@echo off


call :clean_all
goto :display_finish_message


:clean_all
	if exist .vs (
		rmdir /S /Q .vs
	)
	if exist Scrupdate\obj (
		rmdir /S /Q Scrupdate\obj
	)
	if exist Scrupdate\bin (
		rmdir /S /Q Scrupdate\bin
	)
	if exist Installer\src\tmp (
		rmdir /S /Q Installer\src\tmp
	)
	if exist Installer\bin\tmp (
		rmdir /S /Q Installer\bin\tmp
	)
	if exist Installer\bin (
		rmdir /S /Q Installer\bin
	)
	exit /b 0

:display_finish_message
	echo ---------------------------------------------------------------------------------------------------------------
	echo Clean Was Done!
	echo ---------------------------------------------------------------------------------------------------------------
	pause
	exit /b 0
