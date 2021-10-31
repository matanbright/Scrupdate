import sys
import os



SCRIPT_LINE_TO_REPLACE__WITH_32BIT_FILES_CREATION_COMMANDS = "~[INSERT_32BIT_FILES_CREATION_COMMANDS_HERE]~"
SCRIPT_LINE_TO_REPLACE__WITH_64BIT_FILES_CREATION_COMMANDS = "~[INSERT_64BIT_FILES_CREATION_COMMANDS_HERE]~"
SCRIPT_LINE_TO_REPLACE__WITH_COMMON_FILES_CREATION_COMMANDS = "~[INSERT_COMMON_FILES_CREATION_COMMANDS_HERE]~"
SCRIPT_LINE_TO_REPLACE__WITH_32BIT_FILES_REMOVAL_COMMANDS = "~[INSERT_32BIT_FILES_REMOVAL_COMMANDS_HERE]~"
SCRIPT_LINE_TO_REPLACE__WITH_64BIT_FILES_REMOVAL_COMMANDS = "~[INSERT_64BIT_FILES_REMOVAL_COMMANDS_HERE]~"
SCRIPT_LINE_TO_REPLACE__WITH_COMMON_FILES_REMOVAL_COMMANDS = "~[INSERT_COMMON_FILES_REMOVAL_COMMANDS_HERE]~"
PHRASE__COMMENT = "#"
PHRASE__SET_32BIT_ROOT_POINT = "~32"
PHRASE__SET_64BIT_ROOT_POINT = "~64"
PHRASE__SET_COMMON_ROOT_POINT = "~"
PHRASE__INCLUDE_32BIT_FILE = "+32"
PHRASE__INCLUDE_64BIT_FILE = "+64"
PHRASE__INCLUDE_COMMON_FILE = "+"

_32bitRootPoint = ""
_64bitRootPoint = ""
commonRootPoint = ""


if ("--help" in sys.argv):
    print("Usage: " + os.path.basename(__file__) + " path_to_file_with_paths_of_files_to_include path_to_nsi_script")
elif (len(sys.argv) < 3):
    print("Error: Missing arguments!")
