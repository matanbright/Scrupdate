!include LogicLib.nsh
!include x64.nsh
!include MUI2.nsh
!include nsDialogs.nsh
!include ..\include\definitions.nsh

Var installToAllUsers
Var createDesktopShortcut
Var createStartMenuShortcut

!macro InitializeGlobalVariables
	Unicode true
	RequestExecutionLevel user
	Name "${PROGRAM_NAME}"
	OutFile "..\bin\${INSTALLER_NAME}"
	Caption "${INSTALLER_CAPTION}"
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

!macro InitializeInstallerWelcomePage
	!define MUI_WELCOMEPAGE_TITLE "Welcome to ${PROGRAM_NAME} Installer"
	!define MUI_WELCOMEPAGE_TEXT "This wizard will install ${PROGRAM_NAME} version ${PROGRAM_VERSION} on your computer."
	!insertmacro MUI_PAGE_WELCOME
!macroend

!macro InitializeInstallerLicensePage
	!define MUI_LICENSEPAGE_TEXT_TOP "Press the 'Page Down' key to see the rest of the agreement."
	!define MUI_LICENSEPAGE_TEXT_BOTTOM "If you accept the terms of the agreement, click 'I Agree' to continue. You must accept the agreement to install ${PROGRAM_NAME}."
	!insertmacro MUI_PAGE_LICENSE "..\..\LICENSE.txt"
!macroend

!macro InitializeInstallerInstallOptionsPage
	Page custom OnInstallOptionsPageShow OnInstallOptionsPageLeave
	Var dialog_installOptionsSelection
	Var label_installScope
	Var radioButton_installJustForMe
	Var radioButton_installForAnyoneUsingThisComputer
	Var label_additionalOptions
	Var checkBox_createDesktopShortcut
	Var checkBox_createStartMenuShortcut
	Function OnInstallOptionsPageShow
		!insertmacro MUI_HEADER_TEXT "Choose Install Options" "Choose install scope and additional options."
		nsDialogs::Create 1018
		Pop $dialog_installOptionsSelection
		${NSD_CreateLabel} 0 0 100% 6% "Select whether you want to install ${PROGRAM_NAME} for yourself or for all users of this computer."
		Pop $label_installScope
		${NSD_CreateAdditionalRadioButton} 0 15% 100% 6% "Install just for me (recommended)"
		Pop $radioButton_installJustForMe
		${NSD_CreateFirstRadioButton} 0 25% 100% 6% "Install for anyone using this computer (requires administrator privileges)"
		Pop $radioButton_installForAnyoneUsingThisComputer
		${If} $installToAllUsers == true
			${NSD_SetState} $radioButton_installForAnyoneUsingThisComputer ${BST_CHECKED}
		${Else}
			${NSD_SetState} $radioButton_installJustForMe ${BST_CHECKED}
		${EndIf}
		${NSD_CreateLabel} 0 50% 100% 6% "Additional options:"
		Pop $label_additionalOptions
		${NSD_CreateCheckBox} 0 65% 100% 6% "Create desktop shortcut"
		Pop $checkBox_createDesktopShortcut
		${If} $createDesktopShortcut == true
			${NSD_SetState} $checkBox_createDesktopShortcut ${BST_CHECKED}
		${Else}
			${NSD_SetState} $checkBox_createDesktopShortcut ${BST_UNCHECKED}
		${EndIf}
		${NSD_CreateCheckBox} 0 75% 100% 6% "Create start-menu shortcut"
		Pop $checkBox_createStartMenuShortcut
		${If} $createStartMenuShortcut == true
			${NSD_SetState} $checkBox_createStartMenuShortcut ${BST_CHECKED}
		${Else}
			${NSD_SetState} $checkBox_createStartMenuShortcut ${BST_UNCHECKED}
		${EndIf}
		GetDlgItem $1 $HWNDPARENT 1
		SendMessage $1 ${BCM_SETSHIELD} 0 0
		nsDialogs::Show
	FunctionEnd
	Function OnInstallOptionsPageLeave
		${NSD_GetState} $radioButton_installForAnyoneUsingThisComputer $0
		${If} $0 == ${BST_CHECKED}
			StrCpy $installToAllUsers true
			${If} ${RunningX64}
				StrCpy $INSTDIR "${DEFAULT_SYSTEM_64BIT_INSTALL_LOCATION}"
			${Else}
				StrCpy $INSTDIR "${DEFAULT_SYSTEM_32BIT_INSTALL_LOCATION}"
			${EndIf}
		${Else}
			StrCpy $installToAllUsers false
			StrCpy $INSTDIR "${DEFAULT_USER_INSTALL_LOCATION}"
		${EndIf}
		${NSD_GetState} $checkBox_createDesktopShortcut $0
		${If} $0 == ${BST_CHECKED}
			StrCpy $createDesktopShortcut true
		${Else}
			StrCpy $createDesktopShortcut false
		${EndIf}
		${NSD_GetState} $checkBox_createStartMenuShortcut $0
		${If} $0 == ${BST_CHECKED}
			StrCpy $createStartMenuShortcut true
		${Else}
			StrCpy $createStartMenuShortcut false
		${EndIf}
	FunctionEnd
!macroend

