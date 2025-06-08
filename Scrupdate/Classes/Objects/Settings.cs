// Copyright © 2021-2025 Matan Brightbert
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
using System.Text.Json;
using System.Windows;


namespace Scrupdate.Classes.Objects
{
    public class Settings
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class SettingsVersionIsNotCompatibleException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Settings version is not compatible!";
            public SettingsVersionIsNotCompatibleException() : base(EXCEPTION_MESSAGE) { }
        }
        public class CachedSettings
        {
            public enum ProgramFilteringOption
            {
                Unknown,
                All,
                OnlyUpdates,
                OnlyUpToDate,
                OnlyAutomaticallyAdded,
                OnlyManuallyAdded,
                OnlyInstalled,
                OnlyUninstalled,
                OnlySkipped,
                OnlyValid,
                OnlyInvalid,
                OnlyNotChecked,
                OnlyNotConfigured
            }
            public WindowState? LastWindowState { get; set; }
            public Size? LastWindowSize { get; set; }
            public Point? LastWindowLocation { get; set; }
            public string LastHashOfAllInstalledPrograms { get; set; }
            public bool LastProgramFilteringState { get; set; }
            public ProgramFilteringOption LastProgramFilteringOption { get; set; }
            public bool LastShowHiddenProgramsState { get; set; }
            public List<ProgramListViewColumnState> LastProgramListColumnStates { get; set; }
            public DateTime LastProgramUpdatesCheckTime { get; set; }
            public DateTime LastProgramUpdatesScheduledCheckAttemptionTime { get; set; }
            public string LastChecksumOfInstalledGoogleChromeBrowserExecutableFile { get; set; }
            public string LastDefaultChromeDriverUserAgentString { get; set; }
            public CachedSettings() :
                this(
                    null,
                    null,
                    null,
                    "",
                    false,
                    ProgramFilteringOption.Unknown,
                    false,
                    new List<ProgramListViewColumnState>(),
                    new DateTime(),
                    new DateTime(),
                    "",
                    ""
                )
            { }
            public CachedSettings(WindowState? lastWindowState,
                                  Size? lastWindowSize,
                                  Point? lastWindowLocation,
                                  string lastHashOfAllInstalledPrograms,
                                  bool lastProgramFilteringState,
                                  ProgramFilteringOption lastProgramFilteringOption,
                                  bool lastShowHiddenProgramsState,
                                  List<ProgramListViewColumnState> lastProgramListColumnStates,
                                  DateTime lastProgramUpdatesCheckTime,
                                  DateTime lastProgramUpdatesScheduledCheckAttemptionTime,
                                  string lastChecksumOfInstalledGoogleChromeBrowserExecutableFile,
                                  string lastDefaultChromeDriverUserAgentString)
            {
                LastWindowState = lastWindowState;
                LastWindowSize = lastWindowSize;
                LastWindowLocation = lastWindowLocation;
                LastHashOfAllInstalledPrograms = lastHashOfAllInstalledPrograms;
                LastProgramFilteringState = lastProgramFilteringState;
                LastProgramFilteringOption = lastProgramFilteringOption;
                LastShowHiddenProgramsState = lastShowHiddenProgramsState;
                LastProgramListColumnStates = lastProgramListColumnStates;
                LastProgramUpdatesCheckTime = lastProgramUpdatesCheckTime;
                LastProgramUpdatesScheduledCheckAttemptionTime = lastProgramUpdatesScheduledCheckAttemptionTime;
                LastChecksumOfInstalledGoogleChromeBrowserExecutableFile = lastChecksumOfInstalledGoogleChromeBrowserExecutableFile;
                LastDefaultChromeDriverUserAgentString = lastDefaultChromeDriverUserAgentString;
            }
        }
        public class GeneralSettings
        {
            public enum WeekDays
            {
                None = 0,
                Sunday = 1,
                Monday = (1 << 1),
                Tuesday = (1 << 2),
                Wednesday = (1 << 3),
                Thursday = (1 << 4),
                Friday = (1 << 5),
                Saturday = (1 << 6)
            }
            public bool EnableScanningForInstalledPrograms { get; set; }
            public bool ScanForInstalledProgramsAutomaticallyOnStart { get; set; }
            public bool RememberLastProgramListOptions { get; set; }
            public bool RememberLastProgramListColumnStates { get; set; }
            public bool EnableScheduledCheckForProgramUpdates { get; set; }
            public WeekDays ProgramUpdatesScheduledCheckDays { get; set; }
            public int ProgramUpdatesScheduledCheckHour { get; set; }
            public bool IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults { get; set; }
            public GeneralSettings() :
                this(
                    true,
                    true,
                    false,
                    false,
                    false,
                    WeekDays.None,
                    0,
                    false
                )
            { }
            public GeneralSettings(bool enableScanningForInstalledPrograms,
                                   bool scanForInstalledProgramsAutomaticallyOnStart,
                                   bool rememberLastProgramListOptions,
                                   bool rememberLastProgramListColumnStates,
                                   bool enableScheduledCheckForProgramUpdates,
                                   WeekDays programUpdatesScheduledCheckDays,
                                   int programUpdatesScheduledCheckHour,
                                   bool includeHiddenProgramsInProgramUpdatesScheduledCheckResults)
            {
                EnableScanningForInstalledPrograms = enableScanningForInstalledPrograms;
                ScanForInstalledProgramsAutomaticallyOnStart = scanForInstalledProgramsAutomaticallyOnStart;
                RememberLastProgramListOptions = rememberLastProgramListOptions;
                RememberLastProgramListColumnStates = rememberLastProgramListColumnStates;
                EnableScheduledCheckForProgramUpdates = enableScheduledCheckForProgramUpdates;
                ProgramUpdatesScheduledCheckDays = programUpdatesScheduledCheckDays;
                ProgramUpdatesScheduledCheckHour = programUpdatesScheduledCheckHour;
                IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults = includeHiddenProgramsInProgramUpdatesScheduledCheckResults;
            }
        }
        public class AppearanceSettings
        {
            public double WindowsScalingFactor { get; set; }
            public int MinimumVersionSegments { get; set; }
            public int MaximumVersionSegments { get; set; }
            public bool RemoveTrailingZeroSegmentsOfVersions { get; set; }
            public bool HighlightNewProgramsInTheList { get; set; }
            public AppearanceSettings() :
                this(
                    1.0D,
                    2,
                    4,
                    true,
                    true
                )
            { }
            public AppearanceSettings(double windowsScalingFactor,
                                      int minimumVersionSegments,
                                      int maximumVersionSegments,
                                      bool removeTrailingZeroSegmentsOfVersions,
                                      bool highlightNewProgramsInTheList)
            {
                WindowsScalingFactor = windowsScalingFactor;
                MinimumVersionSegments = minimumVersionSegments;
                MaximumVersionSegments = maximumVersionSegments;
                RemoveTrailingZeroSegmentsOfVersions = removeTrailingZeroSegmentsOfVersions;
                HighlightNewProgramsInTheList = highlightNewProgramsInTheList;
            }
        }
        public class ChromeDriverSettings
        {
            public enum ChromeDriverPageLoadTimeout
            {
                NoTimeout,
                After1Seconds,
                After3Seconds,
                After5Seconds,
                After10Seconds,
                After15Seconds,
                After30Seconds,
            }
            public ChromeDriverPageLoadTimeout PageLoadTimeout { get; set; }
            public bool UseCustomUserAgentString { get; set; }
            public string CustomUserAgentString { get; set; }
            public bool UseIncognitoMode { get; set; }
            public ChromeDriverSettings() :
                this(
                    ChromeDriverPageLoadTimeout.After15Seconds,
                    false,
                    "",
                    false
                )
            { }
            public ChromeDriverSettings(ChromeDriverPageLoadTimeout pageLoadTimeout,
                                        bool useCustomUserAgentString,
                                        string customUserAgentString,
                                        bool useIncognitoMode)
            {
                PageLoadTimeout = pageLoadTimeout;
                UseCustomUserAgentString = useCustomUserAgentString;
                CustomUserAgentString = customUserAgentString;
                UseIncognitoMode = useIncognitoMode;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static readonly Version SETTINGS_VERSION = new Version(1, 1); /* Note: When changing or removing existing settings, the major number should be incremented and the minor number should be zeroed.
                                                                               *       When only adding new settings, the minor number should be incremented.
                                                                               */
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool versionHasBeenChecked;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Version Version
        {
            get { return SETTINGS_VERSION; }
            set
            {
                versionHasBeenChecked = true;
                if (value.MajorNumber != SETTINGS_VERSION.MajorNumber)
                    throw new SettingsVersionIsNotCompatibleException();
            }
        }
        public CachedSettings Cached { get; set; }
        public GeneralSettings General { get; set; }
        public AppearanceSettings Appearance { get; set; }
        public ChromeDriverSettings ChromeDriver { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Settings() :
            this(
                new CachedSettings(),
                new GeneralSettings(),
                new AppearanceSettings(),
                new ChromeDriverSettings()
            )
        { }
        public Settings(CachedSettings cached,
                        GeneralSettings general,
                        AppearanceSettings appearance,
                        ChromeDriverSettings chromeDriver)
        {
            Cached = cached;
            General = general;
            Appearance = appearance;
            ChromeDriver = chromeDriver;
            versionHasBeenChecked = false;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool HasVersionBeenChecked()
        {
            return versionHasBeenChecked;
        }
        public string GetJsonRepresentation()
        {
            return JsonSerializer.Serialize(this);
        }
        public static Settings CreateFromJsonRepresentation(string jsonRepresentation)
        {
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonRepresentation);
            if (!settings.HasVersionBeenChecked())
                settings.Version = new Version();
            return settings;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
