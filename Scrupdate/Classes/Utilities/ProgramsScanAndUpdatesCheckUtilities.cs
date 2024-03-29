﻿// Copyright © 2021 Matan Brightbert
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.




using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Scrupdate.Classes.Objects;

namespace Scrupdate.Classes.Utilities
{
    public static class ProgramsScanAndUpdatesCheckUtilities
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class ProgramDatabaseIsNotOpenException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__PROGRAM_DATABASE_IS_NOT_OPEN = "Program database is not open!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public ProgramDatabaseIsNotOpenException() : base(EXCEPTION_MESSAGE__PROGRAM_DATABASE_IS_NOT_OPEN) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class NoChromeDriverIsInstalledException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__NO_CHROMEDRIVER_IS_INSTALLED = "No ChromeDriver is installed!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public NoChromeDriverIsInstalledException() : base(EXCEPTION_MESSAGE__NO_CHROMEDRIVER_IS_INSTALLED) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class UnableToAccessChromeDriverExecutableFileException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__UNABLE_TO_ACCESS_CHROMEDRIVER_EXECUTABLE_FILE = "Unable to access ChromeDriver executable file!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public UnableToAccessChromeDriverExecutableFileException() : base(EXCEPTION_MESSAGE__UNABLE_TO_ACCESS_CHROMEDRIVER_EXECUTABLE_FILE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class GoogleChromeBrowserIsNotInstalledException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__GOOGLE_CHROME_BROWSER_IS_NOT_INSTALLED = "Google Chrome™ browser is not installed!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public GoogleChromeBrowserIsNotInstalledException() : base(EXCEPTION_MESSAGE__GOOGLE_CHROME_BROWSER_IS_NOT_INSTALLED) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class UnableToAccessGoogleChromeBrowserExecutableFileException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__UNABLE_TO_ACCESS_GOOGLE_CHROME_BROWSER_EXECUTABLE_FILE = "Unable to access Google Chrome™ browser executable file!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public UnableToAccessGoogleChromeBrowserExecutableFileException() : base(EXCEPTION_MESSAGE__UNABLE_TO_ACCESS_GOOGLE_CHROME_BROWSER_EXECUTABLE_FILE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class UnableToGetDefaultChromeDriverUserAgentStringException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__UNABLE_TO_GET_DEFAULT_CHROMEDRIVER_USER_AGENT_STRING = "Unable to get default ChromeDriver user-agent string!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public UnableToGetDefaultChromeDriverUserAgentStringException() : base(EXCEPTION_MESSAGE__UNABLE_TO_GET_DEFAULT_CHROMEDRIVER_USER_AGENT_STRING) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class ChromeDriverIsNotCompatibleOrGoogleChromeBrowserCannotBeOpenedException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__CHROMEDRIVER_IS_NOT_COMPATIBLE_OR_GOOGLE_CHROME_BROWSER_CANNOT_BE_OPENED = "The ChromeDriver's version is not compatible with the version of the installed Google Chrome™ browser or the browser cannot be opened!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public ChromeDriverIsNotCompatibleOrGoogleChromeBrowserCannotBeOpenedException() : base(EXCEPTION_MESSAGE__CHROMEDRIVER_IS_NOT_COMPATIBLE_OR_GOOGLE_CHROME_BROWSER_CANNOT_BE_OPENED) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class WebPageDidNotRespondException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__WEB_PAGE_DID_NOT_RESPOND = "Web page did not respond!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public WebPageDidNotRespondException() : base(EXCEPTION_MESSAGE__WEB_PAGE_DID_NOT_RESPOND) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class HtmlElementWasNotFoundException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__HTML_ELEMENT_WAS_NOT_FOUND = "HTML element was not found!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public HtmlElementWasNotFoundException() : base(EXCEPTION_MESSAGE__HTML_ELEMENT_WAS_NOT_FOUND) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class TextWasNotFoundWithinWebPageException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__TEXT_WAS_NOT_FOUND_WITHIN_WEB_PAGE = "Text was not found within web page!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public TextWasNotFoundWithinWebPageException() : base(EXCEPTION_MESSAGE__TEXT_WAS_NOT_FOUND_WITHIN_WEB_PAGE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class NoVersionWasFoundException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__NO_VERSION_WAS_FOUND = "No version was found!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public NoVersionWasFoundException() : base(EXCEPTION_MESSAGE__NO_VERSION_WAS_FOUND) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Delegates ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public delegate void ProgramUpdatesCheckProgressChangedEventHandler(double progress);
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private enum ProgramType
        {
            Unknown,
            UserProgram,
            System32BitProgram,
            System64BitProgram
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void ScanForInstalledProgramsAndUpdateProgramDatabase(ProgramDatabase programDatabase, SettingsHandler settingsHandler, CancellationToken? cancellationToken)
        {
            if (programDatabase == null || settingsHandler == null)
                throw new ArgumentNullException();
            if (!programDatabase.IsOpen())
                throw new ProgramDatabaseIsNotOpenException();
            if (settingsHandler.SettingsInMemory == null)
                throw new SettingsHandler.NoSettingsInMemoryException();
            RegistryKey installedUserProgramsRegistryKey = Registry.CurrentUser?.OpenSubKey("SOFTWARE")?.OpenSubKey("Microsoft")?.OpenSubKey("Windows")?.OpenSubKey("CurrentVersion")?.OpenSubKey("Uninstall");
            RegistryKey installedSystem32BitProgramsRegistryKey = Registry.LocalMachine?.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Microsoft")?.OpenSubKey("Windows")?.OpenSubKey("CurrentVersion")?.OpenSubKey("Uninstall");
            RegistryKey installedSystem64BitProgramsRegistryKey = Registry.LocalMachine?.OpenSubKey("SOFTWARE")?.OpenSubKey("Microsoft")?.OpenSubKey("Windows")?.OpenSubKey("CurrentVersion")?.OpenSubKey("Uninstall");
            string[] installedUserProgramsRegistrySubKeysNames = installedUserProgramsRegistryKey?.GetSubKeyNames();
            string[] installedSystem32BitProgramsRegistrySubKeysNames = installedSystem32BitProgramsRegistryKey?.GetSubKeyNames();
            string[] installedSystem64BitProgramsRegistrySubKeysNames = installedSystem64BitProgramsRegistryKey?.GetSubKeyNames();
            StringBuilder stringOfAllInstalledPrograms = new StringBuilder();
            Dictionary<string, Program> installedPrograms = new Dictionary<string, Program>();
            for (ProgramType installedProgramsType = (ProgramType)1; ((int)installedProgramsType) < Enum.GetNames(typeof(ProgramType)).Length; installedProgramsType++)
            {
                RegistryKey installedProgramsRegistryKey = null;
                string[] installedProgramsRegistrySubKeysNames = null;
                Program.ProgramInstallationScope installedProgramsInstallationScope = Program.ProgramInstallationScope.None;
                switch (installedProgramsType)
                {
                    case ProgramType.UserProgram:
                        installedProgramsRegistryKey = installedUserProgramsRegistryKey;
                        installedProgramsRegistrySubKeysNames = installedUserProgramsRegistrySubKeysNames;
                        installedProgramsInstallationScope = Program.ProgramInstallationScope.User;
                        break;
                    case ProgramType.System32BitProgram:
                        installedProgramsRegistryKey = installedSystem32BitProgramsRegistryKey;
                        installedProgramsRegistrySubKeysNames = installedSystem32BitProgramsRegistrySubKeysNames;
                        installedProgramsInstallationScope = Program.ProgramInstallationScope.Everyone;
                        break;
                    case ProgramType.System64BitProgram:
                        installedProgramsRegistryKey = installedSystem64BitProgramsRegistryKey;
                        installedProgramsRegistrySubKeysNames = installedSystem64BitProgramsRegistrySubKeysNames;
                        installedProgramsInstallationScope = Program.ProgramInstallationScope.Everyone;
                        break;
                }
                if (installedProgramsRegistryKey != null && installedProgramsRegistrySubKeysNames != null)
                {
                    foreach (string installedProgramRegistrySubKeyName in installedProgramsRegistrySubKeysNames)
                    {
                        if (cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                            return;
                        string installedProgramName = (string)installedProgramsRegistryKey.OpenSubKey(installedProgramRegistrySubKeyName).GetValue("DisplayName");
                        string installedProgramInstalledVersion = (string)installedProgramsRegistryKey.OpenSubKey(installedProgramRegistrySubKeyName).GetValue("DisplayVersion");
                        if (installedProgramInstalledVersion != null)
                        {
                            if (!VersionsUtilities.IsVersion(installedProgramInstalledVersion, VersionsUtilities.VersionValidation.ValidateVersionSegmentsCount))
                                installedProgramInstalledVersion = new string(Array.FindAll(installedProgramInstalledVersion.ToCharArray(), ((char c) => (char.IsDigit(c) || c == '.'))));
                            try
                            {
                                installedProgramInstalledVersion = VersionsUtilities.TrimVersion(installedProgramInstalledVersion, VersionsUtilities.MINIMUM_VERSION_SEGMENTS, VersionsUtilities.MAXIMUM_VERSION_SEGMENTS);
                            }
                            catch
                            {
                                installedProgramInstalledVersion = null;
                            }
                        }
                        Program.ProgramInstallationScope installedInstallationScope = installedProgramsInstallationScope;
                        if (installedProgramName != null)
                        {
                            stringOfAllInstalledPrograms.Append(installedProgramName);
                            stringOfAllInstalledPrograms.Append((installedProgramInstalledVersion != null ? installedProgramInstalledVersion : ""));
                            stringOfAllInstalledPrograms.Append(Convert.ToString((long)installedInstallationScope));
                            if (installedInstallationScope == Program.ProgramInstallationScope.Everyone)
                                stringOfAllInstalledPrograms.Append((installedProgramsType == ProgramType.System32BitProgram ? "32" : (installedProgramsType == ProgramType.System64BitProgram ? "64" : "")));
                            string versionFromProgramName;
                            installedProgramName = VersionsUtilities.GetStringWithoutTheFirstFoundVersion(installedProgramName.Trim(), false, false, true, out versionFromProgramName);
                            if (versionFromProgramName != null && (installedProgramInstalledVersion == null || installedProgramInstalledVersion.Equals("")))
                                installedProgramInstalledVersion = versionFromProgramName;
                            if (!installedPrograms.ContainsKey(installedProgramName))
                            {
                                installedPrograms.Add(installedProgramName, new Program(
                                    installedProgramName,
                                    (installedProgramInstalledVersion != null ? installedProgramInstalledVersion : ""),
                                    "",
                                    installedInstallationScope,
                                    false,
                                    "",
                                    Program.ProgramVersionSearchMethod.Unknown,
                                    "",
                                    "",
                                    false,
                                    Program.ProgramVersionSearchBehavior.Unknown,
                                    Program.ProgramWebPagePostLoadDelay._None,
                                    new List<WebPageElementLocatingInstruction>(),
                                    true,
                                    Program.ProgramUpdateCheckConfigurationStatus.Unknown,
                                    Program.ProgramUpdateCheckConfigurationError.None,
                                    false
                                ));
                            }
                            else if (installedProgramInstalledVersion != null)
                            {
                                Program programAlreadyFound = installedPrograms[installedProgramName];
                                if (VersionsUtilities.IsVersionNewer(installedProgramInstalledVersion, programAlreadyFound.InstalledVersion))
                                {
                                    programAlreadyFound.InstalledVersion = installedProgramInstalledVersion;
                                    programAlreadyFound.InstallationScope = installedInstallationScope;
                                }
                            }
                        }
                    }
                }
            }
            string hashOfStringOfAllInstalledPrograms = HashingUtilities.GetMD5Hash(stringOfAllInstalledPrograms.ToString());
            if (!settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms.Equals(hashOfStringOfAllInstalledPrograms))
            {
                programDatabase.BeginTransaction();
                for (Dictionary<string, Program>.Enumerator i = programDatabase.GetPrograms().GetEnumerator(); i.MoveNext();)
                {
                    Program programAlreadyInDatabase = i.Current.Value;
                    if (!installedPrograms.ContainsKey(programAlreadyInDatabase.Name))
                        programDatabase.UpdateProgramInstallationInfoToAutomaticallyDetectedProgram(programAlreadyInDatabase.Name, "", Program.ProgramInstallationScope.None);
                    else
                    {
                        Program installedProgram = installedPrograms[programAlreadyInDatabase.Name];
                        installedPrograms[programAlreadyInDatabase.Name] = null;
                        programDatabase.UpdateProgramInstallationInfoToAutomaticallyDetectedProgram(programAlreadyInDatabase.Name, installedProgram.InstalledVersion, installedProgram.InstallationScope);
                    }
                }
                programDatabase.CommitTransaction();
                programDatabase.BeginTransaction();
                for (Dictionary<string, Program>.Enumerator i = installedPrograms.GetEnumerator(); i.MoveNext();)
                {
                    Program foundProgram = i.Current.Value;
                    if (foundProgram != null)
                        programDatabase.AddNewProgram(foundProgram);
                }
                programDatabase.CommitTransaction();
                string backupOfLastHashOfAllInstalledPrograms = settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms;
                settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = hashOfStringOfAllInstalledPrograms;
                if (!settingsHandler.SaveSettingsFromMemoryToSettingsFile())
                    settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = backupOfLastHashOfAllInstalledPrograms;
            }
        }
        public static void CheckForProgramUpdatesAndUpdateDatabase(List<Program> programsToCheck, ProgramDatabase programDatabase, SettingsHandler settingsHandler, ProgramUpdatesCheckProgressChangedEventHandler updatesCheckProgressChangedEventHandler, CancellationToken? cancellationToken)
        {
            if (programsToCheck == null || programDatabase == null || settingsHandler == null)
                throw new ArgumentNullException();
            if (settingsHandler.SettingsInMemory == null)
                throw new SettingsHandler.NoSettingsInMemoryException();
            bool unableToAccessInstalledChromeDriverExecutableFile;
            if (ChromeDriverUtilities.GetInstalledChromeDriverInformation(out unableToAccessInstalledChromeDriverExecutableFile) == null)
            {
                if (unableToAccessInstalledChromeDriverExecutableFile)
                    throw new UnableToAccessChromeDriverExecutableFileException();
                throw new NoChromeDriverIsInstalledException();
            }
            if (!GoogleChromeBrowserUtilities.IsGoogleChromeBrowserInstalled())
                throw new GoogleChromeBrowserIsNotInstalledException();
            string checksumOfInstalledGoogleChromeBrowserExecutableFile = GoogleChromeBrowserUtilities.GetChecksumOfInstalledGoogleChromeBrowserExecutableFile();
            if (checksumOfInstalledGoogleChromeBrowserExecutableFile == null)
                throw new UnableToAccessGoogleChromeBrowserExecutableFileException();
            string defaultChromeDriverUserAgentString = settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString;
            if (settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString.Equals("") || !checksumOfInstalledGoogleChromeBrowserExecutableFile.Equals(settingsHandler.SettingsInMemory.Cached.LastChecksumOfInstalledGoogleChromeBrowserExecutableFile))
            {
                defaultChromeDriverUserAgentString = ChromeDriverUtilities.GetDefaultChromeDriverUserAgentString();
                if (defaultChromeDriverUserAgentString == null)
                    throw new UnableToGetDefaultChromeDriverUserAgentStringException();
                string backupOfLastChecksumOfInstalledGoogleChromeBrowserExecutableFile = settingsHandler.SettingsInMemory.Cached.LastChecksumOfInstalledGoogleChromeBrowserExecutableFile;
                string backupOfLastDefaultChromeDriverUserAgentString = settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString;
                settingsHandler.SettingsInMemory.Cached.LastChecksumOfInstalledGoogleChromeBrowserExecutableFile = checksumOfInstalledGoogleChromeBrowserExecutableFile;
                settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString = defaultChromeDriverUserAgentString;
                if (!settingsHandler.SaveSettingsFromMemoryToSettingsFile())
                {
                    settingsHandler.SettingsInMemory.Cached.LastChecksumOfInstalledGoogleChromeBrowserExecutableFile = backupOfLastChecksumOfInstalledGoogleChromeBrowserExecutableFile;
                    settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString = backupOfLastDefaultChromeDriverUserAgentString;
                }
            }
            if (cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                return;
            string chromeDriverUserAgentString = null;
            if (!settingsHandler.SettingsInMemory.ChromeDriver.UseCustomUserAgentString)
                chromeDriverUserAgentString = defaultChromeDriverUserAgentString;
            else
                chromeDriverUserAgentString = settingsHandler.SettingsInMemory.ChromeDriver.CustomUserAgentString;
            int chromeDriverPageLoadTimeoutInMilliseconds = 0;
            switch (settingsHandler.SettingsInMemory.ChromeDriver.PageLoadTimeout)
            {
                case Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout.After1Seconds:
                    chromeDriverPageLoadTimeoutInMilliseconds = 1000;
                    break;
                case Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout.After3Seconds:
                    chromeDriverPageLoadTimeoutInMilliseconds = 3000;
                    break;
                case Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout.After5Seconds:
                    chromeDriverPageLoadTimeoutInMilliseconds = 5000;
                    break;
                case Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout.After10Seconds:
                    chromeDriverPageLoadTimeoutInMilliseconds = 10000;
                    break;
                case Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout.After15Seconds:
                    chromeDriverPageLoadTimeoutInMilliseconds = 15000;
                    break;
                case Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout.After30Seconds:
                    chromeDriverPageLoadTimeoutInMilliseconds = 30000;
                    break;
            }
            using (ChromeDriver chromeDriver = new ChromeDriver(ChromeDriverUtilities.chromeDriverDirectoryPath, chromeDriverUserAgentString, chromeDriverPageLoadTimeoutInMilliseconds))
            {
                try
                {
                    chromeDriver.Open(true);
                }
                catch
                {
                    throw new ChromeDriverIsNotCompatibleOrGoogleChromeBrowserCannotBeOpenedException();
                }
                if (cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                    return;
                programDatabase.ResetLatestVersionAndUpdateCheckConfigurationStatusOfAllConfiguredPrograms();
                settingsHandler.SettingsInMemory.Cached.LastProgramUpdatesCheckTime = DateTime.Now;
                settingsHandler.SaveSettingsFromMemoryToSettingsFile();
                for (int i = 0; i < programsToCheck.Count; i++)
                {
                    Program programToCheck = programsToCheck[i];
                    updatesCheckProgressChangedEventHandler?.Invoke(((double)i / programsToCheck.Count) * 100.0D);
                    if (cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                        return;
                    try
                    {
                        try
                        {
                            chromeDriver.NavigateToAWebPage(programToCheck.WebPageUrl);
                        }
                        catch
                        {
                            throw new WebPageDidNotRespondException();
                        }
                        if (cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                            return;
                        int webPagePostLoadDelayInMilliseconds = 0;
                        switch (programToCheck.WebPagePostLoadDelay)
                        {
                            case Program.ProgramWebPagePostLoadDelay._100Ms:
                                webPagePostLoadDelayInMilliseconds = 100;
                                break;
                            case Program.ProgramWebPagePostLoadDelay._250Ms:
                                webPagePostLoadDelayInMilliseconds = 250;
                                break;
                            case Program.ProgramWebPagePostLoadDelay._500Ms:
                                webPagePostLoadDelayInMilliseconds = 500;
                                break;
                            case Program.ProgramWebPagePostLoadDelay._1000Ms:
                                webPagePostLoadDelayInMilliseconds = 1000;
                                break;
                            case Program.ProgramWebPagePostLoadDelay._2000Ms:
                                webPagePostLoadDelayInMilliseconds = 2000;
                                break;
                            case Program.ProgramWebPagePostLoadDelay._3000Ms:
                                webPagePostLoadDelayInMilliseconds = 3000;
                                break;
                            case Program.ProgramWebPagePostLoadDelay._4000Ms:
                                webPagePostLoadDelayInMilliseconds = 4000;
                                break;
                            case Program.ProgramWebPagePostLoadDelay._5000Ms:
                                webPagePostLoadDelayInMilliseconds = 5000;
                                break;
                        }
                        if (webPagePostLoadDelayInMilliseconds > 0)
                        {
                            if (cancellationToken != null)
                                cancellationToken.Value.WaitHandle.WaitOne(webPagePostLoadDelayInMilliseconds);
                            else
                                Thread.Sleep(webPagePostLoadDelayInMilliseconds);
                        }
                        if (cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                            return;
                        if (programToCheck.WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn.Count > 0)
                        {
                            foreach (WebPageElementLocatingInstruction webPageElementLocatingInstructionOfWebPageElementToSimulateAClickOn in programToCheck.WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn)
                            {
                                if (cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                                    return;
                                try
                                {
                                    chromeDriver.ClickOnAnElementWithinTheWebpage(webPageElementLocatingInstructionOfWebPageElementToSimulateAClickOn, cancellationToken);
                                }
                                catch { }
                            }
                        }
                        if (cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                            return;
                        string textToSerachVersion = null;
                        string tempTextToSerachVersion = null;
                        switch (programToCheck.VersionSearchMethod)
                        {
                            case Program.ProgramVersionSearchMethod.SearchInTheContentOfHtmlElementWithId:
                                try
                                {
                                    textToSerachVersion = chromeDriver.GetTextInsideHtmlElementById(programToCheck.VersionSearchMethodArgument1);
                                }
                                catch
                                {
                                    throw new HtmlElementWasNotFoundException();
                                }
                                break;
                            case Program.ProgramVersionSearchMethod.SearchInTheContentOfHtmlElementsMatchingXPath:
                                try
                                {
                                    textToSerachVersion = string.Join(" ", chromeDriver.GetTextsInsideHtmlElementsByXPath(programToCheck.VersionSearchMethodArgument1));
                                }
                                catch
                                {
                                    throw new HtmlElementWasNotFoundException();
                                }
                                break;
                            case Program.ProgramVersionSearchMethod.SearchGloballyInTheWebPage:
                                textToSerachVersion = chromeDriver.GetAllTextWithinWebPage();
                                break;
                            case Program.ProgramVersionSearchMethod.SearchGloballyFromTextWithinWebPage:
                            case Program.ProgramVersionSearchMethod.SearchGloballyUntilTextWithinWebPage:
                                tempTextToSerachVersion = chromeDriver.GetAllTextWithinWebPage();
                                int foundTextIndex = tempTextToSerachVersion.IndexOf(programToCheck.VersionSearchMethodArgument1);
                                if (foundTextIndex < 0)
                                    throw new TextWasNotFoundWithinWebPageException();
                                if (programToCheck.VersionSearchMethod == Program.ProgramVersionSearchMethod.SearchGloballyFromTextWithinWebPage)
                                    textToSerachVersion = tempTextToSerachVersion.Substring(foundTextIndex + programToCheck.VersionSearchMethodArgument1.Length);
                                else
                                    textToSerachVersion = tempTextToSerachVersion.Substring(0, foundTextIndex);
                                break;
                            case Program.ProgramVersionSearchMethod.SearchGloballyFromTextUntilTextWithinWebPage:
                                tempTextToSerachVersion = chromeDriver.GetAllTextWithinWebPage();
                                int foundStartingTextIndex = tempTextToSerachVersion.IndexOf(programToCheck.VersionSearchMethodArgument1);
                                int foundEndingTextIndex = tempTextToSerachVersion.IndexOf(programToCheck.VersionSearchMethodArgument2);
                                if (foundStartingTextIndex < 0 || foundEndingTextIndex < 0)
                                    throw new TextWasNotFoundWithinWebPageException();
                                if (foundEndingTextIndex - foundStartingTextIndex >= programToCheck.VersionSearchMethodArgument1.Length)
                                    textToSerachVersion = tempTextToSerachVersion.Substring(foundStartingTextIndex + programToCheck.VersionSearchMethodArgument1.Length, foundEndingTextIndex - (foundStartingTextIndex + programToCheck.VersionSearchMethodArgument1.Length));
                                break;
                        }
                        if (textToSerachVersion == null || textToSerachVersion.Equals(""))
                            throw new NoVersionWasFoundException();
                        string versionString = null;
                        switch (programToCheck.VersionSearchBehavior)
                        {
                            case Program.ProgramVersionSearchBehavior.GetTheFirstVersionThatIsFound:
                                versionString = VersionsUtilities.GetTheFirstFoundVersionFromString(textToSerachVersion, programToCheck.TreatAStandaloneNumberAsAVersion, false);
                                break;
                            case Program.ProgramVersionSearchBehavior.GetTheFirstVersionThatIsFoundFromTheEnd:
                                versionString = VersionsUtilities.GetTheFirstFoundVersionFromString(textToSerachVersion, programToCheck.TreatAStandaloneNumberAsAVersion, true);
                                break;
                            case Program.ProgramVersionSearchBehavior.GetTheLatestVersionFromAllTheVersionsThatAreFound:
                                versionString = VersionsUtilities.GetTheLatestVersionFromString(textToSerachVersion, programToCheck.TreatAStandaloneNumberAsAVersion);
                                break;
                        }
                        if (versionString == null || versionString.Equals(""))
                            throw new NoVersionWasFoundException();
                        programDatabase.UpdateProgramLatestVersion(programToCheck.Name, VersionsUtilities.TrimVersion(versionString, VersionsUtilities.MINIMUM_VERSION_SEGMENTS, VersionsUtilities.MAXIMUM_VERSION_SEGMENTS));
                        programDatabase.ChangeProgramConfigurationStatus(programToCheck.Name, Program.ProgramUpdateCheckConfigurationStatus.Valid, Program.ProgramUpdateCheckConfigurationError.None);
                    }
                    catch (Exception e)
                    {
                        Program.ProgramUpdateCheckConfigurationError updateCheckConfigurationError = Program.ProgramUpdateCheckConfigurationError.None;
                        if (e.GetType().Equals(typeof(WebPageDidNotRespondException)))
                            updateCheckConfigurationError = Program.ProgramUpdateCheckConfigurationError.WebPageDidNotRespond;
                        else if (e.GetType().Equals(typeof(HtmlElementWasNotFoundException)))
                            updateCheckConfigurationError = Program.ProgramUpdateCheckConfigurationError.HtmlElementWasNotFound;
                        else if (e.GetType().Equals(typeof(TextWasNotFoundWithinWebPageException)))
                            updateCheckConfigurationError = Program.ProgramUpdateCheckConfigurationError.TextWasNotFoundWithinWebPage;
                        else if (e.GetType().Equals(typeof(NoVersionWasFoundException)))
                            updateCheckConfigurationError = Program.ProgramUpdateCheckConfigurationError.NoVersionWasFound;
                        else
                            updateCheckConfigurationError = Program.ProgramUpdateCheckConfigurationError.GeneralFailure;
                        programDatabase.UpdateProgramLatestVersion(programToCheck.Name, "");
                        programDatabase.ChangeProgramConfigurationStatus(programToCheck.Name, Program.ProgramUpdateCheckConfigurationStatus.Invalid, updateCheckConfigurationError);
                    }
                }
                settingsHandler.SettingsInMemory.Cached.LastProgramUpdatesCheckTime = DateTime.Now;
                settingsHandler.SaveSettingsFromMemoryToSettingsFile();
                updatesCheckProgressChangedEventHandler?.Invoke(100.0D);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
