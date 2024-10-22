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
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Data.SQLite;
using Scrupdate.Classes.Utilities;


namespace Scrupdate.Classes.Objects
{
    public class ProgramDatabase : IDisposable
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class DatabaseIsNotOpenException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE = "Database is not open!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public DatabaseIsNotOpenException() : base(EXCEPTION_MESSAGE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const int DATABASE_VERSION = 1;
        private const string TABLE_NAME__PROGRAMS = "programs";
        private const string TABLE_COLUMN_NAME__ID = "id";
        private const string TABLE_COLUMN_NAME__NAME = "name";
        private const string TABLE_COLUMN_NAME__INSTALLED_VERSION = "installed_version";
        private const string TABLE_COLUMN_NAME__LATEST_VERSION = "latest_version";
        private const string TABLE_COLUMN_NAME__INSTALLATION_SCOPE = "installation_scope";
        private const string TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED = "is_update_check_configured";
        private const string TABLE_COLUMN_NAME__WEB_PAGE_URL = "web_page_url";
        private const string TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD = "version_search_method";
        private const string TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1 = "version_search_method_argument_1";
        private const string TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2 = "version_search_method_argument_2";
        private const string TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION = "treat_a_standalone_number_as_a_version";
        private const string TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR = "version_search_behavior";
        private const string TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY = "web_page_post_load_delay";
        private const string TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON = "web_page_element_locating_instructions_of_web_page_elements_to_simulate_a_click_on";
        private const string TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED = "is_automatically_added";
        private const string TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS = "update_check_configuration_status";
        private const string TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR = "update_check_configuration_error";
        private const string TABLE_COLUMN_NAME__IS_HIDDEN = "is_hidden";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private volatile bool disposed;
        private StringBuilder tempStringBuilder;
        private string programDatabaseFilePath;
        private string programDatabaseChecksumFilePath;
        private FileStream fileStreamOfProgramDatabaseChecksumFile;
        private SQLiteConnection sqLiteConnection;
        private SQLiteTransaction currentSqLiteTransaction;
        private volatile bool open;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ProgramDatabase(string programDatabaseFilePath, string programDatabaseChecksumFilePath)
        {
            disposed = false;
            tempStringBuilder = new StringBuilder();
            this.programDatabaseFilePath = programDatabaseFilePath;
            this.programDatabaseChecksumFilePath = programDatabaseChecksumFilePath;
            open = false;
            tempStringBuilder.Clear().Append("Data Source='").Append(programDatabaseFilePath).Append('\'');
            sqLiteConnection = new SQLiteConnection(tempStringBuilder.ToString());
            currentSqLiteTransaction = null;
            if (!File.Exists(programDatabaseFilePath))
            {
                bool programDatabaseCreationWasSucceeded = false;
                try
                {
                    string programDatabaseFileDirectoryPath = Path.GetDirectoryName(programDatabaseFilePath);
                    if (!Directory.Exists(programDatabaseFileDirectoryPath))
                        Directory.CreateDirectory(programDatabaseFileDirectoryPath);
                    string programDatabaseChecksumFileDirectoryPath = Path.GetDirectoryName(programDatabaseChecksumFilePath);
                    if (!Directory.Exists(programDatabaseChecksumFileDirectoryPath))
                        Directory.CreateDirectory(programDatabaseChecksumFileDirectoryPath);
                    if (File.Exists(programDatabaseChecksumFilePath))
                        File.SetAttributes(programDatabaseChecksumFilePath, File.GetAttributes(programDatabaseChecksumFilePath) & (~FileAttributes.Hidden));
                    fileStreamOfProgramDatabaseChecksumFile = new FileStream(programDatabaseChecksumFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    File.SetAttributes(programDatabaseChecksumFilePath, File.GetAttributes(programDatabaseChecksumFilePath) | FileAttributes.Hidden);
                    sqLiteConnection.Open();
                    open = true;
                    tempStringBuilder.Clear().Append("PRAGMA user_version = ").Append(DATABASE_VERSION).Append(';');
                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand(tempStringBuilder.ToString(), sqLiteConnection))
                        sqLiteCommand.ExecuteNonQuery();
                    tempStringBuilder.Clear();
                    tempStringBuilder.Append($"CREATE TABLE {TABLE_NAME__PROGRAMS} (")
                        .Append($"{TABLE_COLUMN_NAME__ID} INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, ")
                        .Append($"{TABLE_COLUMN_NAME__NAME} TEXT NOT NULL UNIQUE, ")
                        .Append($"{TABLE_COLUMN_NAME__INSTALLED_VERSION} TEXT NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__LATEST_VERSION} TEXT NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__INSTALLATION_SCOPE} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_URL} TEXT NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1} TEXT NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2} TEXT NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON} TEXT NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR} INTEGER NOT NULL, ")
                        .Append($"{TABLE_COLUMN_NAME__IS_HIDDEN} INTEGER NOT NULL");
                    tempStringBuilder.Append(");");
                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand(tempStringBuilder.ToString(), sqLiteConnection))
                        sqLiteCommand.ExecuteNonQuery();
                    programDatabaseCreationWasSucceeded = true;
                }
                catch { }
                if (programDatabaseCreationWasSucceeded)
                    UpdateProgramDatabaseChecksumFile();
                fileStreamOfProgramDatabaseChecksumFile?.Dispose();
                fileStreamOfProgramDatabaseChecksumFile = null;
                sqLiteConnection.Close();
                open = false;
                if (!programDatabaseCreationWasSucceeded)
                {
                    if (File.Exists(programDatabaseFilePath))
                        File.Delete(programDatabaseFilePath);
                    if (File.Exists(programDatabaseChecksumFilePath))
                        File.Delete(programDatabaseChecksumFilePath);
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Destructors /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ~ProgramDatabase() => Dispose();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool IsOpen()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            return open;
        }
        public bool Open(out bool programDatabaseFileIsCorrupted)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            programDatabaseFileIsCorrupted = false;
            if (open)
                return true;
            if (!File.Exists(programDatabaseChecksumFilePath))
            {
                programDatabaseFileIsCorrupted = true;
                return false;
            }
            bool programDatabaseOpenWasSucceeded = false;
            try
            {
                string programDatabaseFileChecksum = HashingUtilities.GetMD5Hash(File.ReadAllBytes(programDatabaseFilePath));
                fileStreamOfProgramDatabaseChecksumFile = new FileStream(programDatabaseChecksumFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                File.SetAttributes(programDatabaseChecksumFilePath, File.GetAttributes(programDatabaseChecksumFilePath) | FileAttributes.Hidden);
                sqLiteConnection.Open();
                open = true;
                byte[] buffer = new byte[fileStreamOfProgramDatabaseChecksumFile.Length];
                fileStreamOfProgramDatabaseChecksumFile.Position = 0;
                fileStreamOfProgramDatabaseChecksumFile.Read(buffer);
                string savedChecksumOfProgramDatabaseFile = Encoding.UTF8.GetString(buffer);
                if (!programDatabaseFileChecksum.Equals(savedChecksumOfProgramDatabaseFile, StringComparison.CurrentCultureIgnoreCase))
                {
                    programDatabaseFileIsCorrupted = true;
                    return false;
                }
                programDatabaseOpenWasSucceeded = true;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (!programDatabaseOpenWasSucceeded)
                {
                    fileStreamOfProgramDatabaseChecksumFile?.Dispose();
                    fileStreamOfProgramDatabaseChecksumFile = null;
                    sqLiteConnection?.Close();
                    open = false;
                }
            }
        }
        public bool Close()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                return true;
            try
            {
                if (currentSqLiteTransaction != null)
                {
                    try
                    {
                        currentSqLiteTransaction.Commit();
                        currentSqLiteTransaction = null;
                    }
                    catch
                    {
                        return false;
                    }
                }
                UpdateProgramDatabaseChecksumFile();
                fileStreamOfProgramDatabaseChecksumFile.Dispose();
                fileStreamOfProgramDatabaseChecksumFile = null;
                sqLiteConnection.Close();
                open = false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool IsTransactionInProgress()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            return (currentSqLiteTransaction != null);
        }
        public bool BeginTransaction()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            if (currentSqLiteTransaction != null)
                return true;
            try
            {
                currentSqLiteTransaction = sqLiteConnection.BeginTransaction();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CommitTransaction()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            if (currentSqLiteTransaction == null)
                return true;
            try
            {
                currentSqLiteTransaction.Commit();
                currentSqLiteTransaction = null;
                UpdateProgramDatabaseChecksumFile();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool UpdateProgramDatabaseChecksumFile()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            try
            {
                string programDatabaseFileChecksum;
                sqLiteConnection.Close();
                try
                {
                    programDatabaseFileChecksum = HashingUtilities.GetMD5Hash(File.ReadAllBytes(programDatabaseFilePath));
                }
                finally
                {
                    sqLiteConnection.Open();
                }
                byte[] buffer = Encoding.UTF8.GetBytes(programDatabaseFileChecksum);
                fileStreamOfProgramDatabaseChecksumFile.Position = 0;
                fileStreamOfProgramDatabaseChecksumFile.SetLength(buffer.Length);
                fileStreamOfProgramDatabaseChecksumFile.Write(buffer);
                fileStreamOfProgramDatabaseChecksumFile.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool AddNewProgram(Program program)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            tempStringBuilder.Clear();
            tempStringBuilder.Append($"INSERT INTO {TABLE_NAME__PROGRAMS} (")
                .Append($"{TABLE_COLUMN_NAME__NAME}, ")
                .Append($"{TABLE_COLUMN_NAME__INSTALLED_VERSION}, ")
                .Append($"{TABLE_COLUMN_NAME__LATEST_VERSION}, ")
                .Append($"{TABLE_COLUMN_NAME__INSTALLATION_SCOPE}, ")
                .Append($"{TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED}, ")
                .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_URL}, ")
                .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD}, ")
                .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1}, ")
                .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2}, ")
                .Append($"{TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION}, ")
                .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR}, ")
                .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY}, ")
                .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON}, ")
                .Append($"{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED}, ")
                .Append($"{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS}, ")
                .Append($"{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR}, ")
                .Append($"{TABLE_COLUMN_NAME__IS_HIDDEN}");
            tempStringBuilder.Append(") VALUES (")
                .Append($"@{TABLE_COLUMN_NAME__NAME}, ")
                .Append($"@{TABLE_COLUMN_NAME__INSTALLED_VERSION}, ")
                .Append($"@{TABLE_COLUMN_NAME__LATEST_VERSION}, ")
                .Append($"@{TABLE_COLUMN_NAME__INSTALLATION_SCOPE}, ")
                .Append($"@{TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED}, ")
                .Append($"@{TABLE_COLUMN_NAME__WEB_PAGE_URL}, ")
                .Append($"@{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD}, ")
                .Append($"@{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1}, ")
                .Append($"@{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2}, ")
                .Append($"@{TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION}, ")
                .Append($"@{TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR}, ")
                .Append($"@{TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY}, ")
                .Append($"@{TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON}, ")
                .Append($"@{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED}, ")
                .Append($"@{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS}, ")
                .Append($"@{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR}, ")
                .Append($"@{TABLE_COLUMN_NAME__IS_HIDDEN}");
            tempStringBuilder.Append(");");
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(tempStringBuilder.ToString(), sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, program.Name);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__INSTALLED_VERSION, program.InstalledVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__LATEST_VERSION, program.LatestVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__INSTALLATION_SCOPE, (long)program.InstallationScope);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED, Convert.ToInt64(program.IsUpdateCheckConfigured));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__WEB_PAGE_URL, program.WebPageUrl);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD, (long)program.VersionSearchMethod);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1, program.VersionSearchMethodArgument1);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2, program.VersionSearchMethodArgument2);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION, Convert.ToInt64(program.TreatAStandaloneNumberAsAVersion));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR, (long)program.VersionSearchBehavior);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY, (long)program.WebPagePostLoadDelay);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON, JsonSerializer.Serialize(program.WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED, Convert.ToInt64(program.IsAutomaticallyAdded));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS, (long)program.UpdateCheckConfigurationStatus);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR, (long)program.UpdateCheckConfigurationError);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__IS_HIDDEN, Convert.ToInt64(program.IsHidden));
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool IsProgramHidden(string programName)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool programIsHidden = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"SELECT {TABLE_COLUMN_NAME__IS_HIDDEN} FROM {TABLE_NAME__PROGRAMS} WHERE {TABLE_COLUMN_NAME__NAME} = @{TABLE_COLUMN_NAME__NAME};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, programName);
                try
                {
                    SQLiteDataReader sQLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        if (sQLiteDataReader.Read())
                            programIsHidden = Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__IS_HIDDEN]);
                    }
                    catch { }
                    sQLiteDataReader.Close();
                }
                catch { }
            }
            return programIsHidden;
        }
        public bool HideProgram(string programName)
        {
            return HideOrUnhideProgram(programName, true);
        }
        public bool UnhideProgram(string programName)
        {
            return HideOrUnhideProgram(programName, false);
        }
        private bool HideOrUnhideProgram(string programName, bool hidden)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN_NAME__IS_HIDDEN} = @new_{TABLE_COLUMN_NAME__IS_HIDDEN} WHERE {TABLE_COLUMN_NAME__NAME} = @{TABLE_COLUMN_NAME__NAME};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__IS_HIDDEN}", hidden ? 1L : 0L);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, programName);
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool ChangeProgramConfigurationStatus(string programName, Program.ProgramUpdateCheckConfigurationStatus updateCheckConfigurationStatus, Program.ProgramUpdateCheckConfigurationError updateCheckConfigurationError)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS} = @new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS}, {TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR} = @new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR} WHERE {TABLE_COLUMN_NAME__NAME} = @{TABLE_COLUMN_NAME__NAME};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS}", (long)updateCheckConfigurationStatus);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR}", (long)updateCheckConfigurationError);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, programName);
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool UpdateProgramInstallationInfoToAutomaticallyDetectedProgram(string programName, string installedVersion, Program.ProgramInstallationScope installationScope)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN_NAME__INSTALLED_VERSION} = @new_{TABLE_COLUMN_NAME__INSTALLED_VERSION}, {TABLE_COLUMN_NAME__INSTALLATION_SCOPE} = @new_{TABLE_COLUMN_NAME__INSTALLATION_SCOPE} WHERE {TABLE_COLUMN_NAME__NAME} = @{TABLE_COLUMN_NAME__NAME} AND {TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED} = @{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__INSTALLED_VERSION}", installedVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__INSTALLATION_SCOPE}", (long)installationScope);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, programName);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED, 1);
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool UpdateProgramLatestVersion(string programName, string latestVersion)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN_NAME__LATEST_VERSION} = @new_{TABLE_COLUMN_NAME__LATEST_VERSION} WHERE {TABLE_COLUMN_NAME__NAME} = @{TABLE_COLUMN_NAME__NAME};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__LATEST_VERSION}", latestVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, programName);
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool ResetLatestVersionAndUpdateCheckConfigurationStatusOfAllConfiguredPrograms()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN_NAME__LATEST_VERSION} = @new_{TABLE_COLUMN_NAME__LATEST_VERSION}, {TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS} = @new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS}, {TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR} = @new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR} WHERE {TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED} = @{TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__LATEST_VERSION}", "");
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS}", (long)Program.ProgramUpdateCheckConfigurationStatus.Unknown);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR}", (long)Program.ProgramUpdateCheckConfigurationError.None);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED, 1);
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool ConvertAllProgramsToManuallyInstalledPrograms()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED} = @new_{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED}", 0);
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool RemoveProgram(string programName)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"DELETE FROM {TABLE_NAME__PROGRAMS} WHERE {TABLE_COLUMN_NAME__NAME} = @{TABLE_COLUMN_NAME__NAME};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, programName);
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public Program GetProgram(string programName)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            Program program = null;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"SELECT * FROM {TABLE_NAME__PROGRAMS} WHERE {TABLE_COLUMN_NAME__NAME} = @{TABLE_COLUMN_NAME__NAME};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, programName);
                try
                {
                    SQLiteDataReader sQLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        if (sQLiteDataReader.Read())
                        {
                            program = new Program(
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__NAME],
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__INSTALLED_VERSION],
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__LATEST_VERSION],
                                    (Program.ProgramInstallationScope)((long)sQLiteDataReader[TABLE_COLUMN_NAME__INSTALLATION_SCOPE]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED]),
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__WEB_PAGE_URL],
                                    (Program.ProgramVersionSearchMethod)((long)sQLiteDataReader[TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD]),
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1],
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2],
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION]),
                                    (Program.ProgramVersionSearchBehavior)((long)sQLiteDataReader[TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR]),
                                    (Program.ProgramWebPagePostLoadDelay)((long)sQLiteDataReader[TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY]),
                                    JsonSerializer.Deserialize<List<WebPageElementLocatingInstruction>>((string)sQLiteDataReader[TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED]),
                                    (Program.ProgramUpdateCheckConfigurationStatus)((long)sQLiteDataReader[TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS]),
                                    (Program.ProgramUpdateCheckConfigurationError)((long)sQLiteDataReader[TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__IS_HIDDEN])
                                ); ;
                        }
                    }
                    catch { }
                    sQLiteDataReader.Close();
                }
                catch { }
            }
            return program;
        }
        public Dictionary<string, Program> GetPrograms()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            Dictionary<string, Program> programs = new Dictionary<string, Program>();
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"SELECT * FROM {TABLE_NAME__PROGRAMS};", sqLiteConnection))
            {
                try
                {
                    SQLiteDataReader sQLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        while (sQLiteDataReader.Read())
                        {
                            programs.Add((string)sQLiteDataReader[TABLE_COLUMN_NAME__NAME], new Program(
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__NAME],
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__INSTALLED_VERSION],
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__LATEST_VERSION],
                                    (Program.ProgramInstallationScope)((long)sQLiteDataReader[TABLE_COLUMN_NAME__INSTALLATION_SCOPE]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED]),
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__WEB_PAGE_URL],
                                    (Program.ProgramVersionSearchMethod)((long)sQLiteDataReader[TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD]),
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1],
                                    (string)sQLiteDataReader[TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2],
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION]),
                                    (Program.ProgramVersionSearchBehavior)((long)sQLiteDataReader[TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR]),
                                    (Program.ProgramWebPagePostLoadDelay)((long)sQLiteDataReader[TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY]),
                                    JsonSerializer.Deserialize<List<WebPageElementLocatingInstruction>>((string)sQLiteDataReader[TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED]),
                                    (Program.ProgramUpdateCheckConfigurationStatus)((long)sQLiteDataReader[TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS]),
                                    (Program.ProgramUpdateCheckConfigurationError)((long)sQLiteDataReader[TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN_NAME__IS_HIDDEN])
                                ));
                        }
                    }
                    catch { }
                    sQLiteDataReader.Close();
                }
                catch { }
            }
            return programs;
        }
        public bool UpdateProgram(string programName, Program newProgram)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            tempStringBuilder.Clear();
            tempStringBuilder.Append($"UPDATE {TABLE_NAME__PROGRAMS} SET ")
                .Append($"{TABLE_COLUMN_NAME__NAME} = @new_{TABLE_COLUMN_NAME__NAME}, ")
                .Append($"{TABLE_COLUMN_NAME__INSTALLED_VERSION} = @new_{TABLE_COLUMN_NAME__INSTALLED_VERSION}, ")
                .Append($"{TABLE_COLUMN_NAME__LATEST_VERSION} = @new_{TABLE_COLUMN_NAME__LATEST_VERSION}, ")
                .Append($"{TABLE_COLUMN_NAME__INSTALLATION_SCOPE} = @new_{TABLE_COLUMN_NAME__INSTALLATION_SCOPE}, ")
                .Append($"{TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED} = @new_{TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED}, ")
                .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_URL} = @new_{TABLE_COLUMN_NAME__WEB_PAGE_URL}, ")
                .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD} = @new_{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD}, ")
                .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1} = @new_{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1}, ")
                .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2} = @new_{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2}, ")
                .Append($"{TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION} = @new_{TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION}, ")
                .Append($"{TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR} = @new_{TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR}, ")
                .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY} = @new_{TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY}, ")
                .Append($"{TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON} = @new_{TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON}, ")
                .Append($"{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED} = @new_{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED}, ")
                .Append($"{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS} = @new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS}, ")
                .Append($"{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR} = @new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR}, ")
                .Append($"{TABLE_COLUMN_NAME__IS_HIDDEN} = @new_{TABLE_COLUMN_NAME__IS_HIDDEN} ");
            tempStringBuilder.Append($"WHERE {TABLE_COLUMN_NAME__NAME} = @{TABLE_COLUMN_NAME__NAME};");
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(tempStringBuilder.ToString(), sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__NAME}", newProgram.Name);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__INSTALLED_VERSION}", newProgram.InstalledVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__LATEST_VERSION}", newProgram.LatestVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__INSTALLATION_SCOPE}", (long)newProgram.InstallationScope);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__IS_UPDATE_CHECK_CONFIGURED}", Convert.ToInt64(newProgram.IsUpdateCheckConfigured));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__WEB_PAGE_URL}", newProgram.WebPageUrl);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD}", (long)newProgram.VersionSearchMethod);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_1}", newProgram.VersionSearchMethodArgument1);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__VERSION_SEARCH_METHOD_ARGUMENT_2}", newProgram.VersionSearchMethodArgument2);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION}", Convert.ToInt64(newProgram.TreatAStandaloneNumberAsAVersion));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__VERSION_SEARCH_BEHAVIOR}", (long)newProgram.VersionSearchBehavior);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__WEB_PAGE_POST_LOAD_DELAY}", (long)newProgram.WebPagePostLoadDelay);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON}", JsonSerializer.Serialize(newProgram.WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__IS_AUTOMATICALLY_ADDED}", Convert.ToInt64(newProgram.IsAutomaticallyAdded));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_STATUS}", (long)newProgram.UpdateCheckConfigurationStatus);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__UPDATE_CHECK_CONFIGURATION_ERROR}", (long)newProgram.UpdateCheckConfigurationError);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN_NAME__IS_HIDDEN}", Convert.ToInt64(newProgram.IsHidden));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN_NAME__NAME, programName);
                try
                {
                    succeeded = (sqLiteCommand.ExecuteNonQuery() > 0);
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public void Dispose()
        {
            if (!disposed)
            {
                Close();
                disposed = true;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
