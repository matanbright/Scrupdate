!addplugindir ..\plugins\x86-unicode
!include LogicLib.nsh
!include FileFunc.nsh
!include x64.nsh
!include MUI2.nsh
!include ..\include\definitions.nsh

Var createDesktopShortcut
Var createStartMenuShortcut
Var removeAllUserDataCreatedByTheProgram
Var installationWasFailed
Var uninstallationWasFailed

!macro InitializeGlobalVariables
	Unicode true
	RequestExecutionLevel admin
	Name "${PROGRAM_NAME}"
	OutFile "..\bin\tmp\${SYSTEM_AUTO_INSTALLER_NAME}"
	Caption "${INSTALLER_CAPTION}"
	UninstallCaption "${UNINSTALLER_CAPTION}"
	!define MUI_ICON "..\resources\icons\installer_icon.ico"
	!define MUI_UNICON "..\resources\icons\installer_icon.ico"
	VIProductVersion "${INSTALLER_VERSION}"
	VIAddVersionKey /LANG=0 "ProductName" "${PROGRAM_NAME}"
	VIAddVersionKey /LANG=0 "ProductVersion" "${PROGRAM_VERSION}"
	VIAddVersionKey /LANG=0 "FileVersion" "${INSTALLER_VERSION}"
	VIAddVersionKey /LANG=0 "FileDescription" "${INSTALLER_CAPTION}"
	VIAddVersionKey /LANG=0 "LegalCopyright" "${PROGRAM_COPYRIGHT}"
	!define MUI_WELCOMEFINISHPAGE_BITMAP "..\resources\images\welcome_page_image.bmp"
	!define MUI_UNWELCOMEFINISHPAGE_BITMAP "..\resources\images\welcome_page_image.bmp"
!macroend

!macro InitializeInstallerInstfilesPage
	!insertmacro MUI_PAGE_INSTFILES
!macroend

!macro InitializeInstallerFinishPage
	!define MUI_FINISHPAGE_TITLE_3LINES
	!define MUI_PAGE_CUSTOMFUNCTION_SHOW OnFinishPageShow
	!insertmacro MUI_PAGE_FINISH
	Function OnFinishPageShow
		${If} $installationWasFailed == true
			SendMessage $mui.FinishPage.Title ${WM_SETTEXT} 0 "STR:${PROGRAM_NAME} Installation Has Been Failed"
			SendMessage $mui.FinishPage.Text ${WM_SETTEXT} 0 "STR:${PROGRAM_NAME} has not been installed on your computer.$\r$\n$\r$\nClick 'Finish' to close the installation wizard."
		${Else}
			SendMessage $mui.FinishPage.Title ${WM_SETTEXT} 0 "STR:${PROGRAM_NAME} Installation Has Been Completed"
			SendMessage $mui.FinishPage.Text ${WM_SETTEXT} 0 "STR:${PROGRAM_NAME} has been installed on your computer.$\r$\n$\r$\nClick 'Finish' to close the installation wizard."
		${EndIf}
	FunctionEnd
!macroend

!macro InitializeUninstallerWelcomePage
	!define MUI_WELCOMEPAGE_TITLE "Welcome to ${PROGRAM_NAME} Uninstaller"
	!define MUI_WELCOMEPAGE_TEXT "This wizard will uninstall ${PROGRAM_NAME} from your computer."
	!insertmacro MUI_UNPAGE_WELCOME
!macroend

!macro InitializeUninstallerUninstallOptionsPage
	UninstPage custom un.OnUninstallOptionsPageShow un.OnUninstallOptionsPageLeave
	Var dialog_uninstallOptionsSelection
	Var label_additionalOptions
	Var checkBox_removeAllUserDataCreatedByTheProgram
	Var label_removeAllUserDataCreatedByTheProgramNote
	Var label_uninstallationNote
	Function un.OnUninstallOptionsPageShow
		!insertmacro MUI_HEADER_TEXT "Choose Uninstall Options" "Choose additional options."
		nsDialogs::Create 1018
		Pop $dialog_uninstallOptionsSelection
		${NSD_CreateLabel} 0 0 100% 6% "Additional options:"
		Pop $label_additionalOptions
		${NSD_CreateCheckBox} 0 15% 100% 6% "Remove all user data created by ${PROGRAM_NAME}"
		Pop $checkBox_removeAllUserDataCreatedByTheProgram
		${NSD_CreateLabel} 4% 25% 96% 18% "Please note: The data will be removed only for the current user.$\r$\nOther users will have to manually remove ${PROGRAM_NAME}'s data files from the AppData folder if they were created."
		Pop $label_removeAllUserDataCreatedByTheProgramNote
		${NSD_CreateLabel} 0 75% 100% 18% "Please note: The scheduled task created by ${PROGRAM_NAME} will be removed only for the current user. Other users will have to manually remove the scheduled task from Windows Task Scheduler if it was created."
		Pop $label_uninstallationNote
		${If} $removeAllUserDataCreatedByTheProgram == true
			${NSD_SetState} $checkBox_removeAllUserDataCreatedByTheProgram ${BST_CHECKED}
		${Else}
			${NSD_SetState} $checkBox_removeAllUserDataCreatedByTheProgram ${BST_UNCHECKED}
		${EndIf}
		GetDlgItem $1 $HWNDPARENT 1
		SendMessage $1 ${BCM_SETSHIELD} 0 0
		nsDialogs::Show
	FunctionEnd
	Function un.OnUninstallOptionsPageLeave
		${NSD_GetState} $checkBox_removeAllUserDataCreatedByTheProgram $0
		${If} $0 == ${BST_CHECKED}
			StrCpy $removeAllUserDataCreatedByTheProgram true
		${Else}
			StrCpy $removeAllUserDataCreatedByTheProgram false
		${EndIf}
	FunctionEnd
