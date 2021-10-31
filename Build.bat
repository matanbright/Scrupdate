@echo off


call :clean_all
call :check_prerequisites || (call :clean_all & goto :display_failure_message)
dotnet.exe build -c Release -p:Platform=x86 . || (call :clean_all & goto :display_failure_message)
dotnet.exe build -c Release -p:Platform=x64 . || (call :clean_all & goto :display_failure_message)
call :clean_temporary_files
goto :display_success_message


:check_prerequisites
	where dotnet > nul 2>&1 || (echo ERROR: '.NET SDK' Was Not Found! & exit /b -1)
	exit /b 0

:clean_temporary_files
	if exist .vs (
		rmdir /S /Q .vs
	)
	if exist Scrupdate\obj (
		rmdir /S /Q Scrupdate\obj
	)
	exit /b 0

:clean_all
	call :clean_temporary_files
	if exist Scrupdate\bin (
		rmdir /S /Q Scrupdate\bin
	)
	exit /b 0

:display_failure_message
	echo ---------------------------------------------------------------------------------------------------------------
	echo Build Was Failed!
	echo ---------------------------------------------------------------------------------------------------------------
	pause
	exit /b 0

:display_success_message
	echo ---------------------------------------------------------------------------------------------------------------
	echo Build Was Succeeded! [Check output in the 'Scrupdate\bin' directory]
	echo ---------------------------------------------------------------------------------------------------------------
	pause
	exit /b 0
