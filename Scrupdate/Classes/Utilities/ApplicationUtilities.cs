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
using System.Text;
using System.IO;
using System.Windows;
using Scrupdate.Classes.Objects;
using Scrupdate.UiElements.Windows;

namespace Scrupdate.Classes.Utilities
{
    public static class ApplicationUtilities
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class UnableToOpenOrCreateSettingsFileException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE__UNABLE_TO_OPEN_OR_CREATE_SETTINGS_FILE = "Unable to open or create settings file!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public UnableToOpenOrCreateSettingsFileException() : base(EXCEPTION_MESSAGE__UNABLE_TO_OPEN_OR_CREATE_SETTINGS_FILE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string FILE_NAME__SETTINGS = "Settings.json";
        private const string FILE_NAME__PROGRAM_DATABASE = "Programs.sqlite";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly string dataDirectoryPath = ((new StringBuilder()).Append(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).Append('\\').Append(AppDomain.CurrentDomain.FriendlyName).Append("\\Data")).ToString();
        public static readonly string settingsFilePath = ((new StringBuilder()).Append(dataDirectoryPath).Append('\\').Append(FILE_NAME__SETTINGS)).ToString();
        public static readonly string settingsChecksumFilePath = ((new StringBuilder()).Append(settingsFilePath).Append(HashingUtilities.MD5_HASH_FILE_EXTENSION)).ToString();
        public static readonly string programDatabaseFilePath = ((new StringBuilder()).Append(dataDirectoryPath).Append('\\').Append(FILE_NAME__PROGRAM_DATABASE)).ToString();
        public static readonly string programDatabaseChecksumFilePath = ((new StringBuilder()).Append(programDatabaseFilePath).Append(HashingUtilities.MD5_HASH_FILE_EXTENSION)).ToString();
        public static readonly string[] programDatabaseTemporaryFilesPaths = new string[]
        {
            ((new StringBuilder()).Append(programDatabaseFilePath).Append("-journal")).ToString(),
            ((new StringBuilder()).Append(programDatabaseFilePath).Append("-wal")).ToString(),
            ((new StringBuilder()).Append(programDatabaseFilePath).Append("-shm")).ToString()
        };
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool IsItTimeForProgramUpdatesScheduledCheckAttemption(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException();
            try
            {
                if (!settings.Global.EnableScheduledCheckForProgramUpdates)
                    return false;
                DateTime lastProgramUpdatesScheduledCheckAttemptionTime = settings.Cached.LastProgramUpdatesScheduledCheckAttemptionTime;
                DateTime nextProgramUpdatesScheduledCheckAttemptionTime;
                if (lastProgramUpdatesScheduledCheckAttemptionTime.Hour < settings.Global.ProgramUpdatesScheduledCheckHour)
                    nextProgramUpdatesScheduledCheckAttemptionTime = new DateTime(lastProgramUpdatesScheduledCheckAttemptionTime.Year, lastProgramUpdatesScheduledCheckAttemptionTime.Month, lastProgramUpdatesScheduledCheckAttemptionTime.Day, settings.Global.ProgramUpdatesScheduledCheckHour, 0, 0, 0);
                else
                    nextProgramUpdatesScheduledCheckAttemptionTime = (new DateTime(lastProgramUpdatesScheduledCheckAttemptionTime.Year, lastProgramUpdatesScheduledCheckAttemptionTime.Month, lastProgramUpdatesScheduledCheckAttemptionTime.Day, settings.Global.ProgramUpdatesScheduledCheckHour, 0, 0, 0)).AddDays(1);
                for (int i = 0; i < 7; i++)
                {
                    if ((settings.Global.ProgramUpdatesScheduledCheckDays & ((Settings.GlobalSettings.WeekDays)(1 << (int)nextProgramUpdatesScheduledCheckAttemptionTime.DayOfWeek))) == 0)
                        nextProgramUpdatesScheduledCheckAttemptionTime = nextProgramUpdatesScheduledCheckAttemptionTime.AddDays(1);
                    else
                        return (DateTime.Now >= nextProgramUpdatesScheduledCheckAttemptionTime);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public static bool CreateDataFolderIfNotExists()
        {
            try
            {
                if (!Directory.Exists(dataDirectoryPath))
                    Directory.CreateDirectory(dataDirectoryPath);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool UpdateScheduledTask(SettingsHandler settingsHandler)
        {
            if (settingsHandler == null)
                throw new ArgumentNullException();
            try
            {
                if (settingsHandler.SettingsInMemory == null)
                    return false;
                if (settingsHandler.SettingsInMemory.Global.EnableScheduledCheckForProgramUpdates)
                    return WindowsTaskSchedulerUtilities.ScheduleProgramUpdatesCheck(settingsHandler.SettingsInMemory.Global.ProgramUpdatesScheduledCheckDays, settingsHandler.SettingsInMemory.Global.ProgramUpdatesScheduledCheckHour);
                else
                    return WindowsTaskSchedulerUtilities.UnscheduleProgramUpdatesCheck();
            }
            catch
            {
                return false;
            }
        }
        public static bool ResetProgramDatabase(SettingsHandler settingsHandler)
        {
            if (settingsHandler == null)
                throw new ArgumentNullException();
            try
            {
                if (settingsHandler.SettingsInMemory == null)
                    return false;
                string backupOfLastHashOfAllInstalledPrograms = settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms;
                settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = "";
                if (!settingsHandler.SaveSettingsFromMemoryToSettingsFile())
                {
                    settingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = backupOfLastHashOfAllInstalledPrograms;
                    return false;
                }
                if (File.Exists(programDatabaseFilePath))
                    File.Delete(programDatabaseFilePath);
                if (File.Exists(programDatabaseChecksumFilePath))
                    File.Delete(programDatabaseChecksumFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool ResetAll()
        {
            try
            {
                bool thereIsAnError = false;
                if (!WindowsTaskSchedulerUtilities.UnscheduleProgramUpdatesCheck())
                    thereIsAnError = true;
                try
                {
                    if (File.Exists(settingsFilePath))
                        File.Delete(settingsFilePath);
                }
                catch
                {
                    thereIsAnError = true;
                }
                try
                {
                    if (File.Exists(settingsChecksumFilePath))
                        File.Delete(settingsChecksumFilePath);
                }
                catch
                {
                    thereIsAnError = true;
                }
                try
                {
                    if (File.Exists(programDatabaseFilePath))
                        File.Delete(programDatabaseFilePath);
                }
                catch
                {
                    thereIsAnError = true;
                }
                try
                {
                    if (File.Exists(programDatabaseChecksumFilePath))
                        File.Delete(programDatabaseChecksumFilePath);
                }
                catch
                {
                    thereIsAnError = true;
                }
                foreach (string programDatabaseTemporaryFilePath in programDatabaseTemporaryFilesPaths)
                {
                    try
                    {
                        if (File.Exists(programDatabaseTemporaryFilePath))
                            File.Delete(programDatabaseTemporaryFilePath);
                    }
                    catch
                    {
                        thereIsAnError = true;
                    }
                }
                try
                {
                    if (File.Exists(ChromeDriverUtilities.chromeDriverExecutableFilePath))
                        File.Delete(ChromeDriverUtilities.chromeDriverExecutableFilePath);
                    if (Directory.Exists(ChromeDriverUtilities.chromeDriverDirectoryPath))
                        if (Directory.GetFiles(ChromeDriverUtilities.chromeDriverDirectoryPath).Length == 0 && Directory.GetDirectories(ChromeDriverUtilities.chromeDriverDirectoryPath).Length == 0)
                            Directory.Delete(ChromeDriverUtilities.chromeDriverDirectoryPath);
                }
                catch
                {
                    thereIsAnError = true;
                }
                if (!thereIsAnError)
                {
                    try
                    {
                        if (Directory.Exists(dataDirectoryPath))
                            if (Directory.GetFiles(dataDirectoryPath).Length == 0 && Directory.GetDirectories(dataDirectoryPath).Length == 0)
                                Directory.Delete(dataDirectoryPath);
                        string appDirectoryInsideAppDataDirectory = Path.GetDirectoryName(dataDirectoryPath);
                        if (Directory.Exists(appDirectoryInsideAppDataDirectory))
                            if (Directory.GetFiles(appDirectoryInsideAppDataDirectory).Length == 0 && Directory.GetDirectories(appDirectoryInsideAppDataDirectory).Length == 0)
                                Directory.Delete(appDirectoryInsideAppDataDirectory);
                    }
                    catch
                    {
                        thereIsAnError = true;
                    }
                }
                return !thereIsAnError;
            }
            catch
            {
                return false;
            }
        }
        public static void ChangeRenderingScaleOfAllOpenWindowsAndMoveThemIntoScreenBoundaries(double windowsRenderingScale)
        {
            App.WindowsRenderingScale = windowsRenderingScale;
            foreach (Window openWindow in Application.Current.Windows)
            {
                if (openWindow != null && openWindow.IsLoaded)
                {
                    Size baseSizeOfOpenWindow = new Size();
                    Type typeOfOpenWindow = openWindow.GetType();
                    if (typeOfOpenWindow == typeof(MainWindow))
                        baseSizeOfOpenWindow = ((MainWindow)openWindow).BaseSizeOfWindow;
                    else if (typeOfOpenWindow == typeof(AboutWindow))
                        baseSizeOfOpenWindow = ((AboutWindow)openWindow).BaseSizeOfWindow;
                    else if (typeOfOpenWindow == typeof(HelpWindow))
                        baseSizeOfOpenWindow = ((HelpWindow)openWindow).BaseSizeOfWindow;
                    else if (typeOfOpenWindow == typeof(SettingsWindow))
                        baseSizeOfOpenWindow = ((SettingsWindow)openWindow).BaseSizeOfWindow;
                    else if (typeOfOpenWindow == typeof(ProgramAdditionOrEditionWindow))
                        baseSizeOfOpenWindow = ((ProgramAdditionOrEditionWindow)openWindow).BaseSizeOfWindow;
                    else if (typeOfOpenWindow == typeof(ErrorDialogWindow))
                        baseSizeOfOpenWindow = ((ErrorDialogWindow)openWindow).BaseSizeOfWindow;
                    else if (typeOfOpenWindow == typeof(QuestionDialogWindow))
                        baseSizeOfOpenWindow = ((QuestionDialogWindow)openWindow).BaseSizeOfWindow;
                    else if (typeOfOpenWindow == typeof(ProgramUpdatesScheduledCheckWindow))
                        baseSizeOfOpenWindow = ((ProgramUpdatesScheduledCheckWindow)openWindow).BaseSizeOfWindow;
                    else
                        continue;
                    WindowsUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(openWindow, baseSizeOfOpenWindow, windowsRenderingScale);
                    if (typeOfOpenWindow == typeof(SettingsWindow))
                        ((SettingsWindow)openWindow).RefreshAvailableWindowsScalingFactorSelections();
                    else if (typeOfOpenWindow == typeof(ErrorDialogWindow))
                        ((ErrorDialogWindow)openWindow).CalculateWindowDynamicSizeAndResizeWindow();
                    else if (typeOfOpenWindow == typeof(QuestionDialogWindow))
                        ((QuestionDialogWindow)openWindow).CalculateWindowDynamicSizeAndResizeWindow();
                    else if (typeOfOpenWindow == typeof(ProgramAdditionOrEditionWindow))
                        ((ProgramAdditionOrEditionWindow)openWindow).CalculateWindowDynamicSizeAndResizeWindow();
                    else if (typeOfOpenWindow == typeof(ProgramUpdatesScheduledCheckWindow))
                        ((ProgramUpdatesScheduledCheckWindow)openWindow).MoveWindowNearSystemTrayIcons();
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