!macroend

!macro InitializeUninstallerConfirmPage
	!define MUI_UNCONFIRMPAGE_TEXT_TOP "${PROGRAM_NAME} will be uninstalled from the following folder. Click 'Uninstall' to start the uninstallation."
	!insertmacro MUI_UNPAGE_CONFIRM
!macroend

!macro InitializeUninstallerInstfilesPage
	!insertmacro MUI_UNPAGE_INSTFILES
!macroend

!macro InitializeUninstallerFinishPage
	!define MUI_FINISHPAGE_TITLE_3LINES
	!define MUI_PAGE_CUSTOMFUNCTION_SHOW un.OnFinishPageShow
	!insertmacro MUI_UNPAGE_FINISH
	Function un.OnFinishPageShow
		${If} $uninstallationWasFailed == true
			SendMessage $mui.FinishPage.Title ${WM_SETTEXT} 0 "STR:${PROGRAM_NAME} Uninstallation Has Been Failed"
			SendMessage $mui.FinishPage.Text ${WM_SETTEXT} 0 "STR:${PROGRAM_NAME} has not been uninstalled from your computer.$\r$\n$\r$\nClick 'Finish' to close the uninstallation wizard."
		${Else}
			SendMessage $mui.FinishPage.Title ${WM_SETTEXT} 0 "STR:${PROGRAM_NAME} Uninstallation Has Been Completed"
			SendMessage $mui.FinishPage.Text ${WM_SETTEXT} 0 "STR:${PROGRAM_NAME} has been uninstalled from your computer.$\r$\n$\r$\nClick 'Finish' to close the uninstallation wizard."
		${EndIf}
	FunctionEnd
!macroend

!macro SetUserInterfaceLanguage Language
	!insertmacro MUI_LANGUAGE "${Language}"
!macroend



!insertmacro InitializeGlobalVariables
!insertmacro InitializeInstallerInstfilesPage
!insertmacro InitializeInstallerFinishPage
!insertmacro InitializeUninstallerWelcomePage
!insertmacro InitializeUninstallerUninstallOptionsPage
!insertmacro InitializeUninstallerConfirmPage
!insertmacro InitializeUninstallerInstfilesPage
!insertmacro InitializeUninstallerFinishPage
!insertmacro SetUserInterfaceLanguage "English"

Function .onInit
	BringToFront
	SetShellVarContext all
	SetOutPath "$TEMP"
	ClearErrors
	${GetOptions} "$CMDLINE" "${INSTALLER_ARGUMENT__INSTALL_LOCATION}" $0
	${If} ${Errors}
		Quit
	${EndIf}
	${If} "$0" != ""
		StrCpy $INSTDIR "$0"
	${EndIf}
	ClearErrors
	${GetOptions} "$CMDLINE" "${INSTALLER_ARGUMENT__CREATE_DESKTOP_SHORTCUT}" $0
	${If} ${Errors}
		StrCpy $createDesktopShortcut false
	${Else}
		StrCpy $createDesktopShortcut true
	${EndIf}
	ClearErrors
	${GetOptions} "$CMDLINE" "${INSTALLER_ARGUMENT__CREATE_START_MENU_SHORTCUT}" $0
	${If} ${Errors}
		StrCpy $createStartMenuShortcut false
	${Else}
		StrCpy $createStartMenuShortcut true
	${EndIf}
	StrCpy $installationWasFailed false
FunctionEnd

Function .onGUIEnd
	RMDir /r "${TEMPORARY_INSTALLER_EXTRACTION_LOCATION}"
	SelfDel::Del /RMDIR
FunctionEnd

Function un.onInit
	SetShellVarContext all
	SetOutPath "$TEMP"
	StrCpy $removeAllUserDataCreatedByTheProgram false
	StrCpy $uninstallationWasFailed false
