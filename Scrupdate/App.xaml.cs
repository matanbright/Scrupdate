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
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;
using Scrupdate.Classes.Objects;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Windows;


namespace Scrupdate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Delegates ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public delegate void ScheduleInvokationCommandReceivedEventHandler();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string STARTUP_ARGUMENT__RESET_ALL_AND_CLOSE = "/reset-all-and-close";
        private const string STARTUP_ARGUMENT__RESET_SCHEDULED_TASK_AND_CLOSE = "/reset-scheduled-task-and-close";
        private const string STARTUP_ARGUMENT__UPDATE_SCHEDULED_TASK_AND_CLOSE = "/update-scheduled-task-and-close";
        private const string STARTUP_ARGUMENT__START_IN_SCHEDULED_MODE = "/start-in-scheduled-mode";
        private const string NAMED_PIPE_NAME__SCHEDULE_INVOKATION = "Scrupdate_ScheduleInvokation";
        private const int NAMED_PIPE_CONNECTION_TIMEOUT_IN_MILLISECONDS__SCHEDULE_INVOKATION = 250;
        private const int WINDOW_SHOW_COMMAND_CODE__SW_RESTORE = 9;
        private const string ERROR_DIALOG_TITLE__ERROR = "Error";
        private const string ERROR_DIALOG_MESSAGE__RUNNING_AS_ADMINISTRATOR = "Running as Administrator!\r\n\r\n•  Running Scrupdate with elevated privileges is forbidden. Please run it with normal privileges.";
        private const string ERROR_DIALOG_MESSAGE__UNABLE_TO_ACCESS_THE_SETTINGS_FILE = "Unable to Access the Settings File!\r\n\r\n•  If this error persists, try to restart your computer or reinstall Scrupdate.";
        private const string ERROR_DIALOG_MESSAGE__FAILED_TO_INITIALIZE_SETTINGS = "Failed to Initialize Settings!\r\n\r\n•  If this error persists, try to restart your computer or reinstall Scrupdate.";
        private const string ERROR_DIALOG_MESSAGE__THE_SETTINGS_FILE_WAS_CORRUPTED = "The Settings File Was Corrupted!\r\n\r\n•  The settings have been reset to their default values.\r\n•  If you have enabled scheduled check for program updates before, You will need to enable and set it again.\r\n•  The program database and the installed ChromeDriver were not affected.";
        private const string ERROR_DIALOG_MESSAGE__THE_SETTINGS_FILE_WAS_NOT_COMPATIBLE = "The Settings File Was Not Compatible!\r\n\r\n•  The settings have been reset to their default values.\r\n•  If you have enabled scheduled check for program updates before, You will need to enable and set it again.\r\n•  The program database and the installed ChromeDriver were not affected.";
        private const string ERROR_DIALOG_MESSAGE__THE_CURRENT_WINDOWS_SCALING_FACTOR_IS_INVALID = "The Current Windows Scaling Factor Is Invalid for the Current Display Resolution!\r\n\r\n•  It has been set to the highest valid value.";
        public const string RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH = "transparentSolidColorBrush";
        public const string RESOURCE_KEY__WEAK_TRANSPARENT_RED_SOLID_COLOR_BRUSH = "weakTransparentRedSolidColorBrush";
        public const string RESOURCE_KEY__STRONG_TRANSPARENT_RED_SOLID_COLOR_BRUSH = "strongTransparentRedSolidColorBrush";
        public const string RESOURCE_KEY__WEAK_TRANSPARENT_LIGHT_RED_SOLID_COLOR_BRUSH = "weakTransparentLightRedSolidColorBrush";
        public const string RESOURCE_KEY__STRONG_TRANSPARENT_LIGHT_RED_SOLID_COLOR_BRUSH = "strongTransparentLightRedSolidColorBrush";
        public const string RESOURCE_KEY__WHITE_SOLID_COLOR_BRUSH = "whiteSolidColorBrush";
        public const string RESOURCE_KEY__GHOST_WHITE_SOLID_COLOR_BRUSH = "ghostWhiteSolidColorBrush";
        public const string RESOURCE_KEY__SMOKE_WHITE_SOLID_COLOR_BRUSH = "smokeWhiteSolidColorBrush";
        public const string RESOURCE_KEY__WARM_WHITE_SOLID_COLOR_BRUSH = "warmWhiteSolidColorBrush";
        public const string RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH = "blackSolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_BLACK_SOLID_COLOR_BRUSH = "lightBlackSolidColorBrush";
        public const string RESOURCE_KEY__DARK_GRAY_SOLID_COLOR_BRUSH = "darkGraySolidColorBrush";
        public const string RESOURCE_KEY__GRAY_SOLID_COLOR_BRUSH = "graySolidColorBrush";
        public const string RESOURCE_KEY__BRIGHT_GRAY_SOLID_COLOR_BRUSH = "brightGraySolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_GRAY_SOLID_COLOR_BRUSH_2 = "lightGraySolidColorBrush2";
        public const string RESOURCE_KEY__LIGHT_GRAY_SOLID_COLOR_BRUSH_1 = "lightGraySolidColorBrush1";
        public const string RESOURCE_KEY__PALE_GRAY_SOLID_COLOR_BRUSH = "paleGraySolidColorBrush";
        public const string RESOURCE_KEY__DARK_SILVERISH_GRAY_SOLID_COLOR_BRUSH = "darkSilverishGraySolidColorBrush";
        public const string RESOURCE_KEY__SILVERISH_GRAY_SOLID_COLOR_BRUSH = "silverishGraySolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_SILVERISH_GRAY_SOLID_COLOR_BRUSH = "lightSilverishGraySolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_BLUISH_GRAY_SOLID_COLOR_BRUSH = "lightBluishGraySolidColorBrush";
        public const string RESOURCE_KEY__DARK_RED_SOLID_COLOR_BRUSH = "darkRedSolidColorBrush";
        public const string RESOURCE_KEY__RED_SOLID_COLOR_BRUSH = "redSolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_GRAYISH_RED_SOLID_COLOR_BRUSH = "lightGrayishRedSolidColorBrush";
        public const string RESOURCE_KEY__BORDO_SOLID_COLOR_BRUSH = "bordoSolidColorBrush";
        public const string RESOURCE_KEY__BRIGHT_BORDO_SOLID_COLOR_BRUSH = "brightBordoSolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_BORDO_SOLID_COLOR_BRUSH = "lightBordoSolidColorBrush";
        public const string RESOURCE_KEY__PALE_BORDO_SOLID_COLOR_BRUSH = "paleBordoSolidColorBrush";
        public const string RESOURCE_KEY__PALE_ORANGE_SOLID_COLOR_BRUSH = "paleOrangeSolidColorBrush";
        public const string RESOURCE_KEY__BROWNISH_ORANGE_SOLID_COLOR_BRUSH = "brownishOrangeSolidColorBrush";
        public const string RESOURCE_KEY__BRIGHT_BROWNISH_ORANGE_SOLID_COLOR_BRUSH = "brightBrownishOrangeSolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_BROWNISH_ORANGE_SOLID_COLOR_BRUSH = "lightBrownishOrangeSolidColorBrush";
        public const string RESOURCE_KEY__PALE_BROWNISH_ORANGE_SOLID_COLOR_BRUSH = "paleBrownishOrangeSolidColorBrush";
        public const string RESOURCE_KEY__DARK_YELLOW_SOLID_COLOR_BRUSH = "darkYellowSolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_GRAYISH_YELLOW_SOLID_COLOR_BRUSH = "lightGrayishYellowSolidColorBrush";
        public const string RESOURCE_KEY__ORANGISH_YELLOW_SOLID_COLOR_BRUSH = "orangishYellowSolidColorBrush";
        public const string RESOURCE_KEY__DARK_GREEN_SOLID_COLOR_BRUSH = "darkGreenSolidColorBrush";
        public const string RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH = "greenSolidColorBrush";
        public const string RESOURCE_KEY__GRAYISH_GREEN_SOLID_COLOR_BRUSH_2 = "grayishGreenSolidColorBrush2";
        public const string RESOURCE_KEY__GRAYISH_GREEN_SOLID_COLOR_BRUSH_1 = "grayishGreenSolidColorBrush1";
        public const string RESOURCE_KEY__BRIGHT_GRAYISH_GREEN_SOLID_COLOR_BRUSH = "brightGrayishGreenSolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_GRAYISH_GREEN_SOLID_COLOR_BRUSH_2 = "lightGrayishGreenSolidColorBrush2";
        public const string RESOURCE_KEY__LIGHT_GRAYISH_GREEN_SOLID_COLOR_BRUSH_1 = "lightGrayishGreenSolidColorBrush1";
        public const string RESOURCE_KEY__PALE_GRAYISH_GREEN_SOLID_COLOR_BRUSH_2 = "paleGrayishGreenSolidColorBrush2";
        public const string RESOURCE_KEY__PALE_GRAYISH_GREEN_SOLID_COLOR_BRUSH_1 = "paleGrayishGreenSolidColorBrush1";
        public const string RESOURCE_KEY__DARK_SKY_BLUE_SOLID_COLOR_BRUSH = "darkSkyBlueSolidColorBrush";
        public const string RESOURCE_KEY__GRAYISH_BLUE_SOLID_COLOR_BRUSH = "grayishBlueSolidColorBrush";
        public const string RESOURCE_KEY__BRIGHT_GRAYISH_BLUE_SOLID_COLOR_BRUSH = "brightGrayishBlueSolidColorBrush";
        public const string RESOURCE_KEY__LIGHT_GRAYISH_BLUE_SOLID_COLOR_BRUSH = "lightGrayishBlueSolidColorBrush";
        public const string RESOURCE_KEY__PALE_GRAYISH_BLUE_SOLID_COLOR_BRUSH = "paleGrayishBlueSolidColorBrush";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static NamedPipeServerStream scheduleInvokationCommandListeningServer;
        private static Thread scheduleInvokationCommandListeningThread;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static SettingsHandler SettingsHandler { get; set; }
        public static double WindowsRenderingScale { get; set; }
        public static event ScheduleInvokationCommandReceivedEventHandler ScheduleInvokationCommandReceived;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnApplicationStartupEvent(object sender, StartupEventArgs e)
        {
            SettingsHandler = null;
            WindowsRenderingScale = 1.0D;
            bool resetAllAndClose = false;
            bool resetScheduledTaskAndClose = false;
            bool updateScheduledTaskAndClose = false;
            bool startInScheduledMode = false;
            foreach (string arg in e.Args)
            {
                if (arg.ToLower().Equals(STARTUP_ARGUMENT__RESET_ALL_AND_CLOSE))
                    resetAllAndClose = true;
                if (arg.ToLower().Equals(STARTUP_ARGUMENT__RESET_SCHEDULED_TASK_AND_CLOSE))
                    resetScheduledTaskAndClose = true;
                else if (arg.ToLower().Equals(STARTUP_ARGUMENT__UPDATE_SCHEDULED_TASK_AND_CLOSE))
                    updateScheduledTaskAndClose = true;
                else if (arg.ToLower().Equals(STARTUP_ARGUMENT__START_IN_SCHEDULED_MODE))
                    startInScheduledMode = true;
            }
            if (resetAllAndClose)
            {
                ApplicationUtilities.ResetAll();
                Shutdown();
            }
            else if (resetScheduledTaskAndClose)
            {
                WindowsTaskSchedulerUtilities.UnscheduleProgramUpdatesCheck();
                Shutdown();
            }
            else if (updateScheduledTaskAndClose)
            {
                if (File.Exists(ApplicationUtilities.settingsFilePath))
                {
                    try
                    {
                        using (SettingsHandler settingsHandler = new SettingsHandler(ApplicationUtilities.settingsFilePath, ApplicationUtilities.settingsChecksumFilePath))
                        {
                            if (settingsHandler.LoadSettingsToMemoryFromSettingsFile(out _))
                                ApplicationUtilities.UpdateScheduledTask(settingsHandler);
                        }
                    }
                    catch { }
                }
                Shutdown();
            }
            else
            {
                Process[] processesOfAlreadyRunningInstancesOfTheProgram = Array.FindAll(Process.GetProcessesByName(AppDomain.CurrentDomain.FriendlyName), (Process processOfAlreadyRunningInstanceOfTheProgram) => (processOfAlreadyRunningInstanceOfTheProgram.SessionId == Process.GetCurrentProcess().SessionId));
                if (processesOfAlreadyRunningInstancesOfTheProgram.Length > 1)
                {
                    if (startInScheduledMode)
                        SendScheduleInvokationCommand();
                    else
                    {
                        foreach (Process processOfAlreadyRunningInstanceOfTheProgram in processesOfAlreadyRunningInstancesOfTheProgram)
                        {
                            if (processOfAlreadyRunningInstanceOfTheProgram.Id != Environment.ProcessId)
                            {
                                ShowWindow(processOfAlreadyRunningInstanceOfTheProgram.MainWindowHandle, WINDOW_SHOW_COMMAND_CODE__SW_RESTORE);
                                SetForegroundWindow(processOfAlreadyRunningInstanceOfTheProgram.MainWindowHandle);
                            }
                        }
                    }
                    Shutdown();
                    return;
                }
                else if ((new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__RUNNING_AS_ADMINISTRATOR, null);
                    Shutdown();
                    return;
                }
                try
                {
                    SettingsHandler = new SettingsHandler(ApplicationUtilities.settingsFilePath, ApplicationUtilities.settingsChecksumFilePath);
                }
                catch
                {
                    if (processesOfAlreadyRunningInstancesOfTheProgram.Length <= 1)
                        DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__UNABLE_TO_ACCESS_THE_SETTINGS_FILE, null);
                    Shutdown();
                    return;
                }
                ConfigError settingsFileError;
                if (!SettingsHandler.LoadSettingsToMemoryFromSettingsFile(out settingsFileError))
                {
                    SettingsHandler.SettingsInMemory = new Settings();
                    if (!SettingsHandler.SaveSettingsFromMemoryToSettingsFile())
                    {
                        if (processesOfAlreadyRunningInstancesOfTheProgram.Length <= 1)
                            DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__FAILED_TO_INITIALIZE_SETTINGS, null);
                        SettingsHandler.Dispose();
                        Shutdown();
                        return;
                    }
                }
                bool windowsScalingFactorHasBeenChangedDueToBeingInvalid;
                WindowsRenderingScale = WindowsUtilities.GetWindowsRenderingScale(SettingsHandler, true, out windowsScalingFactorHasBeenChangedDueToBeingInvalid);
                if (!startInScheduledMode)
                {
                    ApplicationUtilities.UpdateScheduledTask(SettingsHandler);
                    SystemEvents.DisplaySettingsChanged += OnSystemDisplaySettingsChangedEvent;
                    switch (settingsFileError)
                    {
                        case ConfigError.Corrupted:
                            DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__THE_SETTINGS_FILE_WAS_CORRUPTED, null);
                            break;
                        case ConfigError.NotCompatible:
                            DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__THE_SETTINGS_FILE_WAS_NOT_COMPATIBLE, null);
                            break;
                    }
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    if (windowsScalingFactorHasBeenChangedDueToBeingInvalid)
                        DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__THE_CURRENT_WINDOWS_SCALING_FACTOR_IS_INVALID, null);
                }
                else if (ApplicationUtilities.IsItTimeForProgramUpdatesScheduledCheckAttemption(SettingsHandler.SettingsInMemory))
                {
                    ApplicationUtilities.UpdateScheduledTask(SettingsHandler);
                    StartListeningToScheduleInvokationCommand();
                    SystemEvents.DisplaySettingsChanged += OnSystemDisplaySettingsChangedEvent;
                    ProgramUpdatesScheduledCheckWindow programUpdatesScheduledCheckWindow = new ProgramUpdatesScheduledCheckWindow(settingsFileError);
                    programUpdatesScheduledCheckWindow.Show();
                }
                else
                {
                    SettingsHandler.Dispose();
                    Shutdown();
                }
            }
        }
        private void OnApplicationExitEvent(object sender, ExitEventArgs e)
        {
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () => SystemEvents.DisplaySettingsChanged -= OnSystemDisplaySettingsChangedEvent);
            StopListeningToScheduleInvokationCommand();
            SettingsHandler?.Dispose();
            SettingsHandler = null;
        }
        private void OnSystemDisplaySettingsChangedEvent(object sender, EventArgs e)
        {
            bool windowsScalingFactorHasBeenChangedDueToBeingInvalid;
            double newWindowsRenderingScale = WindowsUtilities.GetWindowsRenderingScale(SettingsHandler, true, out windowsScalingFactorHasBeenChangedDueToBeingInvalid);
            ApplicationUtilities.ChangeRenderingScaleOfAllOpenWindowsAndMoveThemIntoScreenBoundaries(newWindowsRenderingScale);
            if (windowsScalingFactorHasBeenChangedDueToBeingInvalid)
                DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__THE_CURRENT_WINDOWS_SCALING_FACTOR_IS_INVALID, null);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private static void StartListeningToScheduleInvokationCommand()
        {
            scheduleInvokationCommandListeningThread = new Thread(() =>
            {
                try
                {
                    scheduleInvokationCommandListeningServer = new NamedPipeServerStream(NAMED_PIPE_NAME__SCHEDULE_INVOKATION, PipeDirection.In);
                    scheduleInvokationCommandListeningServer.WaitForConnection();
                    byte[] buffer = new byte[1024];
                    int bufferLength = scheduleInvokationCommandListeningServer.Read(buffer);
                    string scheduleInvokationCommand = Encoding.UTF8.GetString(buffer, 0, bufferLength);
                    if (scheduleInvokationCommand.Equals(STARTUP_ARGUMENT__START_IN_SCHEDULED_MODE))
                        ScheduleInvokationCommandReceived.Invoke();
                }
                catch { }
                StopListeningToScheduleInvokationCommand();
                StartListeningToScheduleInvokationCommand();
            });
            scheduleInvokationCommandListeningThread.IsBackground = true;
            scheduleInvokationCommandListeningThread.Start();
        }
        private static void StopListeningToScheduleInvokationCommand()
        {
            try
            {
                scheduleInvokationCommandListeningServer?.Disconnect();
            }
            catch { }
            try
            {
                scheduleInvokationCommandListeningServer?.Dispose();
                scheduleInvokationCommandListeningServer = null;
            }
            catch { }
        }
        private static void SendScheduleInvokationCommand()
        {
            try
            {
                using (NamedPipeClientStream scheduleInvokationNamedPipeClientStream = new NamedPipeClientStream(".", NAMED_PIPE_NAME__SCHEDULE_INVOKATION, PipeDirection.Out))
                {
                    scheduleInvokationNamedPipeClientStream.Connect(NAMED_PIPE_CONNECTION_TIMEOUT_IN_MILLISECONDS__SCHEDULE_INVOKATION);
                    scheduleInvokationNamedPipeClientStream.Write(Encoding.UTF8.GetBytes(STARTUP_ARGUMENT__START_IN_SCHEDULED_MODE));
                }
            }
            catch { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
