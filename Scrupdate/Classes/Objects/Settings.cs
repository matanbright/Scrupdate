// Copyright © 2021 Matan Brightbert
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
using System.Windows;

namespace Scrupdate.Classes.Objects
{
    public class Settings
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class CachedSettings
        {
            // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
                OnlyValid,
                OnlyInvalid,
                OnlyNotChecked,
                OnlyNotConfigured
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public WindowState? LastWindowState { get; set; }
            public Size? LastWindowSize { get; set; }
            public Point? LastWindowLocation { get; set; }
            public string LastHashOfAllInstalledPrograms { get; set; }
            public bool LastProgramFilteringState { get; set; }
            public ProgramFilteringOption LastProgramFilteringOption { get; set; }
            public bool LastShowHiddenProgramsState { get; set; }
            public DateTime LastProgramUpdatesCheckTime { get; set; }
            public DateTime LastProgramUpdatesScheduledCheckAttemptionTime { get; set; }
            public string LastChecksumOfInstalledGoogleChromeBrowserExecutableFile { get; set; }
            public string LastDefaultChromeDriverUserAgentString { get; set; }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public CachedSettings()
            {
                LastWindowState = null;
                LastWindowSize = null;
                LastWindowLocation = null;
                LastHashOfAllInstalledPrograms = "";
                LastProgramFilteringState = false;
                LastProgramFilteringOption = ProgramFilteringOption.Unknown;
                LastShowHiddenProgramsState = false;
                LastProgramUpdatesCheckTime = new DateTime();
                LastProgramUpdatesScheduledCheckAttemptionTime = new DateTime();
                LastChecksumOfInstalledGoogleChromeBrowserExecutableFile = "";
                LastDefaultChromeDriverUserAgentString = "";
            }
            public CachedSettings(WindowState? lastWindowState, Size? lastWindowSize, Point? lastWindowLocation, string lastHashOfAllInstalledPrograms, bool lastProgramFilteringState, ProgramFilteringOption lastProgramFilteringOption, bool lastShowHiddenProgramsState, DateTime lastProgramUpdatesCheckTime, DateTime lastProgramUpdatesScheduledCheckAttemptionTime, string lastChecksumOfInstalledGoogleChromeBrowserExecutableFile, string lastDefaultChromeDriverUserAgentString)
            {
                LastWindowState = lastWindowState;
                LastWindowSize = lastWindowSize;
                LastWindowLocation = lastWindowLocation;
                LastHashOfAllInstalledPrograms = lastHashOfAllInstalledPrograms;
                LastProgramFilteringState = lastProgramFilteringState;
                LastProgramFilteringOption = lastProgramFilteringOption;
                LastShowHiddenProgramsState = lastShowHiddenProgramsState;
                LastProgramUpdatesCheckTime = lastProgramUpdatesCheckTime;
                LastProgramUpdatesScheduledCheckAttemptionTime = lastProgramUpdatesScheduledCheckAttemptionTime;
                LastChecksumOfInstalledGoogleChromeBrowserExecutableFile = lastChecksumOfInstalledGoogleChromeBrowserExecutableFile;
                LastDefaultChromeDriverUserAgentString = lastDefaultChromeDriverUserAgentString;
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class GlobalSettings
        {
            // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public bool EnableScanningForInstalledPrograms { get; set; }
            public bool ScanForInstalledProgramsAutomaticallyOnStart { get; set; }
            public bool RememberLastProgramListOptions { get; set; }
            public bool EnableScheduledCheckForProgramUpdates { get; set; }
            public WeekDays ProgramUpdatesScheduledCheckDays { get; set; }
            public int ProgramUpdatesScheduledCheckHour { get; set; }
            public bool IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults { get; set; }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public GlobalSettings()
            {
                EnableScanningForInstalledPrograms = true;
                ScanForInstalledProgramsAutomaticallyOnStart = true;
                RememberLastProgramListOptions = false;
                EnableScheduledCheckForProgramUpdates = false;
                ProgramUpdatesScheduledCheckDays = WeekDays.None;
                ProgramUpdatesScheduledCheckHour = 0;
                IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults = false;
            }
            public GlobalSettings(bool enableScanningForInstalledPrograms, bool scanForInstalledProgramsAutomaticallyOnStart, bool rememberLastProgramListOptions, bool enableScheduledCheckForProgramUpdates, WeekDays programUpdatesScheduledCheckDays, int programUpdatesScheduledCheckHour, bool includeHiddenProgramsInProgramUpdatesScheduledCheckResults)
            {
                EnableScanningForInstalledPrograms = enableScanningForInstalledPrograms;
                ScanForInstalledProgramsAutomaticallyOnStart = scanForInstalledProgramsAutomaticallyOnStart;
                RememberLastProgramListOptions = rememberLastProgramListOptions;
                EnableScheduledCheckForProgramUpdates = enableScheduledCheckForProgramUpdates;
                ProgramUpdatesScheduledCheckDays = programUpdatesScheduledCheckDays;
                ProgramUpdatesScheduledCheckHour = programUpdatesScheduledCheckHour;
                IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults = includeHiddenProgramsInProgramUpdatesScheduledCheckResults;
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class AppearanceSettings
        {
            // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public double WindowsScalingFactor { get; set; }
            public int MinimumVersionSegments { get; set; }
            public int MaximumVersionSegments { get; set; }
            public bool RemoveTrailingZeroSegmentsOfVersions { get; set; }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public AppearanceSettings()
            {
                WindowsScalingFactor = 0.0D;
                MinimumVersionSegments = 2;
                MaximumVersionSegments = 4;
                RemoveTrailingZeroSegmentsOfVersions = true;
            }
            public AppearanceSettings(double windowsScalingFactor, int minimumVersionSegments, int maximumVersionSegments, bool removeTrailingZeroSegmentsOfVersions)
            {
                WindowsScalingFactor = windowsScalingFactor;
                MinimumVersionSegments = minimumVersionSegments;
                MaximumVersionSegments = maximumVersionSegments;
                RemoveTrailingZeroSegmentsOfVersions = removeTrailingZeroSegmentsOfVersions;
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class ChromeDriverSettings
        {
            // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public ChromeDriverPageLoadTimeout PageLoadTimeout { get; set; }
            public bool UseCustomUserAgentString { get; set; }
            public string CustomUserAgentString { get; set; }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public ChromeDriverSettings()
            {
                PageLoadTimeout = ChromeDriverPageLoadTimeout.After15Seconds;
                UseCustomUserAgentString = false;
                CustomUserAgentString = "";
            }
            public ChromeDriverSettings(ChromeDriverPageLoadTimeout pageLoadTimeout, bool useCustomUserAgentString, string customUserAgentString)
            {
                PageLoadTimeout = pageLoadTimeout;
                UseCustomUserAgentString = useCustomUserAgentString;
                CustomUserAgentString = customUserAgentString;
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CachedSettings Cached { get; set; }
        public GlobalSettings Global { get; set; }
        public AppearanceSettings Appearance { get; set; }
        public ChromeDriverSettings ChromeDriver { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Settings()
        {
            Cached = new CachedSettings();
            Global = new GlobalSettings();
            Appearance = new AppearanceSettings();
            ChromeDriver = new ChromeDriverSettings();
        }
        public Settings(CachedSettings cached, GlobalSettings global, AppearanceSettings appearance, ChromeDriverSettings chromeDriver)
        {
            Cached = cached;
            Global = global;
            Appearance = appearance;
            ChromeDriver = chromeDriver;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
