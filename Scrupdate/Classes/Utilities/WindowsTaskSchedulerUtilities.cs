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
using System.Diagnostics;
using System.Security.Principal;
using Scrupdate.Classes.Objects;


namespace Scrupdate.Classes.Utilities
{
    public static class WindowsTaskSchedulerUtilities
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class NoScheduleDaysWereSpecifiedException : Exception
        {
            private const string EXCEPTION_MESSAGE = "No schedule days were specified!";
            public NoScheduleDaysWereSpecifiedException() : base(EXCEPTION_MESSAGE) { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string SCHEDULED_TASK_NAME__PROGRAM_UPDATES_SCHEDULED_CHECK = "Scrupdate - Program Updates Scheduled Check Task (*)";
        private const int TASK_SCHEDULER_QUERY_TIMEOUT_IN_MILLISECONDS = 1000;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static readonly string scheduledTaskNameForCurrentUser = SCHEDULED_TASK_NAME__PROGRAM_UPDATES_SCHEDULED_CHECK.Replace("*", WindowsIdentity.GetCurrent().User.Value);
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool ScheduleProgramUpdatesCheck(Settings.GeneralSettings.WeekDays scheduleDays, int scheduleHour)
        {
            if (!((int)scheduleDays >= 0b00000001 && (int)scheduleDays <= 0b01111111))
                throw new ArgumentOutOfRangeException(nameof(scheduleDays));
            if (!(scheduleHour >= 0 && scheduleHour < 24))
                throw new ArgumentOutOfRangeException(nameof(scheduleHour));
            if (scheduleDays == Settings.GeneralSettings.WeekDays.None)
                throw new NoScheduleDaysWereSpecifiedException();
            string temporaryFilePath = Path.GetTempFileName();
            try
            {
                using (FileStream fileStreamOfTemporaryFile = new FileStream(temporaryFilePath, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    DateTime currentTime = DateTime.Now;
                    string scheduledTaskXmlRepresention = $@"<?xml version=""1.0"" encoding=""UTF-16""?>
                            <Task version=""1.2"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
                                <Triggers>
                                    <LogonTrigger>
                                        <Enabled>true</Enabled>
                                        <UserId>{WindowsIdentity.GetCurrent().Name}</UserId>
                                        <Delay>PT15S</Delay>
                                    </LogonTrigger>
                                    <CalendarTrigger>
                                        <StartBoundary>{(new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, scheduleHour, 0, 0, 0)):s}</StartBoundary>
                                        <Enabled>true</Enabled>
                                        <ScheduleByWeek>
                                            <DaysOfWeek>
                                                {(((scheduleDays & Settings.GeneralSettings.WeekDays.Sunday) != 0) ? "<Sunday />" : "")}
                                                {(((scheduleDays & Settings.GeneralSettings.WeekDays.Monday) != 0) ? "<Monday />" : "")}
                                                {(((scheduleDays & Settings.GeneralSettings.WeekDays.Tuesday) != 0) ? "<Tuesday />" : "")}
                                                {(((scheduleDays & Settings.GeneralSettings.WeekDays.Wednesday) != 0) ? "<Wednesday />" : "")}
                                                {(((scheduleDays & Settings.GeneralSettings.WeekDays.Thursday) != 0) ? "<Thursday />" : "")}
                                                {(((scheduleDays & Settings.GeneralSettings.WeekDays.Friday) != 0) ? "<Friday />" : "")}
                                                {(((scheduleDays & Settings.GeneralSettings.WeekDays.Saturday) != 0) ? "<Saturday />" : "")}
                                            </DaysOfWeek>
                                            <WeeksInterval>1</WeeksInterval>
                                        </ScheduleByWeek>
                                    </CalendarTrigger>
                                </Triggers>
                                <Principals>
                                    <Principal id=""Author"">
                                        <UserId>{WindowsIdentity.GetCurrent().User.Value}</UserId>
                                        <LogonType>InteractiveToken</LogonType>
                                        <RunLevel>LeastPrivilege</RunLevel>
                                    </Principal>
                                </Principals>
                                <Settings>
                                    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
                                    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                                    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                                    <AllowHardTerminate>false</AllowHardTerminate>
                                    <StartWhenAvailable>true</StartWhenAvailable>
                                    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
                                    <AllowStartOnDemand>true</AllowStartOnDemand>
                                    <Enabled>true</Enabled>
                                    <Hidden>false</Hidden>
                                    <RunOnlyIfIdle>false</RunOnlyIfIdle>
                                    <WakeToRun>false</WakeToRun>
                                    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
                                    <Priority>7</Priority>
                                </Settings>
                                <Actions Context=""Author"">
                                    <Exec>
                                        <Command>{Process.GetCurrentProcess().MainModule.FileName}</Command>
                                        <Arguments>/start-in-scheduled-mode</Arguments>
                                    </Exec>
                                </Actions>
                            </Task>
                        ";
                    fileStreamOfTemporaryFile.Write(Encoding.UTF8.GetBytes(scheduledTaskXmlRepresention));
                    fileStreamOfTemporaryFile.Flush();
                }
                StringBuilder tempStringBuilder = new StringBuilder();
                tempStringBuilder.Clear().Append(Environment.GetFolderPath(Environment.SpecialFolder.System)).Append('\\').Append("schtasks.exe");
                string fileName = tempStringBuilder.ToString();
                tempStringBuilder.Clear().Append("/Create /TN \"").Append(scheduledTaskNameForCurrentUser).Append("\" /XML \"").Append(temporaryFilePath).Append("\" /F");
                if (ProcessesUtilities.RunFileWithoutElevatedPrivileges(fileName, tempStringBuilder.ToString(), true, true, TASK_SCHEDULER_QUERY_TIMEOUT_IN_MILLISECONDS, true) != 0)
                {
                    File.Delete(temporaryFilePath);
                    return false;
                }
                File.Delete(temporaryFilePath);
                return true;
            }
            catch
            {
                if (File.Exists(temporaryFilePath))
                    File.Delete(temporaryFilePath);
                return false;
            }
        }
        public static bool UnscheduleProgramUpdatesCheck()
        {
            try
            {
                StringBuilder tempStringBuilder = new StringBuilder();
                tempStringBuilder.Clear().Append(Environment.GetFolderPath(Environment.SpecialFolder.System)).Append('\\').Append("schtasks.exe");
                string fileName = tempStringBuilder.ToString();
                tempStringBuilder.Clear().Append("/Create /TN \"").Append(scheduledTaskNameForCurrentUser).Append("\" /SC Once /SD 01/01/1970 /ST 00:00 /TR \"NULL\" /F");
                if (ProcessesUtilities.RunFileWithoutElevatedPrivileges(fileName, tempStringBuilder.ToString(), true, true, TASK_SCHEDULER_QUERY_TIMEOUT_IN_MILLISECONDS, true) != 0)
                    return false;
                tempStringBuilder.Clear().Append("/Delete /TN \"").Append(scheduledTaskNameForCurrentUser).Append("\" /F");
                ProcessesUtilities.RunFileWithoutElevatedPrivileges(fileName, tempStringBuilder.ToString(), true, true, TASK_SCHEDULER_QUERY_TIMEOUT_IN_MILLISECONDS, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