else:
    _32bitFilesCreationCommands = []
    _64bitFilesCreationCommands = []
    commonFilesCreationCommands = []
    _32bitFilesRemovalCommands = []
    _64bitFilesRemovalCommands = []
    commonFilesRemovalCommands = []
    with open(sys.argv[1], "r") as file:
        fileLines = []
        for fileLineWithCrLf in file.readlines():
            fileLines.append(fileLineWithCrLf.replace("\r", "").replace("\n", "").strip())
        for fileLine in fileLines:
            if (len(fileLine) > 0):
                if (not fileLine.startswith(PHRASE__COMMENT)):
                    if ((fileLine.startswith(PHRASE__SET_32BIT_ROOT_POINT)) or (fileLine.startswith(PHRASE__SET_64BIT_ROOT_POINT)) or (fileLine.startswith(PHRASE__SET_COMMON_ROOT_POINT))):
                        if (fileLine.count('"') == 2):
                            rootPointPath = fileLine[fileLine.find('"') + 1 : fileLine.rfind('"')]
                            if (fileLine.startswith(PHRASE__SET_32BIT_ROOT_POINT)):
                                _32bitRootPoint = rootPointPath
                            elif (fileLine.startswith(PHRASE__SET_64BIT_ROOT_POINT)):
                                _64bitRootPoint = rootPointPath
                            elif (fileLine.startswith(PHRASE__SET_COMMON_ROOT_POINT)):
                                commonRootPoint = rootPointPath
                    elif ((fileLine.startswith(PHRASE__INCLUDE_32BIT_FILE)) or (fileLine.startswith(PHRASE__INCLUDE_64BIT_FILE)) or (fileLine.startswith(PHRASE__INCLUDE_COMMON_FILE))):
                        if (fileLine.count('"') == 2):
                            pathOfFileToIncludeRelativeToRootPoint = fileLine[fileLine.find('"') + 1 : fileLine.rfind('"')]
                            if (fileLine.startswith(PHRASE__INCLUDE_32BIT_FILE)):
                                if (pathOfFileToIncludeRelativeToRootPoint.count('\\') > 0):
                                    _32bitFilesCreationCommands.append("CreateDirectory \"$OUTDIR\\" + os.path.dirname(pathOfFileToIncludeRelativeToRootPoint) + "\"")
                                _32bitFilesCreationCommands.append("File /a \"/oname=" + pathOfFileToIncludeRelativeToRootPoint + "\" \"" + _32bitRootPoint + "\\" + pathOfFileToIncludeRelativeToRootPoint + "\"")
                                _32bitFilesRemovalCommands.append("Delete \"$INSTDIR\\" + pathOfFileToIncludeRelativeToRootPoint + "\"")
                                tempPathOfFileToIncludeRelativeToRootPoint = pathOfFileToIncludeRelativeToRootPoint
                                for i in range(pathOfFileToIncludeRelativeToRootPoint.count('\\')):
                                    tempPathOfFileToIncludeRelativeToRootPoint = os.path.dirname(tempPathOfFileToIncludeRelativeToRootPoint)
                                    _32bitFilesRemovalCommands.append("RMDir \"$INSTDIR\\" + tempPathOfFileToIncludeRelativeToRootPoint + "\"")
                            elif (fileLine.startswith(PHRASE__INCLUDE_64BIT_FILE)):
                                if (pathOfFileToIncludeRelativeToRootPoint.count('\\') > 0):
                                    _64bitFilesCreationCommands.append("CreateDirectory \"$OUTDIR\\" + os.path.dirname(pathOfFileToIncludeRelativeToRootPoint) + "\"")
                                _64bitFilesCreationCommands.append("File /a \"/oname=" + pathOfFileToIncludeRelativeToRootPoint + "\" \"" + _64bitRootPoint + "\\" + pathOfFileToIncludeRelativeToRootPoint + "\"")
                                _64bitFilesRemovalCommands.append("Delete \"$INSTDIR\\" + pathOfFileToIncludeRelativeToRootPoint + "\"")
                                tempPathOfFileToIncludeRelativeToRootPoint = pathOfFileToIncludeRelativeToRootPoint
                                for i in range(pathOfFileToIncludeRelativeToRootPoint.count('\\')):
                                    tempPathOfFileToIncludeRelativeToRootPoint = os.path.dirname(tempPathOfFileToIncludeRelativeToRootPoint)
                                    _64bitFilesRemovalCommands.append("RMDir \"$INSTDIR\\" + tempPathOfFileToIncludeRelativeToRootPoint + "\"")
                            elif (fileLine.startswith(PHRASE__INCLUDE_COMMON_FILE)):
                                if (pathOfFileToIncludeRelativeToRootPoint.count('\\') > 0):
                                    commonFilesCreationCommands.append("CreateDirectory \"$OUTDIR\\" + os.path.dirname(pathOfFileToIncludeRelativeToRootPoint) + "\"")
                                commonFilesCreationCommands.append("File /a \"/oname=" + pathOfFileToIncludeRelativeToRootPoint + "\" \"" + commonRootPoint + "\\" + pathOfFileToIncludeRelativeToRootPoint + "\"")
                                commonFilesRemovalCommands.append("Delete \"$INSTDIR\\" + pathOfFileToIncludeRelativeToRootPoint + "\"")
                                tempPathOfFileToIncludeRelativeToRootPoint = pathOfFileToIncludeRelativeToRootPoint
                                for i in range(pathOfFileToIncludeRelativeToRootPoint.count('\\')):
                                    tempPathOfFileToIncludeRelativeToRootPoint = os.path.dirname(tempPathOfFileToIncludeRelativeToRootPoint)
                                    commonFilesRemovalCommands.append("RMDir \"$INSTDIR\\" + tempPathOfFileToIncludeRelativeToRootPoint + "\"")
    nsiScriptCodeLines = None
    with open(sys.argv[2], "r") as file:
        nsiScriptCodeLines = file.readlines()
    with open(sys.argv[2], "w") as file:
        for nsiScriptCodeLine in nsiScriptCodeLines:
            if (SCRIPT_LINE_TO_REPLACE__WITH_32BIT_FILES_CREATION_COMMANDS in nsiScriptCodeLine):
                for _32bitFileCreationCommand in _32bitFilesCreationCommands:
                    file.write(_32bitFileCreationCommand + "\n")
            elif (SCRIPT_LINE_TO_REPLACE__WITH_64BIT_FILES_CREATION_COMMANDS in nsiScriptCodeLine):
                for _64bitFileCreationCommand in _64bitFilesCreationCommands:
                    file.write(_64bitFileCreationCommand + "\n")
            elif (SCRIPT_LINE_TO_REPLACE__WITH_COMMON_FILES_CREATION_COMMANDS in nsiScriptCodeLine):
                for commonFileCreationCommand in commonFilesCreationCommands:
                    file.write(commonFileCreationCommand + "\n")
            elif (SCRIPT_LINE_TO_REPLACE__WITH_32BIT_FILES_REMOVAL_COMMANDS in nsiScriptCodeLine):
                for _32bitFileRemovalCommand in _32bitFilesRemovalCommands:
                    file.write(_32bitFileRemovalCommand + "\n")
            elif (SCRIPT_LINE_TO_REPLACE__WITH_64BIT_FILES_REMOVAL_COMMANDS in nsiScriptCodeLine):
                for _64bitFileRemovalCommand in _64bitFilesRemovalCommands:
                    file.write(_64bitFileRemovalCommand + "\n")
            elif (SCRIPT_LINE_TO_REPLACE__WITH_COMMON_FILES_REMOVAL_COMMANDS in nsiScriptCodeLine):
                for commonFileRemovalCommand in commonFilesRemovalCommands:
                    file.write(commonFileRemovalCommand + "\n")
            else:
                file.write(nsiScriptCodeLine)
