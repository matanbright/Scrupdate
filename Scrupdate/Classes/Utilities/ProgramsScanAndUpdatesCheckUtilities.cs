// Copyright © 2021-2024 Matan Brightbert
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
            private const string EXCEPTION_MESSAGE = "Program database is not open!";
            public ProgramDatabaseIsNotOpenException() : base(EXCEPTION_MESSAGE) { }
        }
        public class ProgramUpdatesCheckWasFailedException : Exception
        {
            public enum Reason
            {
                Unknown,
                NoChromeDriverIsInstalled,
                UnableToAccessTheChromeDriver,
                GoogleChromeBrowserIsNotInstalled,
                UnableToAccessTheGoogleChromeBrowser,
                UnableToGetDefaultChromeDriverUserAgent,
                ChromeDriverError
            }
            private const string EXCEPTION_MESSAGE = "Program Updates Check Was Failed (Reason: {*})!";
            private static readonly Dictionary<Reason, string> REASONS = new Dictionary<Reason, string>()
            {
                { Reason.Unknown, "Unknown!" },
                { Reason.NoChromeDriverIsInstalled, "No ChromeDriver Is Installed!" },
                { Reason.UnableToAccessTheChromeDriver, "Unable to Access the ChromeDriver!" },
                { Reason.GoogleChromeBrowserIsNotInstalled, "Google Chrome™ Browser Is Not Installed!" },
                { Reason.UnableToAccessTheGoogleChromeBrowser, "Unable to Access the Google Chrome™ Browser!" },
                { Reason.UnableToGetDefaultChromeDriverUserAgent, "Unable to Get Default ChromeDriver User-Agent!" },
                { Reason.ChromeDriverError, "ChromeDriver Error!" }
            };
            private static readonly Dictionary<Reason, string> LONG_REASONS = new Dictionary<Reason, string>()
            {
                { Reason.Unknown, "Unknown!" },
                { Reason.NoChromeDriverIsInstalled, "No ChromeDriver Is Installed!" },
                { Reason.UnableToAccessTheChromeDriver, "Unable to Access the ChromeDriver Executable File!\r\n\r\n•  If this error persists, try to restart your computer or reinstall Scrupdate." },
                { Reason.GoogleChromeBrowserIsNotInstalled, "Google Chrome™ Browser Is Not Installed!" },
                { Reason.UnableToAccessTheGoogleChromeBrowser, "Unable to Access the Google Chrome™ Browser Executable File!\r\n\r\n•  If this error persists, try to restart your computer or reinstall Scrupdate." },
                { Reason.UnableToGetDefaultChromeDriverUserAgent, "Unable to Get Default ChromeDriver User-Agent String!\r\n\r\n•  If this error persists, set a custom ChromeDriver user-agent string in the settings\r\n    (In the 'ChromeDriver' tab under the 'ChromeDriver User-Agent String' Field)." },
                { Reason.ChromeDriverError, "The ChromeDriver's Version Is Not Compatible with the Version of the Installed Google Chrome™ Browser\r\nor the Browser Cannot Be Opened!" }
            };
            private Reason reason;
            public ProgramUpdatesCheckWasFailedException(Reason reason) :
                base(EXCEPTION_MESSAGE.Replace("{*}", REASONS[reason]))
            {
                this.reason = reason;
            }
            public Reason GetReason()
            {
                return reason;
            }
            public string GetReasonString()
            {
                return REASONS[reason];
            }
            public string GetLongReasonString()
            {
                return LONG_REASONS[reason];
            }
        }
        public class ProgramVersionSearchingWasFailedException : Exception
        {
            public enum Reason
            {
                Unknown,
                WebPageDidNotRespond,
                HtmlElementWasNotFound,
                TextWasNotFoundWithinTheWebPage,
                NoVersionWasFound
            }
            private const string EXCEPTION_MESSAGE = "Program Version Searching Was Failed (Reason: {*})!";
            private static readonly Dictionary<Reason, string> REASONS = new Dictionary<Reason, string>()
            {
                { Reason.Unknown, "Unknown!" },
                { Reason.WebPageDidNotRespond, "Web page did not respond!" },
                { Reason.HtmlElementWasNotFound, "HTML element was not found!" },
                { Reason.TextWasNotFoundWithinTheWebPage, "Text was not found within the web page!" },
                { Reason.NoVersionWasFound, "No version was found!" }
            };
            private Reason reason;
            public ProgramVersionSearchingWasFailedException(Reason reason) :
                base(EXCEPTION_MESSAGE.Replace("{*}", REASONS[reason]))
            {
                this.reason = reason;
            }
            public Reason GetReason()
            {
                return reason;
            }
            public string GetReasonString()
            {
                return REASONS[reason];
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Delegates ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public delegate void ProgramUpdatesCheckProgressChangedEventHandler(double progress);
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const int INITIAL_CAPACITY_OF_INSTALLED_PROGRAMS_STRING_BUILDER = 100000;
        private const int BROWSER_WINDOW_CLOSING_DELAY_IN_MILLISECONDS = 1000;
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
        public static void ScanForInstalledProgramsAndUpdateProgramDatabase(ProgramDatabase programDatabase,
                                                                            SettingsHandler settingsHandler,
                                                                            CancellationToken? cancellationToken)
        {
            if (programDatabase == null)
                throw new ArgumentNullException(nameof(programDatabase));
            if (settingsHandler == null)
                throw new ArgumentNullException(nameof(settingsHandler));
            if (!programDatabase.IsOpen())
                throw new ProgramDatabaseIsNotOpenException();
            if (settingsHandler.SettingsInMemory == null)
                throw new SettingsHandler.NoSettingsInMemoryException();
            RegistryKey installedUserProgramsRegistryKey = Registry.CurrentUser?
                .OpenSubKey("SOFTWARE")?
                .OpenSubKey("Microsoft")?
                .OpenSubKey("Windows")?
                .OpenSubKey("CurrentVersion")?
                .OpenSubKey("Uninstall");
            RegistryKey installedSystem32BitProgramsRegistryKey = Registry.LocalMachine?
                .OpenSubKey("SOFTWARE")?
                .OpenSubKey("WOW6432Node")?
                .OpenSubKey("Microsoft")?
                .OpenSubKey("Windows")?
                .OpenSubKey("CurrentVersion")?
                .OpenSubKey("Uninstall");
            RegistryKey installedSystem64BitProgramsRegistryKey = Registry.LocalMachine?
                .OpenSubKey("SOFTWARE")?
                .OpenSubKey("Microsoft")?
                .OpenSubKey("Windows")?
                .OpenSubKey("CurrentVersion")?
                .OpenSubKey("Uninstall");
            string[] installedUserProgramsRegistrySubKeysNames =
                installedUserProgramsRegistryKey?.GetSubKeyNames();
            string[] installedSystem32BitProgramsRegistrySubKeysNames =
                installedSystem32BitProgramsRegistryKey?.GetSubKeyNames();
            string[] installedSystem64BitProgramsRegistrySubKeysNames =
                installedSystem64BitProgramsRegistryKey?.GetSubKeyNames();
            StringBuilder stringOfAllInstalledPrograms = new StringBuilder(
                INITIAL_CAPACITY_OF_INSTALLED_PROGRAMS_STRING_BUILDER
            );
            Dictionary<string, Program> installedPrograms = new Dictionary<string, Program>();
            for (ProgramType installedProgramsType = (ProgramType)1;
                 ((int)installedProgramsType) < Enum.GetNames(typeof(ProgramType)).Length;
                 installedProgramsType++)
            {
                RegistryKey installedProgramsRegistryKey = null;
                string[] installedProgramsRegistrySubKeysNames = null;
                Program._InstallationScope installedProgramsInstallationScope =
                    Program._InstallationScope.None;
                switch (installedProgramsType)
                {
                    case ProgramType.UserProgram:
                        installedProgramsRegistryKey = installedUserProgramsRegistryKey;
                        installedProgramsRegistrySubKeysNames = installedUserProgramsRegistrySubKeysNames;
                        installedProgramsInstallationScope = Program._InstallationScope.User;
                        break;
                    case ProgramType.System32BitProgram:
                        installedProgramsRegistryKey = installedSystem32BitProgramsRegistryKey;
                        installedProgramsRegistrySubKeysNames = installedSystem32BitProgramsRegistrySubKeysNames;
                        installedProgramsInstallationScope = Program._InstallationScope.Everyone;
                        break;
                    case ProgramType.System64BitProgram:
                        installedProgramsRegistryKey = installedSystem64BitProgramsRegistryKey;
                        installedProgramsRegistrySubKeysNames = installedSystem64BitProgramsRegistrySubKeysNames;
                        installedProgramsInstallationScope = Program._InstallationScope.Everyone;
                        break;
                }
                if (installedProgramsRegistryKey != null &&
                    installedProgramsRegistrySubKeysNames != null)
                {
                    foreach (string installedProgramRegistrySubKeyName in installedProgramsRegistrySubKeysNames)
                    {
                        if (cancellationToken != null &&
                            cancellationToken.Value.IsCancellationRequested)
                        {
                            return;
                        }
                        string installedProgramName = (string)installedProgramsRegistryKey.OpenSubKey(
                            installedProgramRegistrySubKeyName
                        ).GetValue("DisplayName");
                        string installedProgramInstalledVersion = (string)installedProgramsRegistryKey.OpenSubKey(
                            installedProgramRegistrySubKeyName
                        ).GetValue("DisplayVersion");
                        if (installedProgramInstalledVersion != null)
                        {
                            if (!VersionUtilities.IsVersion(
                                    installedProgramInstalledVersion,
                                    VersionUtilities.VersionValidation.ValidateVersionSegmentsCount
                                ))
                            {
                                installedProgramInstalledVersion = new string(
                                    Array.FindAll(
                                        installedProgramInstalledVersion.ToCharArray(),
                                        c => (char.IsDigit(c) || c == '.')
                                    )
                                );
                            }
                            try
                            {
                                installedProgramInstalledVersion = VersionUtilities.NormalizeAndTrimVersion(
                                    installedProgramInstalledVersion,
                                    VersionUtilities.MINIMUM_VERSION_SEGMENTS,
                                    VersionUtilities.MAXIMUM_VERSION_SEGMENTS
                                );
                            }
                            catch
                            {
                                installedProgramInstalledVersion = null;
                            }
                        }
                        Program._InstallationScope installedProgramInstallationScope = installedProgramsInstallationScope;
                        if (installedProgramName != null)
                        {
                            stringOfAllInstalledPrograms
                                .Append(installedProgramName)
                                .Append((installedProgramInstalledVersion ?? ""))
                                .Append(Convert.ToString((long)installedProgramInstallationScope));
                            if (installedProgramInstallationScope == Program._InstallationScope.Everyone)
                            {
                                stringOfAllInstalledPrograms.Append(
                                    (installedProgramsType == ProgramType.System32BitProgram ?
                                        "32" :
                                        (installedProgramsType == ProgramType.System64BitProgram ?
                                            "64" :
                                            ""))
                                );
                            }
                            string versionFromProgramName;
                            installedProgramName = VersionUtilities.GetStringWithoutTheFirstFoundVersion(
                                installedProgramName.Trim(),
                                false,
                                false,
                                true,
                                out versionFromProgramName
                            );
                            if (versionFromProgramName != null &&
                                (installedProgramInstalledVersion == null || installedProgramInstalledVersion.Equals("")))
                            {
                                installedProgramInstalledVersion = versionFromProgramName;
                            }
                            if (!installedPrograms.ContainsKey(installedProgramName))
                            {
                                Program installedProgram = new Program(
                                    installedProgramName,
                                    (installedProgramInstalledVersion ?? ""),
                                    "",
                                    installedProgramInstallationScope,
                                    true
                                );
                                installedPrograms.Add(installedProgram.Name, installedProgram);
                            }
                            else if (installedProgramInstalledVersion != null)
                            {
                                Program programAlreadyFound = installedPrograms[installedProgramName];
                                if (VersionUtilities.IsVersionNewer(
                                        installedProgramInstalledVersion,
                                        programAlreadyFound.InstalledVersion
                                    ))
                                {
                                    programAlreadyFound.InstalledVersion = installedProgramInstalledVersion;
                                    programAlreadyFound.InstallationScope = installedProgramInstallationScope;
                                }
                            }
                        }
                    }
                }
            }
            string hashOfStringOfAllInstalledPrograms = HashingUtilities.GetMD5Hash(
                stringOfAllInstalledPrograms.ToString()
            );
            if (!settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms.Equals(
                    hashOfStringOfAllInstalledPrograms
                ))
            {
                programDatabase.BeginTransaction();
                for (Dictionary<string, Program>.Enumerator i = programDatabase.GetPrograms().GetEnumerator();
                     i.MoveNext();)
                {
                    Program programAlreadyInDatabase = i.Current.Value;
                    if (!installedPrograms.ContainsKey(programAlreadyInDatabase.Name))
                    {
                        programDatabase.UpdateProgramInstallationInfoToAutomaticallyDetectedProgram(
                            programAlreadyInDatabase.Name,
                            "",
                            Program._InstallationScope.None
                        );
                        programAlreadyInDatabase.InstalledVersion = "";
                        programAlreadyInDatabase.InstallationScope = Program._InstallationScope.None;
                    }
                    else
                    {
                        Program installedProgram = installedPrograms[programAlreadyInDatabase.Name];
                        installedPrograms[programAlreadyInDatabase.Name] = null;
                        programDatabase.UpdateProgramInstallationInfoToAutomaticallyDetectedProgram(
                            programAlreadyInDatabase.Name,
                            installedProgram.InstalledVersion,
                            installedProgram.InstallationScope
                        );
                        programAlreadyInDatabase.InstalledVersion = installedProgram.InstalledVersion;
                        programAlreadyInDatabase.InstallationScope = installedProgram.InstallationScope;
                    }
                    if (!programAlreadyInDatabase.SkippedVersion.Equals("") &&
                        (!programAlreadyInDatabase.InstalledVersion.Equals("") &&
                         !VersionUtilities.IsVersionNewer(
                             programAlreadyInDatabase.SkippedVersion,
                             programAlreadyInDatabase.InstalledVersion
                         )))
                    {
                        programDatabase.UnskipVersionOfProgram(programAlreadyInDatabase.Name);
                    }
                }
                programDatabase.EndTransaction();
                programDatabase.BeginTransaction();
                for (Dictionary<string, Program>.Enumerator i = installedPrograms.GetEnumerator();
                     i.MoveNext();)
                {
                    Program foundProgram = i.Current.Value;
                    if (foundProgram != null)
                        programDatabase.AddNewProgram(foundProgram);
                }
                programDatabase.EndTransaction();
                string backupOfLastHashOfAllInstalledPrograms =
                    settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms;
                settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms =
                    hashOfStringOfAllInstalledPrograms;
                if (!settingsHandler.SaveSettingsFromMemoryToSettingsFile())
                {
                    settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms =
                        backupOfLastHashOfAllInstalledPrograms;
                }
            }
        }
        public static void CheckForProgramUpdatesAndUpdateDatabase(List<Program> programsToCheck,
                                                                   ProgramDatabase programDatabase,
                                                                   SettingsHandler settingsHandler,
                                                                   ProgramUpdatesCheckProgressChangedEventHandler updatesCheckProgressChangedEventHandler,
                                                                   CancellationToken? cancellationToken)
        {
            if (programsToCheck == null)
                throw new ArgumentNullException(nameof(programsToCheck));
            if (programDatabase == null)
                throw new ArgumentNullException(nameof(programDatabase));
            if (settingsHandler == null)
                throw new ArgumentNullException(nameof(settingsHandler));
            if (settingsHandler.SettingsInMemory == null)
                throw new SettingsHandler.NoSettingsInMemoryException();
            string defaultChromeDriverUserAgentString;
            CheckChromeDriverAndBrowserAndUpdateCachedInformation(
                settingsHandler,
                out defaultChromeDriverUserAgentString
            );
            if (cancellationToken != null &&
                cancellationToken.Value.IsCancellationRequested)
            {
                return;
            }
            string chromeDriverUserAgentString =
                (settingsHandler.SettingsInMemory.ChromeDriver.UseCustomUserAgentString ?
                    settingsHandler.SettingsInMemory.ChromeDriver.CustomUserAgentString :
                    defaultChromeDriverUserAgentString);
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
            using (ChromeDriver chromeDriver = new ChromeDriver(
                       ChromeDriverUtilities.chromeDriverDirectoryPath,
                       chromeDriverUserAgentString,
                       chromeDriverPageLoadTimeoutInMilliseconds
                   ))
            {
                try
                {
                    chromeDriver.Open(
                        true,
                        settingsHandler.SettingsInMemory.ChromeDriver.UseIncognitoMode
                    );
                }
                catch
                {
                    throw new ProgramUpdatesCheckWasFailedException(
                        ProgramUpdatesCheckWasFailedException.Reason.ChromeDriverError
                    );
                }
                if (cancellationToken != null &&
                    cancellationToken.Value.IsCancellationRequested)
                {
                    return;
                }
                programDatabase.ResetLatestVersionAndUpdateCheckConfigurationStatusOfAllConfiguredPrograms();
                settingsHandler.SettingsInMemory.Cached.LastProgramUpdatesCheckTime = DateTime.Now;
                settingsHandler.SaveSettingsFromMemoryToSettingsFile();
                for (int i = 0; i < programsToCheck.Count; i++)
                {
                    Program programToCheck = programsToCheck[i];
                    updatesCheckProgressChangedEventHandler?.Invoke(((double)i / programsToCheck.Count) * 100.0D);
                    if (cancellationToken != null &&
                        cancellationToken.Value.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        string programLatestVersionString = GetLatestVersionOfAProgramFromWebPage(
                            chromeDriver,
                            programToCheck,
                            cancellationToken
                        );
                        if (programLatestVersionString == null)
                            return;
                        programLatestVersionString = VersionUtilities.NormalizeAndTrimVersion(
                            programLatestVersionString,
                            VersionUtilities.MINIMUM_VERSION_SEGMENTS,
                            VersionUtilities.MAXIMUM_VERSION_SEGMENTS
                        );
                        programDatabase.UpdateProgramLatestVersion(
                            programToCheck.Name,
                            programLatestVersionString
                        );
                        if (!programToCheck.SkippedVersion.Equals("") &&
                            VersionUtilities.IsVersionNewer(
                                programLatestVersionString,
                                programToCheck.SkippedVersion
                            ))
                        {
                            programDatabase.UnskipVersionOfProgram(programToCheck.Name);
                        }
                        programDatabase.ChangeProgramUpdateCheckConfigurationStatus(
                            programToCheck.Name,
                            Program._UpdateCheckConfigurationStatus.Valid,
                            Program._UpdateCheckConfigurationError.None
                        );
                    }
                    catch (Exception e)
                    {
                        Program._UpdateCheckConfigurationError updateCheckConfigurationError =
                            Program._UpdateCheckConfigurationError.GeneralFailure;
                        if (e.GetType().Equals(typeof(ProgramVersionSearchingWasFailedException)))
                        {
                            ProgramVersionSearchingWasFailedException.Reason reason =
                                ((ProgramVersionSearchingWasFailedException)e).GetReason();
                            switch (reason)
                            {
                                case ProgramVersionSearchingWasFailedException.Reason.WebPageDidNotRespond:
                                    updateCheckConfigurationError = Program._UpdateCheckConfigurationError.WebPageDidNotRespond;
                                    break;
                                case ProgramVersionSearchingWasFailedException.Reason.HtmlElementWasNotFound:
                                    updateCheckConfigurationError = Program._UpdateCheckConfigurationError.HtmlElementWasNotFound;
                                    break;
                                case ProgramVersionSearchingWasFailedException.Reason.TextWasNotFoundWithinTheWebPage:
                                    updateCheckConfigurationError = Program._UpdateCheckConfigurationError.TextWasNotFoundWithinTheWebPage;
                                    break;
                                case ProgramVersionSearchingWasFailedException.Reason.NoVersionWasFound:
                                    updateCheckConfigurationError = Program._UpdateCheckConfigurationError.NoVersionWasFound;
                                    break;
                            }
                        }
                        programDatabase.UpdateProgramLatestVersion(programToCheck.Name, "");
                        programDatabase.ChangeProgramUpdateCheckConfigurationStatus(
                            programToCheck.Name,
                            Program._UpdateCheckConfigurationStatus.Invalid,
                            updateCheckConfigurationError
                        );
                    }
                }
                settingsHandler.SettingsInMemory.Cached.LastProgramUpdatesCheckTime = DateTime.Now;
                settingsHandler.SaveSettingsFromMemoryToSettingsFile();
                updatesCheckProgressChangedEventHandler?.Invoke(100.0D);
            }
        }
        public static string CheckForAProgramUpdateAndGetLatestVersion(Program programToCheck,
                                                                       bool showBrowserWindow,
                                                                       SettingsHandler settingsHandler,
                                                                       CancellationToken? cancellationToken,
                                                                       out Program._UpdateCheckConfigurationError updateCheckConfigurationError)
        {
            if (programToCheck == null)
                throw new ArgumentNullException(nameof(programToCheck));
            if (settingsHandler == null)
                throw new ArgumentNullException(nameof(settingsHandler));
            if (settingsHandler.SettingsInMemory == null)
                throw new SettingsHandler.NoSettingsInMemoryException();
            updateCheckConfigurationError = Program._UpdateCheckConfigurationError.None;
            string defaultChromeDriverUserAgentString;
            CheckChromeDriverAndBrowserAndUpdateCachedInformation(
                settingsHandler,
                out defaultChromeDriverUserAgentString
            );
            if (cancellationToken != null &&
                cancellationToken.Value.IsCancellationRequested)
            {
                return null;
            }
            string chromeDriverUserAgentString =
                (settingsHandler.SettingsInMemory.ChromeDriver.UseCustomUserAgentString ?
                    settingsHandler.SettingsInMemory.ChromeDriver.CustomUserAgentString :
                    defaultChromeDriverUserAgentString);
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
            using (ChromeDriver chromeDriver = new ChromeDriver(
                       ChromeDriverUtilities.chromeDriverDirectoryPath,
                       chromeDriverUserAgentString,
                       chromeDriverPageLoadTimeoutInMilliseconds
                   ))
            {
                try
                {
                    chromeDriver.Open(
                        !showBrowserWindow,
                        settingsHandler.SettingsInMemory.ChromeDriver.UseIncognitoMode
                    );
                }
                catch
                {
                    throw new ProgramUpdatesCheckWasFailedException(
                        ProgramUpdatesCheckWasFailedException.Reason.ChromeDriverError
                    );
                }
                if (cancellationToken != null &&
                    cancellationToken.Value.IsCancellationRequested)
                {
                    return null;
                }
                try
                {
                    string programLatestVersionString = GetLatestVersionOfAProgramFromWebPage(
                        chromeDriver,
                        programToCheck,
                        cancellationToken
                    );
                    if (programLatestVersionString == null)
                        return null;
                    return VersionUtilities.NormalizeAndTrimVersion(
                        programLatestVersionString,
                        VersionUtilities.MINIMUM_VERSION_SEGMENTS,
                        VersionUtilities.MAXIMUM_VERSION_SEGMENTS
                    );
                }
                catch (Exception e)
                {
                    updateCheckConfigurationError = Program._UpdateCheckConfigurationError.GeneralFailure;
                    if (e.GetType().Equals(typeof(ProgramVersionSearchingWasFailedException)))
                    {
                        ProgramVersionSearchingWasFailedException.Reason reason =
                            ((ProgramVersionSearchingWasFailedException)e).GetReason();
                        switch (reason)
                        {
                            case ProgramVersionSearchingWasFailedException.Reason.WebPageDidNotRespond:
                                updateCheckConfigurationError = Program._UpdateCheckConfigurationError.WebPageDidNotRespond;
                                break;
                            case ProgramVersionSearchingWasFailedException.Reason.HtmlElementWasNotFound:
                                updateCheckConfigurationError = Program._UpdateCheckConfigurationError.HtmlElementWasNotFound;
                                break;
                            case ProgramVersionSearchingWasFailedException.Reason.TextWasNotFoundWithinTheWebPage:
                                updateCheckConfigurationError = Program._UpdateCheckConfigurationError.TextWasNotFoundWithinTheWebPage;
                                break;
                            case ProgramVersionSearchingWasFailedException.Reason.NoVersionWasFound:
                                updateCheckConfigurationError = Program._UpdateCheckConfigurationError.NoVersionWasFound;
                                break;
                        }
                    }
                }
                finally
                {
                    if (showBrowserWindow)
                    {
                        if (cancellationToken != null)
                        {
                            cancellationToken.Value.WaitHandle.WaitOne(
                                BROWSER_WINDOW_CLOSING_DELAY_IN_MILLISECONDS
                            );
                        }
                        else
                            Thread.Sleep(BROWSER_WINDOW_CLOSING_DELAY_IN_MILLISECONDS);
                    }
                }
                return null;
            }
        }
        private static void CheckChromeDriverAndBrowserAndUpdateCachedInformation(SettingsHandler settingsHandler,
                                                                                  out string defaultChromeDriverUserAgentString)
        {
            if (settingsHandler == null)
                throw new ArgumentNullException(nameof(settingsHandler));
            bool unableToAccessInstalledChromeDriverExecutableFile;
            if (ChromeDriverUtilities.GetInstalledChromeDriverInformation(
                    out unableToAccessInstalledChromeDriverExecutableFile
                ) == null)
            {
                if (unableToAccessInstalledChromeDriverExecutableFile)
                {
                    throw new ProgramUpdatesCheckWasFailedException(
                        ProgramUpdatesCheckWasFailedException.Reason.UnableToAccessTheChromeDriver
                    );
                }
                throw new ProgramUpdatesCheckWasFailedException(
                    ProgramUpdatesCheckWasFailedException.Reason.NoChromeDriverIsInstalled
                );
            }
            if (!GoogleChromeBrowserUtilities.IsGoogleChromeBrowserInstalled())
            {
                throw new ProgramUpdatesCheckWasFailedException(
                    ProgramUpdatesCheckWasFailedException.Reason.GoogleChromeBrowserIsNotInstalled
                );
            }
            string checksumOfInstalledGoogleChromeBrowserExecutableFile =
                GoogleChromeBrowserUtilities.GetChecksumOfInstalledGoogleChromeBrowserExecutableFile();
            if (checksumOfInstalledGoogleChromeBrowserExecutableFile == null)
            {
                throw new ProgramUpdatesCheckWasFailedException(
                    ProgramUpdatesCheckWasFailedException.Reason.UnableToAccessTheGoogleChromeBrowser
                );
            }
            defaultChromeDriverUserAgentString =
                settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString;
            if (settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString.Equals("") ||
                !checksumOfInstalledGoogleChromeBrowserExecutableFile.Equals(
                    settingsHandler.SettingsInMemory.Cached.LastChecksumOfInstalledGoogleChromeBrowserExecutableFile
                ))
            {
                defaultChromeDriverUserAgentString = ChromeDriverUtilities.GetDefaultChromeDriverUserAgentString();
                if (defaultChromeDriverUserAgentString == null)
                {
                    throw new ProgramUpdatesCheckWasFailedException(
                        ProgramUpdatesCheckWasFailedException.Reason.UnableToGetDefaultChromeDriverUserAgent
                    );
                }
                string backupOfLastChecksumOfInstalledGoogleChromeBrowserExecutableFile =
                    settingsHandler.SettingsInMemory.Cached.LastChecksumOfInstalledGoogleChromeBrowserExecutableFile;
                string backupOfLastDefaultChromeDriverUserAgentString =
                    settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString;
                settingsHandler.SettingsInMemory.Cached.LastChecksumOfInstalledGoogleChromeBrowserExecutableFile =
                    checksumOfInstalledGoogleChromeBrowserExecutableFile;
                settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString =
                    defaultChromeDriverUserAgentString;
                if (!settingsHandler.SaveSettingsFromMemoryToSettingsFile())
                {
                    settingsHandler.SettingsInMemory.Cached.LastChecksumOfInstalledGoogleChromeBrowserExecutableFile =
                        backupOfLastChecksumOfInstalledGoogleChromeBrowserExecutableFile;
                    settingsHandler.SettingsInMemory.Cached.LastDefaultChromeDriverUserAgentString =
                        backupOfLastDefaultChromeDriverUserAgentString;
                }
            }
        }
        private static string GetLatestVersionOfAProgramFromWebPage(ChromeDriver chromeDriver,
                                                                    Program programToCheck,
                                                                    CancellationToken? cancellationToken)
        {
            if (chromeDriver == null)
                throw new ArgumentNullException(nameof(chromeDriver));
            if (programToCheck == null)
                throw new ArgumentNullException(nameof(programToCheck));
            try
            {
                chromeDriver.NavigateToAWebPage(programToCheck.WebPageUrl);
            }
            catch
            {
                throw new ProgramVersionSearchingWasFailedException(
                    ProgramVersionSearchingWasFailedException.Reason.WebPageDidNotRespond
                );
            }
            if (cancellationToken != null &&
                cancellationToken.Value.IsCancellationRequested)
            {
                return null;
            }
            int webPagePostLoadDelayInMilliseconds = 0;
            switch (programToCheck.WebPagePostLoadDelay)
            {
                case Program._WebPagePostLoadDelay._100Ms:
                    webPagePostLoadDelayInMilliseconds = 100;
                    break;
                case Program._WebPagePostLoadDelay._250Ms:
                    webPagePostLoadDelayInMilliseconds = 250;
                    break;
                case Program._WebPagePostLoadDelay._500Ms:
                    webPagePostLoadDelayInMilliseconds = 500;
                    break;
                case Program._WebPagePostLoadDelay._1000Ms:
                    webPagePostLoadDelayInMilliseconds = 1000;
                    break;
                case Program._WebPagePostLoadDelay._2000Ms:
                    webPagePostLoadDelayInMilliseconds = 2000;
                    break;
                case Program._WebPagePostLoadDelay._3000Ms:
                    webPagePostLoadDelayInMilliseconds = 3000;
                    break;
                case Program._WebPagePostLoadDelay._4000Ms:
                    webPagePostLoadDelayInMilliseconds = 4000;
                    break;
                case Program._WebPagePostLoadDelay._5000Ms:
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
            if (cancellationToken != null &&
                cancellationToken.Value.IsCancellationRequested)
            {
                return null;
            }
            if (programToCheck.LocatingInstructionsOfWebPageElementsToSimulateAClickOn.Count > 0)
            {
                foreach (WebPageElementLocatingInstruction locatingInstructionOfWebPageElementToSimulateAClickOn in
                         programToCheck.LocatingInstructionsOfWebPageElementsToSimulateAClickOn)
                {
                    if (cancellationToken != null &&
                        cancellationToken.Value.IsCancellationRequested)
                    {
                        return null;
                    }
                    try
                    {
                        chromeDriver.ClickOnAnElementWithinTheWebpage(
                            locatingInstructionOfWebPageElementToSimulateAClickOn,
                            cancellationToken
                        );
                    }
                    catch { }
                }
            }
            if (cancellationToken != null &&
                cancellationToken.Value.IsCancellationRequested)
            {
                return null;
            }
            string textToSerachVersion = null;
            string tempTextToSerachVersion = null;
            switch (programToCheck.VersionSearchMethod)
            {
                case Program._VersionSearchMethod.SearchWithinTheHtmlElementWithId:
                    try
                    {
                        textToSerachVersion = chromeDriver.GetTextInsideHtmlElementById(
                            programToCheck.VersionSearchMethodArgument1
                        );
                    }
                    catch
                    {
                        throw new ProgramVersionSearchingWasFailedException(
                            ProgramVersionSearchingWasFailedException.Reason.HtmlElementWasNotFound
                        );
                    }
                    break;
                case Program._VersionSearchMethod.SearchWithinTheHtmlElementsThatMatchXPath:
                    try
                    {
                        textToSerachVersion = string.Join(
                            " ",
                            chromeDriver.GetTextsInsideHtmlElementsByXPath(
                                programToCheck.VersionSearchMethodArgument1
                            )
                        );
                    }
                    catch
                    {
                        throw new ProgramVersionSearchingWasFailedException(
                            ProgramVersionSearchingWasFailedException.Reason.HtmlElementWasNotFound
                        );
                    }
                    break;
                case Program._VersionSearchMethod.SearchGloballyWithinTheWebPage:
                    textToSerachVersion = chromeDriver.GetAllTextWithinTheWebPage();
                    break;
                case Program._VersionSearchMethod.SearchGloballyFromTextWithinTheWebPage:
                case Program._VersionSearchMethod.SearchGloballyUntilTextWithinTheWebPage:
                    tempTextToSerachVersion = chromeDriver.GetAllTextWithinTheWebPage();
                    int foundTextIndex = tempTextToSerachVersion.IndexOf(
                        programToCheck.VersionSearchMethodArgument1
                    );
                    if (foundTextIndex < 0)
                    {
                        throw new ProgramVersionSearchingWasFailedException(
                            ProgramVersionSearchingWasFailedException.Reason.TextWasNotFoundWithinTheWebPage
                        );
                    }
                    if (programToCheck.VersionSearchMethod == Program._VersionSearchMethod.SearchGloballyFromTextWithinTheWebPage)
                    {
                        textToSerachVersion = tempTextToSerachVersion.Substring(
                            foundTextIndex + programToCheck.VersionSearchMethodArgument1.Length
                        );
                    }
                    else
                        textToSerachVersion = tempTextToSerachVersion.Substring(0, foundTextIndex);
                    break;
                case Program._VersionSearchMethod.SearchGloballyFromTextUntilTextWithinTheWebPage:
                    tempTextToSerachVersion = chromeDriver.GetAllTextWithinTheWebPage();
                    int foundStartingTextIndex = tempTextToSerachVersion.IndexOf(
                        programToCheck.VersionSearchMethodArgument1
                    );
                    int foundEndingTextIndex = tempTextToSerachVersion.IndexOf(
                        programToCheck.VersionSearchMethodArgument2
                    );
                    if (foundStartingTextIndex < 0 || foundEndingTextIndex < 0)
                    {
                        throw new ProgramVersionSearchingWasFailedException(
                            ProgramVersionSearchingWasFailedException.Reason.TextWasNotFoundWithinTheWebPage
                        );
                    }
                    if (foundEndingTextIndex - foundStartingTextIndex >= programToCheck.VersionSearchMethodArgument1.Length)
                    {
                        textToSerachVersion = tempTextToSerachVersion.Substring(
                            foundStartingTextIndex + programToCheck.VersionSearchMethodArgument1.Length,
                            foundEndingTextIndex - (foundStartingTextIndex + programToCheck.VersionSearchMethodArgument1.Length)
                        );
                    }
                    break;
            }
            if (textToSerachVersion == null || textToSerachVersion.Equals(""))
            {
                throw new ProgramVersionSearchingWasFailedException(
                    ProgramVersionSearchingWasFailedException.Reason.NoVersionWasFound
                );
            }
            string programLatestVersionString = null;
            switch (programToCheck.VersionSearchBehavior)
            {
                case Program._VersionSearchBehavior.GetTheFirstVersionThatIsFound:
                case Program._VersionSearchBehavior.GetTheFirstVersionThatIsFoundFromTheEnd:
                    programLatestVersionString = VersionUtilities.GetTheFirstFoundVersionFromString(
                        textToSerachVersion,
                        programToCheck.TreatAStandaloneNumberAsAVersion,
                        (programToCheck.VersionSearchBehavior == Program._VersionSearchBehavior.GetTheFirstVersionThatIsFoundFromTheEnd)
                    );
                    break;
                case Program._VersionSearchBehavior.GetTheLatestVersionFromAllTheVersionsThatAreFound:
                    programLatestVersionString = VersionUtilities.GetTheLatestVersionFromString(
                        textToSerachVersion,
                        programToCheck.TreatAStandaloneNumberAsAVersion
                    );
                    break;
            }
            if (programLatestVersionString == null || programLatestVersionString.Equals(""))
            {
                throw new ProgramVersionSearchingWasFailedException(
                    ProgramVersionSearchingWasFailedException.Reason.NoVersionWasFound
                );
            }
            return programLatestVersionString;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