FunctionEnd

Function un.onGUIEnd
	RMDir /r "$EXEDIR"
	SelfDel::Del /RMDIR
FunctionEnd

Section
	CreateDirectory "$INSTDIR"
	ClearErrors
	FileOpen $0 "$INSTDIR\temp.tmp" w
	FileClose $0
	Delete "$INSTDIR\temp.tmp"
	${If} ${Errors}
		StrCpy $installationWasFailed true
		MessageBox MB_OK|MB_ICONSTOP "Error: The installation wizard doesn't have a permission to write to the selected location!"
	${Else}
		GetTempFileName $0
		Delete "$0"
		ClearErrors
		Rename "$INSTDIR" "$0"
		Rename "$0" "$INSTDIR"
		${If} ${Errors}
			StrCpy $installationWasFailed true
			MessageBox MB_OK|MB_ICONSTOP "Error: Unable to proceed with the installation! There are files from a previous installation that are still being used."
		${Else}
			CopyFiles /SILENT "${TEMPORARY_INSTALLER_FILES_EXTRACTION_LOCATION}\*" "$INSTDIR"
			ExecWait "$INSTDIR\${PROGRAM_NAME} ${PROGRAM_ARGUMENT__UPDATE_SCHEDULED_TASK_AND_CLOSE}"
			WriteUninstaller "${UNINSTALLER_NAME}"
			WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\${PROGRAM_NAME}.exe" "" "$INSTDIR\${PROGRAM_NAME}.exe"
			WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\${PROGRAM_NAME}.exe" "Path" "$INSTDIR"
			WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "DisplayName" "${PROGRAM_NAME}"
			WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "DisplayIcon" "$INSTDIR\${PROGRAM_NAME}.exe"
			WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "DisplayVersion" "${PROGRAM_VERSION}"
			WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "Publisher" "${PROGRAM_PUBLISHER}"
			WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "UninstallString" "$INSTDIR\${UNINSTALLER_NAME}"
			${If} $createDesktopShortcut == true
				CreateShortCut "$DESKTOP\${PROGRAM_NAME}.lnk" "$INSTDIR\${PROGRAM_NAME}.exe"
			${EndIf}
			${If} $createStartMenuShortcut == true
				CreateShortCut "$STARTMENU\Programs\${PROGRAM_NAME}.lnk" "$INSTDIR\${PROGRAM_NAME}.exe"
			${EndIf}
			System::Call "shell32.dll::SHChangeNotify(i, i, i, i) v(0x08000000, 0, 0, 0)"
		${EndIf}
	${EndIf}
SectionEnd

Section "uninstall"
	ClearErrors
	FileOpen $0 "$INSTDIR\temp.tmp" w
	FileClose $0
	Delete "$INSTDIR\temp.tmp"
	${If} ${Errors}
		StrCpy $uninstallationWasFailed true
		MessageBox MB_OK|MB_ICONSTOP "Error: The installation wizard doesn't have a permission to write to the current location!"
	${Else}
		GetTempFileName $0
		Delete "$0"
		ClearErrors
		Rename "$INSTDIR" "$0"
		Rename "$0" "$INSTDIR"
		${If} ${Errors}
			StrCpy $uninstallationWasFailed true
			MessageBox MB_OK|MB_ICONSTOP "Error: Unable to proceed with the uninstallation! There are files that are still being used."
		${Else}
			${If} $removeAllUserDataCreatedByTheProgram == true
				ExecWait "$INSTDIR\${PROGRAM_NAME} ${PROGRAM_ARGUMENT__RESET_ALL_AND_CLOSE}"
			${Else}
				ExecWait "$INSTDIR\${PROGRAM_NAME} ${PROGRAM_ARGUMENT__RESET_SCHEDULED_TASK_AND_CLOSE}"
			${EndIf}
			DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\${PROGRAM_NAME}.exe"
			DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}"
			${If} ${RunningX64}
				~[INSERT_64BIT_FILES_REMOVAL_COMMANDS_HERE]~ # This line will be replaced by an external script while building the installer
			${Else}
				~[INSERT_32BIT_FILES_REMOVAL_COMMANDS_HERE]~ # This line will be replaced by an external script while building the installer
			${EndIf}
			~[INSERT_COMMON_FILES_REMOVAL_COMMANDS_HERE]~ # This line will be replaced by an external script while building the installer
			Delete "$INSTDIR\${UNINSTALLER_NAME}"
			RMDir "$INSTDIR"
			Delete "$DESKTOP\${PROGRAM_NAME}.lnk"
			Delete "$STARTMENU\Programs\${PROGRAM_NAME}.lnk"
			System::Call "shell32.dll::SHChangeNotify(i, i, i, i) v(0x08000000, 0, 0, 0)"
		${EndIf}
	${EndIf}
SectionEnd
