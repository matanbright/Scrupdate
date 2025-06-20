﻿// Copyright © 2021-2025 Matan Brightbert
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
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Scrupdate.Classes.Utilities;


namespace Scrupdate.Classes.Objects
{
    public class ProgramDatabase : IDisposable
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class DatabaseIsNotOpenException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Database is not open!";
            public DatabaseIsNotOpenException() : base(EXCEPTION_MESSAGE) { }
        }
        public class TransactionIsInProgressException : Exception
        {
            private const string EXCEPTION_MESSAGE = "A transaction is in progress!";
            public TransactionIsInProgressException() : base(EXCEPTION_MESSAGE) { }
        }
        public readonly struct TableColumn
        {
            public string Name { get; init; }
            public string Properties { get; init; }
            public TableColumn() : this("", "") { }
            public TableColumn(string name, string properties)
            {
                Name = name;
                Properties = properties;
            }
            public string ToSqlString()
            {
                StringBuilder sqlString = new StringBuilder(
                    Name.Length + 1 + Properties.Length
                );
                sqlString
                    .Append(Name)
                    .Append(' ')
                    .Append(Properties);
                return sqlString.ToString();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static readonly Version DATABASE_VERSION = new Version(1, 1); /* Note: When changing or removing existing table columns, adding new non-removable table columns (e.g, primary-key, unique),
                                                                               *       or adding new non-null table columns without a default value, the major number should be incremented and the minor number should be zeroed.
                                                                               *       Otherwise, when adding new table columns, the minor number should be incremented.
                                                                               */
        private const int INITIAL_CAPACITY_OF_SQL_QUERY_STRING_BUILDER = 10000;
        private const string TABLE_NAME__PROGRAMS = "programs";
        /* Note: The 'TABLE_COLUMN' constants may be unused in the code and the IDE may warn you about this, but don't remove them!
         *       They are used in run-time (using reflection) to create the table in the database.
         */
        private static readonly TableColumn TABLE_COLUMN__ID = new TableColumn("id", "INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
        private static readonly TableColumn TABLE_COLUMN__NAME = new TableColumn("name", "TEXT NOT NULL UNIQUE");
        private static readonly TableColumn TABLE_COLUMN__INSTALLED_VERSION = new TableColumn("installed_version", "TEXT NOT NULL DEFAULT \"\"");
        private static readonly TableColumn TABLE_COLUMN__LATEST_VERSION = new TableColumn("latest_version", "TEXT NOT NULL DEFAULT \"\"");
        private static readonly TableColumn TABLE_COLUMN__INSTALLATION_SCOPE = new TableColumn("installation_scope", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__IS_AUTOMATICALLY_ADDED = new TableColumn("is_automatically_added", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED = new TableColumn("is_update_check_configured", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__WEBPAGE_URL = new TableColumn("web_page_url", "TEXT NOT NULL DEFAULT \"\"");
        private static readonly TableColumn TABLE_COLUMN__VERSION_SEARCH_METHOD = new TableColumn("version_search_method", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1 = new TableColumn("version_search_method_argument_1", "TEXT NOT NULL DEFAULT \"\"");
        private static readonly TableColumn TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2 = new TableColumn("version_search_method_argument_2", "TEXT NOT NULL DEFAULT \"\"");
        private static readonly TableColumn TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION = new TableColumn("treat_a_standalone_number_as_a_version", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR = new TableColumn("version_search_behavior", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY = new TableColumn("web_page_post_load_delay", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON = new TableColumn("locating_instructions_of_web_page_elements_to_simulate_a_click_on", "TEXT NOT NULL DEFAULT \"\"");
        private static readonly TableColumn TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS = new TableColumn("update_check_configuration_status", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR = new TableColumn("update_check_configuration_error", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__SKIPPED_VERSION = new TableColumn("skipped_version", "TEXT NOT NULL DEFAULT \"\"");
        private static readonly TableColumn TABLE_COLUMN__IS_HIDDEN = new TableColumn("is_hidden", "INTEGER NOT NULL DEFAULT 0");
        private static readonly TableColumn TABLE_COLUMN__IS_NEW = new TableColumn("is_new", "INTEGER NOT NULL DEFAULT 0");
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private volatile bool disposed;
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
            this.programDatabaseFilePath = programDatabaseFilePath;
            this.programDatabaseChecksumFilePath = programDatabaseChecksumFilePath;
            open = false;
            StringBuilder sqlQueryString = new StringBuilder(
                13 + programDatabaseFilePath.Length + 1
            );
            sqlQueryString
                .Append("Data Source='")
                .Append(programDatabaseFilePath)
                .Append('\'');
            sqLiteConnection = new SQLiteConnection(sqlQueryString.ToString());
            currentSqLiteTransaction = null;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Destructors /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ~ProgramDatabase() => Dispose(false);
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static uint GetIntegerFromAVersion(Version version)
        {
            if (version.MajorNumber > 99)
                throw new ArgumentOutOfRangeException(nameof(version) + '.' + nameof(version.MajorNumber));
            if (version.MinorNumber > 99)
                throw new ArgumentOutOfRangeException(nameof(version) + '.' + nameof(version.MinorNumber));
            return version.MajorNumber * 100 + version.MinorNumber;
        }
        private static Version GetVersionFromAnInteger(uint versionInteger)
        {
            if (versionInteger > 9999)
                throw new ArgumentOutOfRangeException(nameof(versionInteger));
            return new Version(versionInteger / 100, versionInteger % 100);
        }
        private static List<TableColumn> GetActualTableColumns()
        {
            return ReflectionUtilities.GetStaticFields(
                       typeof(ProgramDatabase),
                       "TABLE_COLUMN__"
                   ).ConvertAll(x => (TableColumn)x.Value);
        }
        public bool Create()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (File.Exists(programDatabaseFilePath))
                return false;
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
                {
                    File.SetAttributes(
                        programDatabaseChecksumFilePath,
                        File.GetAttributes(programDatabaseChecksumFilePath) & ~FileAttributes.Hidden
                    );
                }
                fileStreamOfProgramDatabaseChecksumFile = new FileStream(
                    programDatabaseChecksumFilePath,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.Read
                );
                File.SetAttributes(
                    programDatabaseChecksumFilePath,
                    File.GetAttributes(programDatabaseChecksumFilePath) | FileAttributes.Hidden
                );
                sqLiteConnection.Open();
                open = true;
                StringBuilder sqlQueryString = new StringBuilder(22 + 10 + 1);
                sqlQueryString
                    .Append("PRAGMA user_version = ")
                    .Append(GetIntegerFromAVersion(DATABASE_VERSION))
                    .Append(';');
                using (SQLiteCommand sqLiteCommand = new SQLiteCommand(sqlQueryString.ToString(), sqLiteConnection))
                    sqLiteCommand.ExecuteNonQuery();
                List<TableColumn> actualTableColumns = GetActualTableColumns();
                sqlQueryString = new StringBuilder(
                    INITIAL_CAPACITY_OF_SQL_QUERY_STRING_BUILDER
                );
                sqlQueryString.Append($"CREATE TABLE {TABLE_NAME__PROGRAMS} (");
                for (int i = 0; i < actualTableColumns.Count; i++)
                {
                    TableColumn actualTableColumn = actualTableColumns[i];
                    sqlQueryString.Append(actualTableColumn.ToSqlString());
                    if (i < actualTableColumns.Count - 1)
                        sqlQueryString.Append(", ");
                }
                sqlQueryString.Append(");");
                using (SQLiteCommand sqLiteCommand = new SQLiteCommand(sqlQueryString.ToString(), sqLiteConnection))
                    sqLiteCommand.ExecuteNonQuery();
                programDatabaseCreationWasSucceeded = true;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (programDatabaseCreationWasSucceeded)
                    UpdateProgramDatabaseChecksumFile();
                if (fileStreamOfProgramDatabaseChecksumFile != null)
                {
                    fileStreamOfProgramDatabaseChecksumFile.Dispose();
                    fileStreamOfProgramDatabaseChecksumFile = null;
                }
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
        public bool IsOpen()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            return open;
        }
        public bool Open(out ConfigError programDatabaseFileError)
        {
            return Open(false, false, out programDatabaseFileError);
        }
        public bool Open(bool shouldCreateIfNotExist,
                         bool shouldUpgradeOrDowngradeIfCompatible,
                         out ConfigError programDatabaseFileError)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            programDatabaseFileError = ConfigError.Unspecified;
            if (open)
                return true;
            if (!File.Exists(programDatabaseFilePath))
            {
                if (!shouldCreateIfNotExist)
                    return false;
                if (!Create())
                    return false;
            }
            if (!File.Exists(programDatabaseChecksumFilePath))
            {
                programDatabaseFileError = ConfigError.Corrupted;
                return false;
            }
            bool programDatabaseOpenWasSucceeded = false;
            try
            {
                string programDatabaseFileChecksum = HashingUtilities.GetMD5Hash(
                    File.ReadAllBytes(programDatabaseFilePath)
                );
                fileStreamOfProgramDatabaseChecksumFile = new FileStream(
                    programDatabaseChecksumFilePath,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.Read
                );
                File.SetAttributes(
                    programDatabaseChecksumFilePath,
                    File.GetAttributes(programDatabaseChecksumFilePath) | FileAttributes.Hidden
                );
                sqLiteConnection.Open();
                open = true;
                byte[] buffer = new byte[fileStreamOfProgramDatabaseChecksumFile.Length];
                fileStreamOfProgramDatabaseChecksumFile.Position = 0;
                fileStreamOfProgramDatabaseChecksumFile.Read(buffer);
                string savedChecksumOfProgramDatabaseFile = Encoding.UTF8.GetString(buffer);
                if (!programDatabaseFileChecksum.Equals(
                        savedChecksumOfProgramDatabaseFile,
                        StringComparison.CurrentCultureIgnoreCase
                    ))
                {
                    programDatabaseFileError = ConfigError.Corrupted;
                    return false;
                }
                Version programDatabaseVersion = GetVersion();
                if (programDatabaseVersion.MajorNumber != DATABASE_VERSION.MajorNumber)
                {
                    programDatabaseFileError = ConfigError.NotCompatible;
                    return false;
                }
                if (programDatabaseVersion.MinorNumber != DATABASE_VERSION.MinorNumber)
                {
                    if (!shouldUpgradeOrDowngradeIfCompatible)
                        return false;
                    if (!UpgradeOrDowngrade())
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
                    if (fileStreamOfProgramDatabaseChecksumFile != null)
                    {
                        fileStreamOfProgramDatabaseChecksumFile.Dispose();
                        fileStreamOfProgramDatabaseChecksumFile = null;
                    }
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
        private bool UpgradeOrDowngrade()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            if (currentSqLiteTransaction != null)
                throw new TransactionIsInProgressException();
            try
            {
                Dictionary<string, TableColumn> actualTableColumns = GetActualTableColumns().ToDictionary(x => x.Name, x => x);
                List<string> savedTableColumnNames = GetTableColumnNames();
                List<string> actualTableColumnNames = actualTableColumns.Keys.ToList();
                if (!savedTableColumnNames.TrueForAll(x => actualTableColumnNames.Any(y => y.Equals(x))) &&
                    !actualTableColumnNames.TrueForAll(x => savedTableColumnNames.Any(y => y.Equals(x))))
                {
                    return false;
                }
                List<string> namesOfTableColumnsToAddOrRemove;
                if (actualTableColumnNames.Count >= savedTableColumnNames.Count)
                    namesOfTableColumnsToAddOrRemove = actualTableColumnNames.Except(savedTableColumnNames).ToList();
                else
                    namesOfTableColumnsToAddOrRemove = savedTableColumnNames.Except(actualTableColumnNames).ToList();
                BeginTransaction();
                bool succeeded = false;
                try
                {
                    if (actualTableColumnNames.Count >= savedTableColumnNames.Count)
                    {
                        foreach (string nameOfTableColumnToAdd in namesOfTableColumnsToAddOrRemove)
                        {
                            TableColumn tableColumnToAdd = actualTableColumns[nameOfTableColumnToAdd];
                            if (!AddNewTableColumn(tableColumnToAdd))
                                return false;
                        }
                    }
                    else
                    {
                        foreach (string nameOfTableColumnToRemove in namesOfTableColumnsToAddOrRemove)
                        {
                            if (!RemoveTableColumn(nameOfTableColumnToRemove))
                                return false;
                        }
                    }
                    succeeded = true;
                }
                finally
                {
                    if (succeeded)
                        SetVersion(DATABASE_VERSION);
                    EndTransaction(!succeeded);
                }
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
                return false;
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
        public bool EndTransaction()
        {
            return EndTransaction(false);
        }
        public bool EndTransaction(bool rollback)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            if (currentSqLiteTransaction == null)
                return false;
            try
            {
                if (rollback)
                    currentSqLiteTransaction.Rollback();
                else
                    currentSqLiteTransaction.Commit();
                currentSqLiteTransaction.Dispose();
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
                    programDatabaseFileChecksum = HashingUtilities.GetMD5Hash(
                        File.ReadAllBytes(programDatabaseFilePath)
                    );
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
        private Version GetVersion()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            Version version = new Version();
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       "PRAGMA user_version;",
                       sqLiteConnection
                   ))
            {
                try
                {
                    SQLiteDataReader sqLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        if (sqLiteDataReader.Read())
                            version = GetVersionFromAnInteger(Convert.ToUInt32((long)sqLiteDataReader[0]));
                    }
                    catch { }
                    sqLiteDataReader.Close();
                }
                catch { }
            }
            return version;
        }
        public bool SetVersion(Version version)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"PRAGMA user_version = {GetIntegerFromAVersion(version)};",
                       sqLiteConnection
                   ))
            {
                try
                {
                    sqLiteCommand.ExecuteNonQuery();
                    succeeded = true;
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        private List<string> GetTableColumnNames()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            List<string> tableColumnNames = new List<string>();
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"PRAGMA table_info('{TABLE_NAME__PROGRAMS}');",
                       sqLiteConnection
                   ))
            {
                try
                {
                    SQLiteDataReader sqLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        while (sqLiteDataReader.Read())
                            tableColumnNames.Add((string)sqLiteDataReader["name"]);
                    }
                    catch { }
                    sqLiteDataReader.Close();
                }
                catch { }
            }
            return tableColumnNames;
        }
        public bool AddNewTableColumn(TableColumn tableColumn)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"ALTER TABLE {TABLE_NAME__PROGRAMS} ADD COLUMN {tableColumn.ToSqlString()};",
                       sqLiteConnection
                   ))
            {
                try
                {
                    sqLiteCommand.ExecuteNonQuery();
                    succeeded = true;
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool RemoveTableColumn(string tableColumnName)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"ALTER TABLE {TABLE_NAME__PROGRAMS} DROP COLUMN {tableColumnName};",
                       sqLiteConnection
                   ))
            {
                try
                {
                    sqLiteCommand.ExecuteNonQuery();
                    succeeded = true;
                }
                catch { }
            }
            if (currentSqLiteTransaction == null)
                UpdateProgramDatabaseChecksumFile();
            return succeeded;
        }
        public bool AddNewProgram(Program program)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            StringBuilder sqlQueryString = new StringBuilder(
                INITIAL_CAPACITY_OF_SQL_QUERY_STRING_BUILDER
            );
            sqlQueryString
                .Append($"INSERT INTO {TABLE_NAME__PROGRAMS} (")
                .Append($"{TABLE_COLUMN__NAME.Name}, ")
                .Append($"{TABLE_COLUMN__INSTALLED_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__LATEST_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__INSTALLATION_SCOPE.Name}, ")
                .Append($"{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}, ")
                .Append($"{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name}, ")
                .Append($"{TABLE_COLUMN__WEBPAGE_URL.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name}, ")
                .Append($"{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name}, ")
                .Append($"{TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY.Name}, ")
                .Append($"{TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name}, ")
                .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, ")
                .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}, ")
                .Append($"{TABLE_COLUMN__SKIPPED_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__IS_HIDDEN.Name}, ")
                .Append($"{TABLE_COLUMN__IS_NEW.Name}")
                .Append(") VALUES (")
                .Append($"@{TABLE_COLUMN__NAME.Name}, ")
                .Append($"@{TABLE_COLUMN__INSTALLED_VERSION.Name}, ")
                .Append($"@{TABLE_COLUMN__LATEST_VERSION.Name}, ")
                .Append($"@{TABLE_COLUMN__INSTALLATION_SCOPE.Name}, ")
                .Append($"@{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}, ")
                .Append($"@{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name}, ")
                .Append($"@{TABLE_COLUMN__WEBPAGE_URL.Name}, ")
                .Append($"@{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name}, ")
                .Append($"@{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name}, ")
                .Append($"@{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name}, ")
                .Append($"@{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name}, ")
                .Append($"@{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name}, ")
                .Append($"@{TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY.Name}, ")
                .Append($"@{TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name}, ")
                .Append($"@{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, ")
                .Append($"@{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}, ")
                .Append($"@{TABLE_COLUMN__SKIPPED_VERSION.Name}, ")
                .Append($"@{TABLE_COLUMN__IS_HIDDEN.Name}, ")
                .Append($"@{TABLE_COLUMN__IS_NEW.Name}")
                .Append(");");
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(sqlQueryString.ToString(), sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, program.Name);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__INSTALLED_VERSION.Name, program.InstalledVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__LATEST_VERSION.Name, program.LatestVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__INSTALLATION_SCOPE.Name, (long)program.InstallationScope);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name, Convert.ToInt64(program.IsAutomaticallyAdded));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name, Convert.ToInt64(program.IsUpdateCheckConfigured));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__WEBPAGE_URL.Name, program.WebpageUrl);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__VERSION_SEARCH_METHOD.Name, (long)program.VersionSearchMethod);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name, program.VersionSearchMethodArgument1);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name, program.VersionSearchMethodArgument2);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name, Convert.ToInt64(program.TreatAStandaloneNumberAsAVersion));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name, (long)program.VersionSearchBehavior);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY.Name, (long)program.WebpagePostLoadDelay);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name, JsonSerializer.Serialize(program.LocatingInstructionsOfWebpageElementsToSimulateAClickOn));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name, (long)program.UpdateCheckConfigurationStatus);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name, (long)program.UpdateCheckConfigurationError);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__SKIPPED_VERSION.Name, program.SkippedVersion);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_HIDDEN.Name, Convert.ToInt64(program.IsHidden));
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_NEW.Name, Convert.ToInt64(program.IsNew));
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
        public bool SkipVersionOfProgram(string programName, string skippedVersion)
        {
            return SetSkippedVersionOfProgram(programName, skippedVersion);
        }
        public bool UnskipVersionOfProgram(string programName)
        {
            return SetSkippedVersionOfProgram(programName, "");
        }
        private bool SetSkippedVersionOfProgram(string programName, string skippedVersion)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__SKIPPED_VERSION.Name} = @new_{TABLE_COLUMN__SKIPPED_VERSION.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};",
                       sqLiteConnection
                   ))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__SKIPPED_VERSION.Name}", skippedVersion);
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
        public bool HideProgram(string programName)
        {
            return SetHidingStateOfProgram(programName, true);
        }
        public bool UnhideProgram(string programName)
        {
            return SetHidingStateOfProgram(programName, false);
        }
        private bool SetHidingStateOfProgram(string programName, bool hidden)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__IS_HIDDEN.Name} = @new_{TABLE_COLUMN__IS_HIDDEN.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};",
                       sqLiteConnection
                   ))
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
        public bool MarkProgramAsNonNew(string programName)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__IS_NEW.Name} = @new_{TABLE_COLUMN__IS_NEW.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};",
                       sqLiteConnection
                   ))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_NEW.Name}", 0L);
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
        public bool MarkAllProgramsAsNonNew()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__IS_NEW.Name} = @new_{TABLE_COLUMN__IS_NEW.Name};",
                       sqLiteConnection
                   ))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_NEW.Name}", 0L);
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
        public bool ChangeProgramUpdateCheckConfigurationStatus(string programName,
                                                                Program._UpdateCheckConfigurationStatus updateCheckConfigurationStatus,
                                                                Program._UpdateCheckConfigurationError updateCheckConfigurationError)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, {TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};",
                       sqLiteConnection
                   ))
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
        public bool UpdateProgramInstallationInfoToAutomaticallyDetectedProgram(string programName,
                                                                                string installedVersion,
                                                                                Program._InstallationScope installationScope)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!open)
                throw new DatabaseIsNotOpenException();
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__INSTALLED_VERSION.Name} = @new_{TABLE_COLUMN__INSTALLED_VERSION.Name}, {TABLE_COLUMN__INSTALLATION_SCOPE.Name} = @new_{TABLE_COLUMN__INSTALLATION_SCOPE.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name} AND {TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name} = @{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name};",
                       sqLiteConnection
                   ))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__INSTALLED_VERSION.Name}", installedVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__INSTALLATION_SCOPE.Name}", (long)installationScope);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name, 1L);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__LATEST_VERSION.Name} = @new_{TABLE_COLUMN__LATEST_VERSION.Name} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};",
                       sqLiteConnection
                   ))
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__LATEST_VERSION.Name} = @new_{TABLE_COLUMN__LATEST_VERSION.Name}, {TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, {TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} WHERE {TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name} = @{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name};",
                       sqLiteConnection
                   ))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__LATEST_VERSION.Name}", "");
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}", (long)Program._UpdateCheckConfigurationStatus.Unknown);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}", (long)Program._UpdateCheckConfigurationError.None);
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name, 1L);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"UPDATE {TABLE_NAME__PROGRAMS} SET {TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name} = @new_{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name};",
                       sqLiteConnection
                   ))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}", 0L);
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"DELETE FROM {TABLE_NAME__PROGRAMS} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};",
                       sqLiteConnection
                   ))
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"SELECT * FROM {TABLE_NAME__PROGRAMS} WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};",
                       sqLiteConnection
                   ))
            {
                sqLiteCommand.Parameters.AddWithValue(TABLE_COLUMN__NAME.Name, programName);
                try
                {
                    SQLiteDataReader sqLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        if (sqLiteDataReader.Read())
                        {
                            program = new Program(
                                (string)sqLiteDataReader[TABLE_COLUMN__NAME.Name],
                                (string)sqLiteDataReader[TABLE_COLUMN__INSTALLED_VERSION.Name],
                                (string)sqLiteDataReader[TABLE_COLUMN__LATEST_VERSION.Name],
                                (Program._InstallationScope)((long)sqLiteDataReader[TABLE_COLUMN__INSTALLATION_SCOPE.Name]),
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name]),
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name]),
                                (string)sqLiteDataReader[TABLE_COLUMN__WEBPAGE_URL.Name],
                                (Program._VersionSearchMethod)((long)sqLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD.Name]),
                                (string)sqLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name],
                                (string)sqLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name],
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name]),
                                (Program._VersionSearchBehavior)((long)sqLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name]),
                                (Program._WebpagePostLoadDelay)((long)sqLiteDataReader[TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY.Name]),
                                JsonSerializer.Deserialize<List<WebpageElementLocatingInstruction>>((string)sqLiteDataReader[TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name]),
                                (Program._UpdateCheckConfigurationStatus)((long)sqLiteDataReader[TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name]),
                                (Program._UpdateCheckConfigurationError)((long)sqLiteDataReader[TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name]),
                                (string)sqLiteDataReader[TABLE_COLUMN__SKIPPED_VERSION.Name],
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__IS_HIDDEN.Name]),
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__IS_NEW.Name])
                            );
                        }
                    }
                    catch { }
                    sqLiteDataReader.Close();
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
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(
                       $"SELECT * FROM {TABLE_NAME__PROGRAMS};",
                       sqLiteConnection
                   ))
            {
                try
                {
                    SQLiteDataReader sqLiteDataReader = sqLiteCommand.ExecuteReader();
                    try
                    {
                        while (sqLiteDataReader.Read())
                        {
                            Program program = new Program(
                                (string)sqLiteDataReader[TABLE_COLUMN__NAME.Name],
                                (string)sqLiteDataReader[TABLE_COLUMN__INSTALLED_VERSION.Name],
                                (string)sqLiteDataReader[TABLE_COLUMN__LATEST_VERSION.Name],
                                (Program._InstallationScope)((long)sqLiteDataReader[TABLE_COLUMN__INSTALLATION_SCOPE.Name]),
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name]),
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name]),
                                (string)sqLiteDataReader[TABLE_COLUMN__WEBPAGE_URL.Name],
                                (Program._VersionSearchMethod)((long)sqLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD.Name]),
                                (string)sqLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name],
                                (string)sqLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name],
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name]),
                                (Program._VersionSearchBehavior)((long)sqLiteDataReader[TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name]),
                                (Program._WebpagePostLoadDelay)((long)sqLiteDataReader[TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY.Name]),
                                JsonSerializer.Deserialize<List<WebpageElementLocatingInstruction>>((string)sqLiteDataReader[TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name]),
                                (Program._UpdateCheckConfigurationStatus)((long)sqLiteDataReader[TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name]),
                                (Program._UpdateCheckConfigurationError)((long)sqLiteDataReader[TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name]),
                                (string)sqLiteDataReader[TABLE_COLUMN__SKIPPED_VERSION.Name],
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__IS_HIDDEN.Name]),
                                Convert.ToBoolean((long)sqLiteDataReader[TABLE_COLUMN__IS_NEW.Name])
                            );
                            programs.Add((string)sqLiteDataReader[TABLE_COLUMN__NAME.Name], program);
                        }
                    }
                    catch { }
                    sqLiteDataReader.Close();
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
            StringBuilder sqlQueryString = new StringBuilder(
                INITIAL_CAPACITY_OF_SQL_QUERY_STRING_BUILDER
            );
            sqlQueryString
                .Append($"UPDATE {TABLE_NAME__PROGRAMS} SET ")
                .Append($"{TABLE_COLUMN__NAME.Name} = @new_{TABLE_COLUMN__NAME.Name}, ")
                .Append($"{TABLE_COLUMN__INSTALLED_VERSION.Name} = @new_{TABLE_COLUMN__INSTALLED_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__LATEST_VERSION.Name} = @new_{TABLE_COLUMN__LATEST_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__INSTALLATION_SCOPE.Name} = @new_{TABLE_COLUMN__INSTALLATION_SCOPE.Name}, ")
                .Append($"{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name} = @new_{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}, ")
                .Append($"{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name} = @new_{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name}, ")
                .Append($"{TABLE_COLUMN__WEBPAGE_URL.Name} = @new_{TABLE_COLUMN__WEBPAGE_URL.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name} = @new_{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name} = @new_{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name} = @new_{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name}, ")
                .Append($"{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name} = @new_{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name} = @new_{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name}, ")
                .Append($"{TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY.Name} = @new_{TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY.Name}, ")
                .Append($"{TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name} = @new_{TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name}, ")
                .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}, ")
                .Append($"{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name} = @new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}, ")
                .Append($"{TABLE_COLUMN__SKIPPED_VERSION.Name} = @new_{TABLE_COLUMN__SKIPPED_VERSION.Name}, ")
                .Append($"{TABLE_COLUMN__IS_HIDDEN.Name} = @new_{TABLE_COLUMN__IS_HIDDEN.Name}, ")
                .Append($"{TABLE_COLUMN__IS_NEW.Name} = @new_{TABLE_COLUMN__IS_NEW.Name} ")
                .Append($"WHERE {TABLE_COLUMN__NAME.Name} = @{TABLE_COLUMN__NAME.Name};");
            bool succeeded = false;
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand(sqlQueryString.ToString(), sqLiteConnection))
            {
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__NAME.Name}", newProgram.Name);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__INSTALLED_VERSION.Name}", newProgram.InstalledVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__LATEST_VERSION.Name}", newProgram.LatestVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__INSTALLATION_SCOPE.Name}", (long)newProgram.InstallationScope);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_AUTOMATICALLY_ADDED.Name}", Convert.ToInt64(newProgram.IsAutomaticallyAdded));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_UPDATE_CHECK_CONFIGURED.Name}", Convert.ToInt64(newProgram.IsUpdateCheckConfigured));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__WEBPAGE_URL.Name}", newProgram.WebpageUrl);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__VERSION_SEARCH_METHOD.Name}", (long)newProgram.VersionSearchMethod);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_1.Name}", newProgram.VersionSearchMethodArgument1);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__VERSION_SEARCH_METHOD_ARGUMENT_2.Name}", newProgram.VersionSearchMethodArgument2);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__TREAT_A_STANDALONE_NUMBER_AS_A_VERSION.Name}", Convert.ToInt64(newProgram.TreatAStandaloneNumberAsAVersion));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__VERSION_SEARCH_BEHAVIOR.Name}", (long)newProgram.VersionSearchBehavior);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__WEBPAGE_POST_LOAD_DELAY.Name}", (long)newProgram.WebpagePostLoadDelay);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON.Name}", JsonSerializer.Serialize(newProgram.LocatingInstructionsOfWebpageElementsToSimulateAClickOn));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_STATUS.Name}", (long)newProgram.UpdateCheckConfigurationStatus);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__UPDATE_CHECK_CONFIGURATION_ERROR.Name}", (long)newProgram.UpdateCheckConfigurationError);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__SKIPPED_VERSION.Name}", newProgram.SkippedVersion);
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_HIDDEN.Name}", Convert.ToInt64(newProgram.IsHidden));
                sqLiteCommand.Parameters.AddWithValue($"new_{TABLE_COLUMN__IS_NEW.Name}", Convert.ToInt64(newProgram.IsNew));
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
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Close();
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
