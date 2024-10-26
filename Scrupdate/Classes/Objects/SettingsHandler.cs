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
using Scrupdate.Classes.Utilities;


namespace Scrupdate.Classes.Objects
{
    public class SettingsHandler : IDisposable
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class UnableToOpenOrCreateSettingsFileException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Unable to open or create settings file!";
            public UnableToOpenOrCreateSettingsFileException() : base(EXCEPTION_MESSAGE) { }
        }
        public class NoSettingsInMemoryException : Exception
        {
            private const string EXCEPTION_MESSAGE = "No settings in memory!";
            public NoSettingsInMemoryException() : base(EXCEPTION_MESSAGE) { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private volatile bool disposed;
        private FileStream fileStreamOfSettingsFile;
        private FileStream fileStreamOfSettingsChecksumFile;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Settings SettingsInMemory { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public SettingsHandler(string settingsFilePath, string settingsChecksumFilePath)
        {
            disposed = false;
            try
            {
                string settingsFileDirectoryPath = Path.GetDirectoryName(settingsFilePath);
                if (!Directory.Exists(settingsFileDirectoryPath))
                    Directory.CreateDirectory(settingsFileDirectoryPath);
                string settingsChecksumFileDirectoryPath = Path.GetDirectoryName(settingsChecksumFilePath);
                if (!Directory.Exists(settingsChecksumFileDirectoryPath))
                    Directory.CreateDirectory(settingsChecksumFileDirectoryPath);
                fileStreamOfSettingsChecksumFile = new FileStream(
                    settingsChecksumFilePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.Read
                );
                File.SetAttributes(
                    settingsChecksumFilePath,
                    File.GetAttributes(settingsChecksumFilePath) | FileAttributes.Hidden
                );
                bool createNewSettingsFileWithDefaultValues = !File.Exists(settingsFilePath);
                fileStreamOfSettingsFile = new FileStream(
                    settingsFilePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.Read
                );
                if (createNewSettingsFileWithDefaultValues)
                {
                    SettingsInMemory = new Settings();
                    SaveSettingsFromMemoryToSettingsFile();
                    SettingsInMemory = null;
                }
            }
            catch
            {
                if (fileStreamOfSettingsFile != null)
                {
                    long settingsFileSize = fileStreamOfSettingsFile.Length;
                    fileStreamOfSettingsFile.Dispose();
                    fileStreamOfSettingsFile = null;
                    if (settingsFileSize == 0)
                    {
                        try
                        {
                            File.Delete(settingsFilePath);
                        }
                        catch { }
                    }
                }
                if (fileStreamOfSettingsChecksumFile != null)
                {
                    long settingsChecksumFileSize = fileStreamOfSettingsChecksumFile.Length;
                    fileStreamOfSettingsChecksumFile.Dispose();
                    fileStreamOfSettingsChecksumFile = null;
                    if (settingsChecksumFileSize == 0)
                    {
                        try
                        {
                            File.Delete(settingsChecksumFilePath);
                        }
                        catch { }
                    }
                }
                throw new UnableToOpenOrCreateSettingsFileException();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Destructors /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ~SettingsHandler() => Dispose();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool SaveSettingsFromMemoryToSettingsFile()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (SettingsInMemory == null)
                throw new NoSettingsInMemoryException();
            try
            {
                DateTime backupOfLastProgramUpdatesScheduledCheckAttemptionTime =
                    SettingsInMemory.Cached.LastProgramUpdatesScheduledCheckAttemptionTime;
                if (!SettingsInMemory.General.EnableScheduledCheckForProgramUpdates)
                    SettingsInMemory.Cached.LastProgramUpdatesScheduledCheckAttemptionTime = new DateTime();
                else if (SettingsInMemory.Cached.LastProgramUpdatesScheduledCheckAttemptionTime.Equals(new DateTime()))
                    SettingsInMemory.Cached.LastProgramUpdatesScheduledCheckAttemptionTime = DateTime.Now;
                bool thereIsAnError = false;
                if (SettingsInMemory.General.EnableScheduledCheckForProgramUpdates)
                {
                    thereIsAnError = !WindowsTaskSchedulerUtilities.ScheduleProgramUpdatesCheck(
                        SettingsInMemory.General.ProgramUpdatesScheduledCheckDays,
                        SettingsInMemory.General.ProgramUpdatesScheduledCheckHour
                    );
                }
                else
                    thereIsAnError = !WindowsTaskSchedulerUtilities.UnscheduleProgramUpdatesCheck();
                if (thereIsAnError)
                {
                    SettingsInMemory.Cached.LastProgramUpdatesScheduledCheckAttemptionTime =
                        backupOfLastProgramUpdatesScheduledCheckAttemptionTime;
                    return false;
                }
                byte[] buffer = null;
                string jsonSerializedSettingsString = SettingsInMemory.GetJsonRepresentation();
                buffer = Encoding.UTF8.GetBytes(jsonSerializedSettingsString);
                fileStreamOfSettingsFile.Position = 0;
                fileStreamOfSettingsFile.SetLength(buffer.Length);
                fileStreamOfSettingsFile.Write(buffer);
                fileStreamOfSettingsFile.Flush();
                string checksumOfJsonSerializedSettingsString = HashingUtilities.GetMD5Hash(
                    jsonSerializedSettingsString
                );
                buffer = Encoding.UTF8.GetBytes(checksumOfJsonSerializedSettingsString);
                fileStreamOfSettingsChecksumFile.Position = 0;
                fileStreamOfSettingsChecksumFile.SetLength(buffer.Length);
                fileStreamOfSettingsChecksumFile.Write(buffer);
                fileStreamOfSettingsChecksumFile.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool LoadSettingsToMemoryFromSettingsFile(out ConfigError settingsFileError)
        {
            settingsFileError = ConfigError.Unspecified;
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            try
            {
                byte[] buffer = null;
                buffer = new byte[fileStreamOfSettingsFile.Length];
                fileStreamOfSettingsFile.Position = 0;
                fileStreamOfSettingsFile.Read(buffer);
                string savedJsonSerializedSettingsString = Encoding.UTF8.GetString(buffer);
                string checksumOfSavedJsonSerializedSettingsString = HashingUtilities.GetMD5Hash(
                    savedJsonSerializedSettingsString
                );
                buffer = new byte[fileStreamOfSettingsChecksumFile.Length];
                fileStreamOfSettingsChecksumFile.Position = 0;
                fileStreamOfSettingsChecksumFile.Read(buffer);
                string savedChecksumOfJsonSerializedSettingsString = Encoding.UTF8.GetString(buffer);
                if (!checksumOfSavedJsonSerializedSettingsString.Equals(
                        savedChecksumOfJsonSerializedSettingsString,
                        StringComparison.CurrentCultureIgnoreCase
                    ))
                {
                    settingsFileError = ConfigError.Corrupted;
                    return false;
                }
                try
                {
                    SettingsInMemory = Settings.CreateFromJsonRepresentation(
                        savedJsonSerializedSettingsString
                    );
                }
                catch (Settings.SettingsVersionIsNotCompatibleException)
                {
                    settingsFileError = ConfigError.NotCompatible;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                fileStreamOfSettingsFile?.Dispose();
                fileStreamOfSettingsFile = null;
                fileStreamOfSettingsChecksumFile?.Dispose();
                fileStreamOfSettingsChecksumFile = null;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