!macro InitializeInstallerDirectoryPage
	!define MUI_DIRECTORYPAGE_TEXT_TOP "The wizard will install ${PROGRAM_NAME} in the following folder. To install in a different folder, click 'Browse' and select another folder. Click 'Install' to start the installation."
	!define MUI_PAGE_CUSTOMFUNCTION_SHOW OnDirectoryPageShow
	!define MUI_PAGE_CUSTOMFUNCTION_LEAVE OnDirectoryPageLeave
	!insertmacro MUI_PAGE_DIRECTORY
	Function OnDirectoryPageShow
		GetDlgItem $1 $HWNDPARENT 1
		SendMessage $1 ${BCM_SETSHIELD} 0 0
		${If} $installToAllUsers == true
			UserInfo::GetAccountType
			Pop $0
			${If} "$0" != "Admin"
				GetDlgItem $1 $HWNDPARENT 1
				SendMessage $1 ${BCM_SETSHIELD} 0 1
			${EndIf}
		${EndIf}
	FunctionEnd
	Function OnDirectoryPageLeave
		GetDlgItem $1 $HWNDPARENT 1
		SendMessage $1 ${BCM_SETSHIELD} 0 0
	FunctionEnd
!macroend

!macro InitializeInstallerInstfilesPage
	!insertmacro MUI_PAGE_INSTFILES
!macroend

!macro InitializeInstallerFinishPage
	!define MUI_FINISHPAGE_TITLE "${PROGRAM_NAME} Installation Has Been Failed"
	!define MUI_FINISHPAGE_TITLE_3LINES
	!define MUI_FINISHPAGE_TEXT "${PROGRAM_NAME} has not been installed on your computer.$\r$\n$\r$\nClick 'Finish' to close the installation wizard."
	!insertmacro MUI_PAGE_FINISH
!macroend

!macro SetUserInterfaceLanguage Language
	!insertmacro MUI_LANGUAGE "${Language}"
!macroend



!insertmacro InitializeGlobalVariables
!insertmacro InitializeInstallerWelcomePage
!insertmacro InitializeInstallerLicensePage
!insertmacro InitializeInstallerInstallOptionsPage
!insertmacro InitializeInstallerDirectoryPage
!insertmacro InitializeInstallerInstfilesPage
!insertmacro InitializeInstallerFinishPage
!insertmacro SetUserInterfaceLanguage "English"

Function .onInit
	SetOutPath "$TEMP"
	StrCpy $installToAllUsers false
	StrCpy $createDesktopShortcut true
	StrCpy $createStartMenuShortcut true
FunctionEnd

Function TryExecShell
	Pop $1
	Pop $2
	Pop $3
	System::Call "*(&l4, i 64, p $HWNDPARENT, t r1, t r2, t r3, p 0, p 5, p 0, p 0, p 0, p 0, p 0, p 0, p 0) p.r1"
	System::Call "shell32.dll::ShellExecuteEx(t) i.r0 (p r1)"
	${If} $0 <> 0
		StrCpy $0 true
	${Else}
		StrCpy $0 false
	${EndIf}
	System::Free $1
	Push $0
FunctionEnd

Section
	SetOutPath "${TEMPORARY_INSTALLER_EXTRACTION_LOCATION}"
	${If} $installToAllUsers == true
		File /a "..\bin\tmp\${SYSTEM_AUTO_INSTALLER_NAME}"
	${Else}
		File /a "..\bin\tmp\${USER_AUTO_INSTALLER_NAME}"
	${EndIf}
	SetOutPath "${TEMPORARY_INSTALLER_FILES_EXTRACTION_LOCATION}"
	${If} ${RunningX64}
		~[INSERT_64BIT_FILES_CREATION_COMMANDS_HERE]~ # This line will be replaced by an external script while building the installer
	${Else}
		~[INSERT_32BIT_FILES_CREATION_COMMANDS_HERE]~ # This line will be replaced by an external script while building the installer
	${EndIf}
	~[INSERT_COMMON_FILES_CREATION_COMMANDS_HERE]~ # This line will be replaced by an external script while building the installer
	SetOutPath "$TEMP"
	StrCpy $0 "${INSTALLER_ARGUMENT__INSTALL_LOCATION} $\"$INSTDIR$\""
	${If} $createDesktopShortcut == true
		StrCpy $1 " ${INSTALLER_ARGUMENT__CREATE_DESKTOP_SHORTCUT}"
	${Else}
		StrCpy $1 ""
	${EndIf}
	${If} $createStartMenuShortcut == true
		StrCpy $2 " ${INSTALLER_ARGUMENT__CREATE_START_MENU_SHORTCUT}"
	${Else}
		StrCpy $2 ""
	${EndIf}
	Push "$0$1$2"
	${If} $installToAllUsers == true
		Push "${TEMPORARY_INSTALLER_EXTRACTION_LOCATION}\${SYSTEM_AUTO_INSTALLER_NAME}"
	${Else}
		Push "${TEMPORARY_INSTALLER_EXTRACTION_LOCATION}\${USER_AUTO_INSTALLER_NAME}"
	${EndIf}
	Push "open"
	Call TryExecShell
	Pop $0
	${If} $0 == true
		Quit
	${EndIf}
	RMDir /r "${TEMPORARY_INSTALLER_EXTRACTION_LOCATION}"
SectionEnd
