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
using System.Runtime.InteropServices;


namespace Scrupdate.Classes.Utilities
{
    public static class ProcessesUtilities
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class FileDoesNotExistException : Exception
        {
            private const string EXCEPTION_MESSAGE = "File doesn't exist!";
            public FileDoesNotExistException() : base(EXCEPTION_MESSAGE) { }
        }
        public class FileIsNotAccessibleException : Exception
        {
            private const string EXCEPTION_MESSAGE = "File is not accessible!";
            public FileIsNotAccessibleException() : base(EXCEPTION_MESSAGE) { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Structs /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private struct SECURITY_ATTRIBUTES
        {
            public uint nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }
        private struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved2;
            public byte lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private enum SaferScope : uint
        {
            Unknown = 0U,
            Machine = 1U,
            User = 2U
        }
        private enum SaferLevel : uint
        {
            Disallowed = 0x0U,
            Untrusted = 0x1000U,
            Constrained = 0x10000U,
            NormalUser = 0x20000U,
            FullyTrusted = 0x40000U
        }
        private enum SaferOpenFlags : uint
        {
            None = 0U,
            Open = 1U
        }
        private enum SaferComputeTokenFlags : uint
        {
            None = 0x0U,
            NullIfEqual = 0x1U,
            CompareOnly = 0x2U,
            MakeInert = 0x4U,
            WantFlags = 0x8U
        }
        private enum ProcessCreationFlags : uint
        {
            None = 0x0U,
            DebugProcess = 0x1U,
            DebugOnlyThisProcess = 0x2U,
            CreateSuspended = 0x4U,
            DetachedProcess = 0x8U,
            CreateNewConsole = 0x10U,
            CreateNewProcessGroup = 0x200U,
            CreateUnicodeEnvironment = 0x400U,
            CreateSeparateWowVdm = 0x800U,
            CreateSharedWowVdm = 0x1000U,
            InheritParentAffinity = 0x10000U,
            CreateProtectedProcess = 0x40000U,
            ExtendedStartupinfoPresent = 0x80000U,
            CreateSecureProcess = 0x400000U,
            CreateBreakawayFromJob = 0x1000000U,
            CreatePreserveCodeAuthzLevel = 0x2000000U,
            CreateDefaultErrorMode = 0x4000000U,
            CreateNoWindow = 0x8000000U
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport("advapi32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SaferCreateLevel(SaferScope dwScopeId,
                                                    SaferLevel dwLevelId,
                                                    SaferOpenFlags OpenFlags,
                                                    out IntPtr pLevelHandle,
                                                    IntPtr lpReserved);
        [DllImport("advapi32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SaferComputeTokenFromLevel(IntPtr LevelHandle,
                                                              IntPtr InAccessToken,
                                                              out IntPtr OutAccessToken,
                                                              SaferComputeTokenFlags dwFlags,
                                                              IntPtr lpReserved);
        [DllImport("advapi32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SaferCloseLevel(IntPtr hLevelHandle);
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcessAsUser(IntPtr hToken,
                                                       string lpApplicationName,
                                                       string lpCommandLine,
                                                       ref SECURITY_ATTRIBUTES lpProcessAttributes,
                                                       ref SECURITY_ATTRIBUTES lpThreadAttributes,
                                                       bool bInheritHandles,
                                                       ProcessCreationFlags dwCreationFlags,
                                                       IntPtr lpEnvironment,
                                                       string lpCurrentDirectory,
                                                       ref STARTUPINFO lpStartupInfo,
                                                       out PROCESS_INFORMATION lpProcessInformation);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);
        public static int RunFile(string filePath)
        {
            return RunFile(
                filePath,
                null,
                false,
                false,
                false,
                -1,
                false,
                false,
                out _
            );
        }
        public static int RunFile(string filePath, string arguments)
        {
            return RunFile(
                filePath,
                arguments,
                false,
                false,
                false,
                -1,
                false,
                false,
                out _
            );
        }
        public static int RunFile(string filePath,
                                  string arguments,
                                  bool useShellExecute,
                                  bool createNoWindow,
                                  bool waitForExit,
                                  int waitingTimeout,
                                  bool killProcessTreeOnTimeout,
                                  bool redirectStandardOutputIfWaitingForExit,
                                  out string standardOutput)
        {
            standardOutput = null;
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileDoesNotExistException();
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.CreateNoWindow = createNoWindow;
                    process.StartInfo.UseShellExecute = useShellExecute;
                    process.StartInfo.RedirectStandardOutput = (waitForExit && redirectStandardOutputIfWaitingForExit);
                    process.StartInfo.FileName = filePath;
                    if (arguments != null && !arguments.Equals(""))
                        process.StartInfo.Arguments = arguments;
                    process.Start();
                    if (waitForExit)
                    {
                        if (waitingTimeout >= 0)
                        {
                            process.WaitForExit(waitingTimeout);
                            if (!process.HasExited && killProcessTreeOnTimeout)
                                process.Kill(true);
                        }
                        else
                            process.WaitForExit();
                        if (redirectStandardOutputIfWaitingForExit)
                            using (StreamReader processStandardOutputStreamReader = process.StandardOutput)
                                standardOutput = processStandardOutputStreamReader.ReadToEnd();
                        return process.ExitCode;
                    }
                }
                return 0;
            }
            catch
            {
                throw new FileIsNotAccessibleException();
            }
        }
        public static int RunFileWithoutElevatedPrivileges(string filePath)
        {
            return RunFileWithoutElevatedPrivileges(
                filePath,
                null,
                false,
                false,
                -1,
                false
            );
        }
        public static int RunFileWithoutElevatedPrivileges(string filePath, string arguments)
        {
            return RunFileWithoutElevatedPrivileges(
                filePath,
                arguments,
                false,
                false,
                -1,
                false
            );
        }
        public static int RunFileWithoutElevatedPrivileges(string filePath,
                                                           string arguments,
                                                           bool createNoWindow,
                                                           bool waitForExit,
                                                           int waitingTimeout,
                                                           bool killProcessTreeOnTimeout)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileDoesNotExistException();
            try
            {
                IntPtr hSaferLevel = IntPtr.Zero, hToken = IntPtr.Zero;
                SECURITY_ATTRIBUTES lpProcessAttributes = default(SECURITY_ATTRIBUTES);
                SECURITY_ATTRIBUTES lpThreadAttributes = default(SECURITY_ATTRIBUTES);
                STARTUPINFO lpStartupInfo = default(STARTUPINFO);
                lpStartupInfo.cb = (uint)Marshal.SizeOf(lpStartupInfo);
                PROCESS_INFORMATION lpProcessInformation = default(PROCESS_INFORMATION);
                try
                {
                    if (!SaferCreateLevel(
                            SaferScope.User,
                            SaferLevel.NormalUser,
                            SaferOpenFlags.Open,
                            out hSaferLevel,
                            IntPtr.Zero
                        ))
                    {
                        return -1;
                    }
                    if (!SaferComputeTokenFromLevel(
                            hSaferLevel,
                            IntPtr.Zero,
                            out hToken,
                            SaferComputeTokenFlags.None,
                            IntPtr.Zero
                        ))
                    {
                        return -1;
                    }
                    if (!CreateProcessAsUser(
                            hToken,
                            filePath,
                            ((arguments == null || arguments.Equals("")) ?
                                null :
                                (new StringBuilder())
                                    .Append(' ')
                                    .Append(arguments)
                                    .ToString()),
                            ref lpProcessAttributes,
                            ref lpThreadAttributes,
                            true,
                            (createNoWindow ? ProcessCreationFlags.CreateNoWindow : ProcessCreationFlags.None),
                            IntPtr.Zero,
                            null,
                            ref lpStartupInfo,
                            out lpProcessInformation
                        ))
                    {
                        return -1;
                    }
                    if (waitForExit)
                    {
                        using (Process process = Process.GetProcessById((int)lpProcessInformation.dwProcessId))
                        {
                            if (waitingTimeout >= 0)
                            {
                                process.WaitForExit(waitingTimeout);
                                if (!process.HasExited && killProcessTreeOnTimeout)
                                    process.Kill(true);
                            }
                            else
                                process.WaitForExit();
                        }
                        uint processExitCode;
                        GetExitCodeProcess(lpProcessInformation.hProcess, out processExitCode);
                        return (int)processExitCode;
                    }
                    return 0;
                }
                finally
                {
                    CloseHandle(lpProcessInformation.hThread);
                    CloseHandle(lpProcessInformation.hProcess);
                    SaferCloseLevel(hSaferLevel);
                }
            }
            catch
            {
                throw new FileIsNotAccessibleException();
            }
        }
        public static bool OpenUrlInDefaultWebBrowser(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = url;
                    process.Start();
                }
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
