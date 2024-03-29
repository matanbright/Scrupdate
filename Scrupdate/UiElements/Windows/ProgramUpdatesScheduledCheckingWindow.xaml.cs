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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Scrupdate.Classes.Objects;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;

namespace Scrupdate.UiElements.Windows
{
    /// <summary>
    /// Interaction logic for ProgramUpdatesScheduledCheckWindow.xaml
    /// </summary>
    public partial class ProgramUpdatesScheduledCheckWindow : Window, INotifyPropertyChanged
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_CLOSING_SCRUPDATE_FORCEFULLY = "Confirmation for Closing Scrupdate Forcefully";
        private const string QUESTION_DIALOG_MESSAGE__ARE_YOU_SURE_YOU_WANT_TO_CLOSE_SCRUPDATE_FORCEFULLY = "Are You Sure You Want to Close Scrupdate Forcefully?\r\n\r\n•  If you close Scrupdate forcefully, ChromeDriver will not have a chance to delete its temporary files.";
        private const string STATUS_MESSAGE__UNABLE_TO_OPEN_THE_PROGRAM_DATABASE = "Unable to Open the Program Database!";
        private const string STATUS_MESSAGE__THE_PROGRAM_DATABASE_IS_CORRUPTED = "The Program Database Is Corrupted!";
        private const string STATUS_MESSAGE__NO_CHROMEDRIVER_IS_INSTALLED = "No ChromeDriver Is Installed!";
        private const string STATUS_MESSAGE__UNABLE_TO_ACCESS_THE_CHROMEDRIVER = "Unable to Access the ChromeDriver!";
        private const string STATUS_MESSAGE__GOOGLE_CHROME_BROWSER_IS_NOT_INSTALLED = "Google Chrome™ Browser Is Not Installed!";
        private const string STATUS_MESSAGE__UNABLE_TO_ACCESS_THE_GOOGLE_CHROME_BROWSER = "Unable to Access the Google Chrome™ Browser!";
        private const string STATUS_MESSAGE__CHROMEDRIVER_ERROR = "ChromeDriver Error!";
        private const string STATUS_MESSAGE__SCANNING_INSTALLED_PROGRAMS = "Scanning Installed Programs...";
        private const string STATUS_MESSAGE__CHECKING_FOR_PROGRAM_UPDATES = "Checking for Program Updates...";
        private const string STATUS_MESSAGE__CANCELLING_AND_CLOSING = "Cancelling and Closing...";
        private const string STATUS_MESSAGE__NO_UPDATES_WERE_FOUND = "No Updates Were Found";
        private const string STATUS_MESSAGE__THERE_IS_AN_UPDATE = "There Is an Update";
        private const string STATUS_MESSAGE__THERE_ARE_N_UPDATES = "There Are {*} Updates";
        private const string ADDITIONAL_STATUS_MESSAGE__THERE_IS_AN_ERROR = "There Is an Error";
        private const string ADDITIONAL_STATUS_MESSAGE__THERE_ARE_N_ERRORS = "There Are {*} Errors";
        private const string TOOLTIP_ERROR_MESSAGE__THE_SETTINGS_FILE_WAS_CORRUPTED = "The Settings File Was Corrupted!\r\n\r\n•  The settings have been reset to their default values.\r\n•  You will need to enable and set again the scheduled check for program updates.\r\n•  The program database and the installed ChromeDriver were not affected.";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum Error
        {
            None,
            CanNotOpenProgramDatabase,
            ProgramDatabaseIsCorrupted
        }
        public enum Operation
        {
            None,
            CancellingOperation,
            UpdatingProgramDatabase,
            CheckingForProgramUpdates
        }
        public enum WindowsTaskbarLocation
        {
            Unknown,
            Left,
            Top,
            Right,
            Bottom
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty WasSettingsFileCorruptedProperty = DependencyProperty.Register(nameof(WasSettingsFileCorrupted), typeof(bool), typeof(ProgramUpdatesScheduledCheckWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty CurrentErrorProperty = DependencyProperty.Register(nameof(CurrentError), typeof(Error), typeof(ProgramUpdatesScheduledCheckWindow), new PropertyMetadata(Error.None));
        public static readonly DependencyProperty IsWindowCollapsedProperty = DependencyProperty.Register(nameof(IsWindowCollapsed), typeof(bool), typeof(ProgramUpdatesScheduledCheckWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty CurrentOperationProperty = DependencyProperty.Register(nameof(CurrentOperation), typeof(Operation), typeof(ProgramUpdatesScheduledCheckWindow), new PropertyMetadata(Operation.None));
        public static readonly DependencyProperty AreThereUpdatesProperty = DependencyProperty.Register(nameof(AreThereUpdates), typeof(bool), typeof(ProgramUpdatesScheduledCheckWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty AreThereErrorsProperty = DependencyProperty.Register(nameof(AreThereErrors), typeof(bool), typeof(ProgramUpdatesScheduledCheckWindow), new PropertyMetadata(false));
        private ProgramDatabase programDatabase;
        private volatile int updatesCount;
        private volatile int errorsCount;
        private CancellableThread programDatabaseUpdatingAndProgramUpdatesCheckCancellableThread;
        private volatile bool closeInQueue;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSizeOfWindow { get; private set; }
        public bool WasSettingsFileCorrupted
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(Dispatcher, () => (bool)GetValue(WasSettingsFileCorruptedProperty));
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    SetValue(WasSettingsFileCorruptedProperty, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WasSettingsFileCorrupted)));
                });
            }
        }
        public Error CurrentError
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(Dispatcher, () => (Error)GetValue(CurrentErrorProperty));
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    SetValue(CurrentErrorProperty, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentError)));
                });
            }
        }
        public bool IsWindowCollapsed
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(Dispatcher, () => (bool)GetValue(IsWindowCollapsedProperty));
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    SetValue(IsWindowCollapsedProperty, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWindowCollapsed)));
                });
            }
        }
        public Operation CurrentOperation
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(Dispatcher, () => (Operation)GetValue(CurrentOperationProperty));
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    SetValue(CurrentOperationProperty, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentOperation)));
                });
            }
        }
        public bool AreThereUpdates
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(Dispatcher, () => (bool)GetValue(AreThereUpdatesProperty));
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    SetValue(AreThereUpdatesProperty, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreThereUpdates)));
                });
            }
        }
        public bool AreThereErrors
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(Dispatcher, () => (bool)GetValue(AreThereErrorsProperty));
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    SetValue(AreThereErrorsProperty, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreThereErrors)));
                });
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ProgramUpdatesScheduledCheckWindow() : this(false) { }
        public ProgramUpdatesScheduledCheckWindow(bool settingsFileWasCorrupted)
        {
            try
            {
                ApplicationUtilities.CreateDataFolderIfNotExists();
                if (!File.Exists(ApplicationUtilities.programDatabaseFilePath))
                {
                    App.SettingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = "";
                    App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
                }
                programDatabase = new ProgramDatabase(ApplicationUtilities.programDatabaseFilePath, ApplicationUtilities.programDatabaseChecksumFilePath);
                bool programDatabaseFileIsCorrupted;
                if (!programDatabase.Open(out programDatabaseFileIsCorrupted))
                    CurrentError = Error.CanNotOpenProgramDatabase;
                if (programDatabaseFileIsCorrupted)
                    CurrentError = Error.ProgramDatabaseIsCorrupted;
            }
            catch
            {
                CurrentError = Error.CanNotOpenProgramDatabase;
            }
            InitializeComponent();
            BaseSizeOfWindow = new Size(Width, Height);
            WindowsUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(this, BaseSizeOfWindow, App.WindowsRenderingScale);
            viewbox_settingsFileCorruptionWarningIndicatorImage.ToolTip = TOOLTIP_ERROR_MESSAGE__THE_SETTINGS_FILE_WAS_CORRUPTED;
            WasSettingsFileCorrupted = settingsFileWasCorrupted;
            App.ScheduleInvokationCommandReceived += OnAppScheduleInvokationCommandReceivedEvent;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnWindowLoadedEvent(object sender, RoutedEventArgs e)
        {
            MoveWindowNearSystemTrayIcons();
            App.SettingsHandler.SettingsInMemory.Cached.LastProgramUpdatesScheduledCheckAttemptionTime = DateTime.Now;
            App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
            if (CurrentError == Error.None)
                StartProgramDatabaseUpdatingAndProgramUpdatesCheckTask();
            else
            {
                switch (CurrentError)
                {
                    case Error.CanNotOpenProgramDatabase:
                        ChangeStatusMessages(STATUS_MESSAGE__UNABLE_TO_OPEN_THE_PROGRAM_DATABASE, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH), "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
                        break;
                    case Error.ProgramDatabaseIsCorrupted:
                        ChangeStatusMessages(STATUS_MESSAGE__THE_PROGRAM_DATABASE_IS_CORRUPTED, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH), "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
                        break;
                }
            }
        }
        private void OnWindowClosingEvent(object sender, CancelEventArgs e)
        {
            if (CurrentOperation == Operation.CancellingOperation || CurrentOperation == Operation.UpdatingProgramDatabase || CurrentOperation == Operation.CheckingForProgramUpdates)
            {
                e.Cancel = true;
                if (CurrentOperation == Operation.UpdatingProgramDatabase || CurrentOperation == Operation.CheckingForProgramUpdates)
                    CancelOperationAndQueueWindowToBeClosedAfterCancelling();
                else if (CurrentOperation == Operation.CancellingOperation && closeInQueue)
                    if (DialogsUtilities.ShowQuestionDialog(QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_CLOSING_SCRUPDATE_FORCEFULLY, QUESTION_DIALOG_MESSAGE__ARE_YOU_SURE_YOU_WANT_TO_CLOSE_SCRUPDATE_FORCEFULLY, this) == true)
                        PrepareWindowForClosing(true);
            }
            else
                PrepareWindowForClosing();
        }
        private void OnAppScheduleInvokationCommandReceivedEvent()
        {
            if (CurrentOperation == Operation.None)
            {
                if (App.SettingsHandler != null && ApplicationUtilities.IsItTimeForProgramUpdatesScheduledCheckAttemption(App.SettingsHandler.SettingsInMemory))
                {
                    ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                    {
                        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                        Close();
                        Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
                        (new ProgramUpdatesScheduledCheckWindow(WasSettingsFileCorrupted)).Show();
                    });
                }
            }
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_closeWindow)
                Close();
            if (senderButton == button_collapseOrExpandWindow)
                CollapseOrExpandWindow();
            else if (senderButton == button_show)
            {
                Settings.CachedSettings.ProgramFilteringOption initialProgramFilteringOption = Settings.CachedSettings.ProgramFilteringOption.Unknown;
                if (updatesCount > 0 && errorsCount == 0)
                    initialProgramFilteringOption = Settings.CachedSettings.ProgramFilteringOption.OnlyUpdates;
                else if (errorsCount > 0 && updatesCount == 0)
                    initialProgramFilteringOption = Settings.CachedSettings.ProgramFilteringOption.OnlyInvalid;
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Close();
                Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
                (new MainWindow(initialProgramFilteringOption, App.SettingsHandler.SettingsInMemory.Global.IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults)).Show();
            }
            else if (senderButton == button_cancelOrDismiss)
            {
                if (CurrentOperation != Operation.CancellingOperation && CurrentOperation != Operation.UpdatingProgramDatabase && CurrentOperation != Operation.CheckingForProgramUpdates)
                    Close();
                else if (CurrentOperation == Operation.UpdatingProgramDatabase || CurrentOperation == Operation.CheckingForProgramUpdates)
                    CancelOperationAndQueueWindowToBeClosedAfterCancelling();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void CollapseOrExpandWindow()
        {
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                Size calculatedWindowSize = new Size();
                if (!IsWindowCollapsed)
                {
                    IsWindowCollapsed = true;
                    calculatedWindowSize = new Size(Math.Floor(470.0D * App.WindowsRenderingScale), Math.Floor(27.0D * App.WindowsRenderingScale));
                }
                else
                {
                    IsWindowCollapsed = false;
                    calculatedWindowSize = new Size(Math.Floor(620.0D * App.WindowsRenderingScale), Math.Floor(120.0D * App.WindowsRenderingScale));
                }
                MinWidth = calculatedWindowSize.Width;
                Width = calculatedWindowSize.Width;
                MaxWidth = calculatedWindowSize.Width;
                MinHeight = calculatedWindowSize.Height;
                Height = calculatedWindowSize.Height;
                MaxHeight = calculatedWindowSize.Height;
                BaseSizeOfWindow = new Size(Width, Height);
                MoveWindowNearSystemTrayIcons();
            });
        }
        private void RefreshStatusMessagesAndButtons()
        {
            if (CurrentError == Error.None)
            {
                updatesCount = 0;
                errorsCount = 0;
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    for (Dictionary<string, Program>.Enumerator i = programDatabase.GetPrograms().GetEnumerator(); i.MoveNext();)
                    {
                        Program program = i.Current.Value;
                        bool? thereIsANewerVersion = null;
                        if (program.InstallationScope != Program.ProgramInstallationScope.None)
                            thereIsANewerVersion = (program.LatestVersion.Equals("") ? false : (program.InstalledVersion.Equals("") ? true : VersionsUtilities.IsVersionNewer(program.LatestVersion, program.InstalledVersion)));
                        if (thereIsANewerVersion == true)
                            if (!program.IsHidden || (program.IsHidden && App.SettingsHandler.SettingsInMemory.Global.IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults))
                                updatesCount++;
                        if (program.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Invalid)
                            if (!program.IsHidden || (program.IsHidden && App.SettingsHandler.SettingsInMemory.Global.IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults))
                                errorsCount++;
                    }
                    string statusMessage = (updatesCount > 0 ? (updatesCount > 1 ? STATUS_MESSAGE__THERE_ARE_N_UPDATES.Replace("{*}", Convert.ToString(updatesCount)) : STATUS_MESSAGE__THERE_IS_AN_UPDATE) : STATUS_MESSAGE__NO_UPDATES_WERE_FOUND);
                    Brush statusMessageColor = (updatesCount > 0 ? (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH) : (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__LIGHT_BLACK_SOLID_COLOR_BRUSH));
                    string additionalStatusMessage = (errorsCount > 0 ? (errorsCount > 1 ? ADDITIONAL_STATUS_MESSAGE__THERE_ARE_N_ERRORS.Replace("{*}", Convert.ToString(errorsCount)) : ADDITIONAL_STATUS_MESSAGE__THERE_IS_AN_ERROR) : "");
                    Brush additionalStatusMessageColor = (errorsCount > 0 ? (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH) : (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
                    ChangeStatusMessages(statusMessage, statusMessageColor, additionalStatusMessage, additionalStatusMessageColor);
                    AreThereUpdates = (updatesCount > 0);
                    AreThereErrors = (errorsCount > 0);
                });
            }
        }
        private void ChangeStatusMessages(string statusMessage, Brush statusMessageColor)
        {
            ChangeStatusMessages(statusMessage, statusMessageColor, "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
        }
        private void ChangeStatusMessages(string statusMessage, Brush statusMessageColor, string additionalStatusMessage, Brush additionalStatusMessageColor)
        {
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                label_statusMessage.Content = statusMessage;
                label_statusMessage.Foreground = statusMessageColor;
                label_additionalStatusMessage.Content = additionalStatusMessage;
                label_additionalStatusMessage.Foreground = additionalStatusMessageColor;
                label_titleStatusMessage.Content = statusMessage;
                label_titleStatusMessage.Foreground = statusMessageColor;
            });
        }
        private void ChangeProgressBarValue(double progressBarValue)
        {
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                if (progressBarValue < 0)
                {
                    progressBar_progress.IsIndeterminate = true;
                    progressBar_progress.ChangeValueSmoothly(0, new Duration(new TimeSpan(0, 0, 0, 0)));
                    progressBar_titleProgress.IsIndeterminate = true;
                    progressBar_titleProgress.ChangeValueSmoothly(0, new Duration(new TimeSpan(0, 0, 0, 0)));
                }
                else
                {
                    progressBar_progress.IsIndeterminate = false;
                    progressBar_progress.ChangeValueSmoothly(progressBarValue, new Duration(new TimeSpan(0, 0, 0, 1)));
                    progressBar_titleProgress.IsIndeterminate = false;
                    progressBar_titleProgress.ChangeValueSmoothly(progressBarValue, new Duration(new TimeSpan(0, 0, 0, 1)));
                }
            });
        }
        private void StartProgramDatabaseUpdatingAndProgramUpdatesCheckTask()
        {
            if (CurrentError == Error.None)
            {
                CurrentOperation = Operation.UpdatingProgramDatabase;
                programDatabaseUpdatingAndProgramUpdatesCheckCancellableThread = new CancellableThread((CancellationToken cancellationToken) =>
                {
                    if (App.SettingsHandler.SettingsInMemory.Global.EnableScanningForInstalledPrograms)
                    {
                        ChangeStatusMessages(STATUS_MESSAGE__SCANNING_INSTALLED_PROGRAMS, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                        ChangeProgressBarValue(-1);
                        if (!closeInQueue && !cancellationToken.IsCancellationRequested)
                            ProgramsScanAndUpdatesCheckUtilities.ScanForInstalledProgramsAndUpdateProgramDatabase(programDatabase, App.SettingsHandler, cancellationToken);
                    }
                    if (!closeInQueue)
                    {
                        CurrentOperation = Operation.CheckingForProgramUpdates;
                        Exception programUpdatesCheckException = null;
                        ChangeStatusMessages(STATUS_MESSAGE__CHECKING_FOR_PROGRAM_UPDATES, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                        ChangeProgressBarValue(-1);
                        if (!closeInQueue && !cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                List<Program> programsToCheck = new List<Program>();
                                for (Dictionary<string, Program>.Enumerator i = programDatabase.GetPrograms().GetEnumerator(); i.MoveNext();)
                                {
                                    Program program = i.Current.Value;
                                    if (program.IsUpdateCheckConfigured)
                                        programsToCheck.Add(program);
                                }
                                ProgramsScanAndUpdatesCheckUtilities.CheckForProgramUpdatesAndUpdateDatabase(programsToCheck, programDatabase, App.SettingsHandler, ChangeProgressBarValue, cancellationToken);
                            }
                            catch (Exception e)
                            {
                                programUpdatesCheckException = e;
                            }
                        }
                        RefreshStatusMessagesAndButtons();
                        CurrentOperation = Operation.None;
                        ChangeProgressBarValue(-1);
                        if (closeInQueue)
                            PrepareWindowForClosing(true);
                        if (programUpdatesCheckException != null)
                        {
                            if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.NoChromeDriverIsInstalledException)))
                                ChangeStatusMessages(STATUS_MESSAGE__NO_CHROMEDRIVER_IS_INSTALLED, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH), "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
                            else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.UnableToAccessChromeDriverExecutableFileException)))
                                ChangeStatusMessages(STATUS_MESSAGE__UNABLE_TO_ACCESS_THE_CHROMEDRIVER, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH), "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
                            else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.GoogleChromeBrowserIsNotInstalledException)))
                                ChangeStatusMessages(STATUS_MESSAGE__GOOGLE_CHROME_BROWSER_IS_NOT_INSTALLED, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH), "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
                            else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.UnableToAccessGoogleChromeBrowserExecutableFileException)))
                                ChangeStatusMessages(STATUS_MESSAGE__UNABLE_TO_ACCESS_THE_GOOGLE_CHROME_BROWSER, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH), "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
                            else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.ChromeDriverIsNotCompatibleOrGoogleChromeBrowserCannotBeOpenedException)))
                                ChangeStatusMessages(STATUS_MESSAGE__CHROMEDRIVER_ERROR, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH), "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
                        }
                        if (!closeInQueue && IsWindowCollapsed)
                            CollapseOrExpandWindow();
                    }
                    ChangeProgressBarValue(-1);
                    if (closeInQueue)
                        PrepareWindowForClosing(true);
                });
                programDatabaseUpdatingAndProgramUpdatesCheckCancellableThread.Start();
            }
        }
        private void CancelOperationAndQueueWindowToBeClosedAfterCancelling()
        {
            if (CurrentOperation != Operation.CancellingOperation)
            {
                CurrentOperation = Operation.CancellingOperation;
                closeInQueue = true;
                programDatabaseUpdatingAndProgramUpdatesCheckCancellableThread?.RequestCancellation();
                ChangeStatusMessages(STATUS_MESSAGE__CANCELLING_AND_CLOSING, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                ChangeProgressBarValue(-1);
            }
        }
        private void PrepareWindowForClosing()
        {
            PrepareWindowForClosing(false);
        }
        private void PrepareWindowForClosing(bool forceCloseApplication)
        {
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                App.ScheduleInvokationCommandReceived -= OnAppScheduleInvokationCommandReceivedEvent;
                programDatabaseUpdatingAndProgramUpdatesCheckCancellableThread?.RequestCancellation();
                programDatabase?.Close();
                App.SettingsHandler?.SaveSettingsFromMemoryToSettingsFile();
                if (forceCloseApplication)
                {
                    App.SettingsHandler?.Dispose();
                    App.SettingsHandler = null;
                    try
                    {
                        foreach (Process chromeDriverProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ChromeDriverUtilities.chromeDriverExecutableFilePath)))
                            chromeDriverProcess.Kill(true);
                    }
                    catch { }
                    Process.GetCurrentProcess().Kill();
                }
            });
        }
        public void MoveWindowNearSystemTrayIcons()
        {
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                Rect screenResolution = new Rect(new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight));
                Rect workingAreaResolution = SystemParameters.WorkArea;
                WindowsTaskbarLocation windowsTaskbarLocation = WindowsTaskbarLocation.Bottom;
                if (screenResolution.Height != workingAreaResolution.Height)
                {
                    if (workingAreaResolution.Y > 0)
                        windowsTaskbarLocation = WindowsTaskbarLocation.Top;
                    else
                        windowsTaskbarLocation = WindowsTaskbarLocation.Bottom;
                }
                else if (screenResolution.Width != workingAreaResolution.Width)
                {
                    if (workingAreaResolution.X > 0)
                        windowsTaskbarLocation = WindowsTaskbarLocation.Left;
                    else
                        windowsTaskbarLocation = WindowsTaskbarLocation.Right;
                }
                switch (windowsTaskbarLocation)
                {
                    case WindowsTaskbarLocation.Left:
                        Left = workingAreaResolution.X;
                        Top = screenResolution.Height - Height;
                        break;
                    case WindowsTaskbarLocation.Top:
                        if (!CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                            Left = screenResolution.Width - Width;
                        else
                            Left = screenResolution.X;
                        Top = workingAreaResolution.Y;
                        break;
                    case WindowsTaskbarLocation.Right:
                        Left = workingAreaResolution.Width - Width;
                        Top = screenResolution.Height - Height;
                        break;
                    case WindowsTaskbarLocation.Bottom:
                        if (!CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                            Left = screenResolution.Width - Width;
                        else
                            Left = screenResolution.X;
                        Top = workingAreaResolution.Height - Height;
                        break;
                }
            });
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
