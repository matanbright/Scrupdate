@echo off

set MAKENSIS_PATH=


call :clean_all
call :check_prerequisites_and_initialize_variables || (call :clean_all & goto :display_failure_message)
dotnet.exe build -c Release -p:Platform=x86 . || (call :clean_all & goto :display_failure_message)
dotnet.exe build -c Release -p:Platform=x64 . || (call :clean_all & goto :display_failure_message)
if not exist Installer\src\tmp (
	mkdir Installer\src\tmp || (call :clean_all & goto :display_failure_message)
)
if not exist Installer\bin\tmp (
	mkdir Installer\bin\tmp || (call :clean_all & goto :display_failure_message)
)
copy Installer\src\* Installer\src\tmp || (call :clean_all & goto :display_failure_message)
python.exe Installer\tools\scripts\insert_file_commands_into_nsi_script.py Installer\included_files Installer\src\scrupdate_installer.nsi || (call :clean_all & goto :display_failure_message)
python.exe Installer\tools\scripts\insert_file_commands_into_nsi_script.py Installer\included_files Installer\src\scrupdate_system_auto_installer.nsi || (call :clean_all & goto :display_failure_message)
python.exe Installer\tools\scripts\insert_file_commands_into_nsi_script.py Installer\included_files Installer\src\scrupdate_user_auto_installer.nsi || (call :clean_all & goto :display_failure_message)
%MAKENSIS_PATH% Installer\src\scrupdate_system_auto_installer.nsi || (call :clean_all & goto :display_failure_message)
%MAKENSIS_PATH% Installer\src\scrupdate_user_auto_installer.nsi || (call :clean_all & goto :display_failure_message)
%MAKENSIS_PATH% Installer\src\scrupdate_installer.nsi || (call :clean_all & goto :display_failure_message)
move /Y Installer\src\tmp\* Installer\src || (call :clean_all & goto :display_failure_message)
call :clean_temporary_files
goto :display_success_message


:check_prerequisites_and_initialize_variables
	where dotnet > nul 2>&1 || (echo ERROR: '.NET SDK' Was Not Found! & exit /b -1)
	where python > nul 2>&1 || (echo ERROR: 'Python' Was Not Found! & exit /b -1)
	if exist "C:\Program Files (x86)\NSIS\Bin\makensis.exe" (
		set MAKENSIS_PATH="C:\Program Files (x86)\NSIS\Bin\makensis.exe"
	) else (
		if exist "C:\Program Files\NSIS\Bin\makensis.exe" (
			set MAKENSIS_PATH="C:\Program Files\NSIS\Bin\makensis.exe"
		) else (
			echo ERROR: 'NSIS' Was Not Found! & exit /b -1
		)
	)
	exit /b 0

:clean_temporary_files
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
	exit /b 0

:clean_all
	call :clean_temporary_files
	if exist Installer\bin (
		rmdir /S /Q Installer\bin
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
	echo Build Was Succeeded! [Check output in the 'Installer\bin' directory]
	echo ---------------------------------------------------------------------------------------------------------------
	pause
	exit /b 0
