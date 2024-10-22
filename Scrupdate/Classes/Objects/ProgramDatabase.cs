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
        public readonly struct TableColumn
        {
            // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public string Name { get; init; }
            public string Properties { get; init; }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public TableColumn() : this("", "") { }
            public TableColumn(string name, string properties)
            {
                Name = name;
                Properties = properties;
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public string ToSqlString()
            {
                StringBuilder sqlString = new StringBuilder();
                sqlString.Append(Name).Append(' ').Append(Properties);
                return sqlString.ToString();
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const int DATABASE_VERSION = 1;
        private const string TABLE_NAME__PROGRAMS = "programs";
        private static readonly TableColumn TABLE_COLUMN__ID = new TableColumn("id", "INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
        private static readonly TableColumn TABLE_COLUMN__NAME = new TableColumn("name", "TEXT NOT NULL UNIQUE");
        private static readonly TableColumn TABLE_COLUMN__INSTALLED_VERSION = new TableColumn("installed_version", "TEXT NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__LATEST_VERSION = new TableColumn("latest_version", "TEXT NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__INSTALLATION_SCOPE = new TableColumn("installation_scope", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED = new TableColumn("is_update_check_configured", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__WEB_PAGE_URL = new TableColumn("web_page_url", "TEXT NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__VERSION_SEARCH_METHOD = new TableColumn("version_search_method", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1 = new TableColumn("version_search_method_argument_1", "TEXT NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2 = new TableColumn("version_search_method_argument_2", "TEXT NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION = new TableColumn("treat_a_standalone_number_as_a_version", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR = new TableColumn("version_search_behavior", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY = new TableColumn("web_page_post_load_delay", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON = new TableColumn("web_page_element_locating_instructions_of_web_page_elements_to_simulate_a_click_on", "TEXT NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__IS_AUTOMATICALLY_ADDED = new TableColumn("is_automatically_added", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS = new TableColumn("update_check_configuration_status", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR = new TableColumn("update_check_configuration_error", "INTEGER NOT NULL");
        private static readonly TableColumn TABLE_COLUMN__IS_HIDDEN = new TableColumn("is_hidden", "INTEGER NOT NULL");
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
                        .Append($"{TABLE_COLUMN__ID.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__NAME.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__INSTALLED_VERSION.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__LATEST_VERSION.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__INSTALLATION_SCOPE.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__WEB_PAGE_URL.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.ToSqlString()}, ")
                        .Append($"{TABLE_COLUMN__IS_HIDDEN.ToSqlString()}");
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
                .Append($"{TABLE_COLUMN__NAME.Name}, ")
                .Append($"{TABLE_COLUMN__INSTALLED_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__LATEST_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__INSTALLATION_SCOPE.Name}, ")
                .Append($"{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name}, ")
                .Append($"{TABLE_COLUMN__WEB_PAGE_URL.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name}, ")
                .Append($"{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name}, ")
                .Append($"{TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.Name}, ")
                .Append($"{TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name}, ")
                .Append($"{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}, ")
                .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, ")
                .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}, ")
                .Append($"{TABLE_COLUMN__IS_HIDDEN.Name}");
            tempStringBuilder.Append(") VALUES (")
                .Append($"@{TABLE_COLUMN__NAME.Name}, ")
                .Append($"@{TABLE_COLUMN__INSTALLED_VERSION.Name}, ")
                .Append($"@{TABLE_COLUMN__LATEST_VERSION.Name}, ")
                .Append($"@{TABLE_COLUMN__INSTALLATION_SCOPE.Name}, ")
                .Append($"@{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name}, ")
                .Append($"@{TABLE_COLUMN__WEB_PAGE_URL.Name}, ")
                .Append($"@{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name}, ")
                .Append($"@{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name}, ")
                .Append($"@{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name}, ")
                .Append($"@{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name}, ")
                .Append($"@{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name}, ")
                .Append($"@{TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.Name}, ")
                .Append($"@{TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name}, ")
                .Append($"@{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}, ")
                .Append($"@{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, ")
                .Append($"@{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}, ")
                .Append($"@{TABLE_COLUMN__IS_HIDDEN.Name}");
            tempStringBuilder.Append(");");
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(tempStringBuilder.ToString(), sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, program.Name);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__INSTALLED_VERSION.Name, program.InstalledVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__LATEST_VERSION.Name, program.LatestVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__INSTALLATION_SCOPE.Name, (long)program.InstallationScope);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name, Convert.ToInt64(program.IsUpdateCheckConfigured));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__WEB_PAGE_URL.Name, program.WebPageUrl);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__VERSION_SEARCH_METHOD.Name, (long)program.VersionSearchMethod);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name, program.VersionSearchMethodArgument1);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name, program.VersionSearchMethodArgument2);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name, Convert.ToInt64(program.TreatAStandaloneNumberAsAVersion));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name, (long)program.VersionSearchBehavior);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.Name, (long)program.WebPagePostLoadDelay);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name, JsonSerializer.Serialize(program.WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name, Convert.ToInt64(program.IsAutomaticallyAdded));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name, (long)program.UpdateCheckConfigurationStatus);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name, (long)program.UpdateCheckConfigurationError);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_HIDDEN.Name, Convert.ToInt64(program.IsHidden));
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"SELECT {TABLE_COLUMN__IS_HIDDEN.Name} FROM {TABLE_NAME__PROGRAMS} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
                try
                {
                    SQLiteDataReader sQLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        if (sQLiteDataReader.Read())
                            programIsHidden = Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__IS_HIDDEN.Name]);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__IS_HIDDEN.Name} = @new_{TABLE_COLUMN__IS_HIDDEN.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_HIDDEN.Name}", hidden ? 1L : 0L);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, {TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}", (long)updateCheckConfigurationStatus);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}", (long)updateCheckConfigurationError);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__INSTALLED_VERSION.Name} = @new_{TABLE_COLUMN__INSTALLED_VERSION.Name}, {TABLE_COLUMN__INSTALLATION_SCOPE.Name} = @new_{TABLE_COLUMN__INSTALLATION_SCOPE.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name} AND {TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name} = @{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__INSTALLED_VERSION.Name}", installedVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__INSTALLATION_SCOPE.Name}", (long)installationScope);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name, 1);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__LATEST_VERSION.Name} = @new_{TABLE_COLUMN__LATEST_VERSION.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__LATEST_VERSION.Name}", latestVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__LATEST_VERSION.Name} = @new_{TABLE_COLUMN__LATEST_VERSION.Name}, {TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, {TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} WHERE {TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name} = @{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__LATEST_VERSION.Name}", "");
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}", (long)Program.ProgramUpdateCheckConfigurationStatus.Unknown);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}", (long)Program.ProgramUpdateCheckConfigurationError.None);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name, 1);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name} = @new_{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}", 0);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"DELETE FROM {TABLE_NAME__PROGRAMS} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"SELECT * FROM {TABLE_NAME__PROGRAMS} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};", sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
                try
                {
                    SQLiteDataReader sQLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        if (sQLiteDataReader.Read())
                        {
                            program = new Program(
                                    (string)sQLiteDataReader[TABLE_COLUMN__NAME.Name],
                                    (string)sQLiteDataReader[TABLE_COLUMN__INSTALLED_VERSION.Name],
                                    (string)sQLiteDataReader[TABLE_COLUMN__LATEST_VERSION.Name],
                                    (Program.ProgramInstallationScope)((long)sQLiteDataReader[TABLE_COLUMN__INSTALLATION_SCOPE.Name]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name]),
                                    (string)sQLiteDataReader[TABLE_COLUMN__WEB_PAGE_URL.Name],
                                    (Program.ProgramVersionSearchMethod)((long)sQLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD.Name]),
                                    (string)sQLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name],
                                    (string)sQLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name],
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name]),
                                    (Program.ProgramVersionSearchBehavior)((long)sQLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name]),
                                    (Program.ProgramWebPagePostLoadDelay)((long)sQLiteDataReader[TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.Name]),
                                    JsonSerializer.Deserialize<List<WebPageElementLocatingInstruction>>((string)sQLiteDataReader[TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name]),
                                    (Program.ProgramUpdateCheckConfigurationStatus)((long)sQLiteDataReader[TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name]),
                                    (Program.ProgramUpdateCheckConfigurationError)((long)sQLiteDataReader[TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__IS_HIDDEN.Name])
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
                            programs.Add((string)sQLiteDataReader[TABLE_COLUMN__NAME.Name], new Program(
                                    (string)sQLiteDataReader[TABLE_COLUMN__NAME.Name],
                                    (string)sQLiteDataReader[TABLE_COLUMN__INSTALLED_VERSION.Name],
                                    (string)sQLiteDataReader[TABLE_COLUMN__LATEST_VERSION.Name],
                                    (Program.ProgramInstallationScope)((long)sQLiteDataReader[TABLE_COLUMN__INSTALLATION_SCOPE.Name]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name]),
                                    (string)sQLiteDataReader[TABLE_COLUMN__WEB_PAGE_URL.Name],
                                    (Program.ProgramVersionSearchMethod)((long)sQLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD.Name]),
                                    (string)sQLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name],
                                    (string)sQLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name],
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name]),
                                    (Program.ProgramVersionSearchBehavior)((long)sQLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name]),
                                    (Program.ProgramWebPagePostLoadDelay)((long)sQLiteDataReader[TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.Name]),
                                    JsonSerializer.Deserialize<List<WebPageElementLocatingInstruction>>((string)sQLiteDataReader[TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name]),
                                    (Program.ProgramUpdateCheckConfigurationStatus)((long)sQLiteDataReader[TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name]),
                                    (Program.ProgramUpdateCheckConfigurationError)((long)sQLiteDataReader[TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name]),
                                    Convert.ToBoolean((long)sQLiteDataReader[TABLE_COLUMN__IS_HIDDEN.Name])
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
                .Append($"{TABLE_COLUMN__NAME.Name} = @new_{TABLE_COLUMN__NAME.Name}, ")
                .Append($"{TABLE_COLUMN__INSTALLED_VERSION.Name} = @new_{TABLE_COLUMN__INSTALLED_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__LATEST_VERSION.Name} = @new_{TABLE_COLUMN__LATEST_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__INSTALLATION_SCOPE.Name} = @new_{TABLE_COLUMN__INSTALLATION_SCOPE.Name}, ")
                .Append($"{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name} = @new_{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name}, ")
                .Append($"{TABLE_COLUMN__WEB_PAGE_URL.Name} = @new_{TABLE_COLUMN__WEB_PAGE_URL.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name} = @new_{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name} = @new_{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name} = @new_{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name}, ")
                .Append($"{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name} = @new_{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name} = @new_{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name}, ")
                .Append($"{TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.Name} = @new_{TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.Name}, ")
                .Append($"{TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name} = @new_{TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name}, ")
                .Append($"{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name} = @new_{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}, ")
                .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, ")
                .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}, ")
                .Append($"{TABLE_COLUMN__IS_HIDDEN.Name} = @new_{TABLE_COLUMN__IS_HIDDEN.Name} ");
            tempStringBuilder.Append($"WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};");
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(tempStringBuilder.ToString(), sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__NAME.Name}", newProgram.Name);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__INSTALLED_VERSION.Name}", newProgram.InstalledVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__LATEST_VERSION.Name}", newProgram.LatestVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__INSTALLATION_SCOPE.Name}", (long)newProgram.InstallationScope);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name}", Convert.ToInt64(newProgram.IsUpdateCheckConfigured));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__WEB_PAGE_URL.Name}", newProgram.WebPageUrl);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name}", (long)newProgram.VersionSearchMethod);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name}", newProgram.VersionSearchMethodArgument1);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name}", newProgram.VersionSearchMethodArgument2);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name}", Convert.ToInt64(newProgram.TreatAStandaloneNumberAsAVersion));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name}", (long)newProgram.VersionSearchBehavior);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__WEB_PAGE_POST_LOAD_DELAY.Name}", (long)newProgram.WebPagePostLoadDelay);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__WEB_PAGE_ELEMENT_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name}", JsonSerializer.Serialize(newProgram.WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}", Convert.ToInt64(newProgram.IsAutomaticallyAdded));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}", (long)newProgram.UpdateCheckConfigurationStatus);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}", (long)newProgram.UpdateCheckConfigurationError);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_HIDDEN.Name}", Convert.ToInt64(newProgram.IsHidden));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
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
