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
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Scrupdate.Classes.Objects;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;

namespace Scrupdate.UiElements.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string ERROR_DIALOG_TITLE__ERROR = "Error";
        private const string ERROR_DIALOG_MESSAGE__PROGRAM_DATABASE_RECREATION_WAS_FAILED = "Program Database Recreation Was Failed!\r\n\r\n•  If this error persists, try to restart your computer or reinstall Scrupdate.";
        private const string ERROR_DIALOG_MESSAGE__NO_CHROMEDRIVER_IS_INSTALLED = "No ChromeDriver Is Installed!";
        private const string ERROR_DIALOG_MESSAGE__UNABLE_TO_ACCESS_THE_CHROMEDRIVER_EXECUTABLE_FILE = "Unable to Access the ChromeDriver Executable File!\r\n\r\n•  If this error persists, try to restart your computer or reinstall Scrupdate.";
        private const string ERROR_DIALOG_MESSAGE__GOOGLE_CHROME_BROWSER_IS_NOT_INSTALLED = "Google Chrome™ Browser Is Not Installed";
        private const string ERROR_DIALOG_MESSAGE__UNABLE_TO_ACCESS_THE_GOOGLE_CHROME_BROWSER_EXECUTABLE_FILE = "Unable to Access the Google Chrome™ Browser Executable File!\r\n\r\n•  If this error persists, try to restart your computer or reinstall Scrupdate.";
        private const string ERROR_DIALOG_MESSAGE__UNABLE_TO_GET_DEFAULT_CHROMEDRIVER_USER_AGENT_STRING = "Unable to Get Default ChromeDriver User-Agent String!\r\n\r\n•  If this error persists, set a custom ChromeDriver user-agent string in the settings\r\n    (In the 'ChromeDriver' tab under the 'ChromeDriver User-Agent String' Field).";
        private const string ERROR_DIALOG_MESSAGE__THE_CHROMEDRIVER_VERSION_IS_NOT_COMPATIBLE_OR_THE_GOOGLE_CHROME_BROWSER_CANNOT_BE_OPENED = "The ChromeDriver's Version Is Not Compatible with the Version of the Installed Google Chrome™ Browser\r\nor the Browser Cannot Be Opened!";
        private const string ERROR_DIALOG_MESSAGE__FAILED_TO_RESET_ONE_OR_MORE_COMPONENTS = "Failed to Reset One or More Components!";
        private const string ERROR_DIALOG_MESSAGE__FAILED_TO_SAVE_SETTINGS = "Failed to Save Settings!";
        private const string QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_CLOSING_SCRUPDATE_FORCEFULLY = "Confirmation for Closing Scrupdate Forcefully";
        private const string QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_RECREATING_THE_PROGRAM_DATABASE = "Confirmation for Recreating the Program Database";
        private const string QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_REMOVING_THE_SELECTED_PROGRAMS_FROM_THE_LIST = "Confirmation for Removing the Selected Program(s) from the List";
        private const string QUESTION_DIALOG_MESSAGE__ARE_YOU_SURE_YOU_WANT_TO_CLOSE_SCRUPDATE_FORCEFULLY = "Are You Sure You Want to Close Scrupdate Forcefully?\r\n\r\n•  If you close Scrupdate forcefully, ChromeDriver will not have a chance to delete its temporary files.";
        private const string QUESTION_DIALOG_MESSAGE__DO_YOU_WANT_TO_RECREATE_THE_PROGRAM_DATABASE = "Do You Want to Recreate the Program Database?\r\n\r\n•  All the program information and configurations will be lost.";
        private const string QUESTION_DIALOG_MESSAGE__REMOVE_THE_SELECTED_PROGRAMS_FROM_THE_LIST = "Remove the Selected Program(s) from the List?";
        private const string STATUS_MESSAGE__SCANNING_INSTALLED_PROGRAMS = "Scanning Installed Programs...";
        private const string STATUS_MESSAGE__CHECKING_FOR_PROGRAM_UPDATES = "Checking for Program Updates...";
        private const string STATUS_MESSAGE__CANCELLING = "Cancelling...";
        private const string STATUS_MESSAGE__CANCELLING_AND_CLOSING = "Cancelling and Closing...";
        private const string STATUS_MESSAGE__NO_UPDATES_WERE_FOUND = "No Updates Were Found";
        private const string STATUS_MESSAGE__THERE_IS_AN_UPDATE = "There Is an Update";
        private const string STATUS_MESSAGE__THERE_ARE_N_UPDATES = "There Are {*} Updates";
        private const string ADDITIONAL_STATUS_MESSAGE__THERE_IS_AN_ERROR = "There Is an Error";
        private const string ADDITIONAL_STATUS_MESSAGE__THERE_ARE_N_ERRORS = "There Are {*} Errors";
        private const string LAST_CHECK_TIME_MESSAGE__LAST_CHECK = "Last Check: {*}";
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
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty CurrentErrorProperty = DependencyProperty.Register(nameof(CurrentError), typeof(Error), typeof(MainWindow), new PropertyMetadata(Error.None));
        public static readonly DependencyProperty IsAutomaticScanningForInstalledProgramsEnabledProperty = DependencyProperty.Register(nameof(IsAutomaticScanningForInstalledProgramsEnabled), typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty CurrentOperationProperty = DependencyProperty.Register(nameof(CurrentOperation), typeof(Operation), typeof(MainWindow), new PropertyMetadata(Operation.None));
        private ProgramDatabase programDatabase;
        private volatile List<ProgramListViewItem> programListViewItems;
        private volatile int updatesCount;
        private volatile int errorsCount;
        private CancellableThread programDatabaseUpdatingCancellableThread;
        private CancellableThread programUpdatesCheckCancellableThread;
        private volatile bool closeInQueue;
        private volatile bool resettingAllSettingsAndData;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSizeOfWindow { get; private set; }
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
        public bool IsAutomaticScanningForInstalledProgramsEnabled
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(Dispatcher, () => (bool)GetValue(IsAutomaticScanningForInstalledProgramsEnabledProperty));
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    SetValue(IsAutomaticScanningForInstalledProgramsEnabledProperty, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAutomaticScanningForInstalledProgramsEnabled)));
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
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public MainWindow() : this(Settings.CachedSettings.ProgramFilteringOption.Unknown, null) { }
        public MainWindow(Settings.CachedSettings.ProgramFilteringOption programFilteringOptionOnStart, bool? isShowingHiddenProgramsOnStart)
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
            programListViewItems = new List<ProgramListViewItem>();
            InitializeComponent();
            BaseSizeOfWindow = new Size(Width, Height);
            WindowsUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(this, BaseSizeOfWindow, App.WindowsRenderingScale);
            ((GridView)listView_programs.View).Columns.CollectionChanged += OnGridViewColumnsCollectionCollectionChangedEvent;
            listView_programs.ItemsSource = programListViewItems;
            label_appVersion.Content = ((string)label_appVersion.Content).Replace("{*}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            foreach (string programFilteringOptionEnumItemName in Enum.GetNames(typeof(Settings.CachedSettings.ProgramFilteringOption)))
                if (!programFilteringOptionEnumItemName.Equals(Settings.CachedSettings.ProgramFilteringOption.Unknown.ToString()))
                    comboBox_programListFilteringOption.Items.Add(StringsUtilities.GetSpaceSeparatedWordsStringFromPascalCasedWordsString(programFilteringOptionEnumItemName));
            comboBox_programListFilteringOption.SelectedItem = StringsUtilities.GetSpaceSeparatedWordsStringFromPascalCasedWordsString(Settings.CachedSettings.ProgramFilteringOption.All.ToString());
            if (App.SettingsHandler.SettingsInMemory.Global.RememberLastProgramListOptions)
            {
                checkBox_filterProgramList.IsChecked = App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringState;
                comboBox_programListFilteringOption.SelectedItem = StringsUtilities.GetSpaceSeparatedWordsStringFromPascalCasedWordsString(App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringOption.ToString());
                checkBox_showHiddenPrograms.IsChecked = App.SettingsHandler.SettingsInMemory.Cached.LastShowHiddenProgramsState;
            }
            if (programFilteringOptionOnStart != Settings.CachedSettings.ProgramFilteringOption.Unknown)
            {
                checkBox_filterProgramList.IsChecked = true;
                comboBox_programListFilteringOption.SelectedItem = StringsUtilities.GetSpaceSeparatedWordsStringFromPascalCasedWordsString(programFilteringOptionOnStart.ToString());
            }
            if (isShowingHiddenProgramsOnStart != null)
                checkBox_showHiddenPrograms.IsChecked = isShowingHiddenProgramsOnStart;
            button_checkForProgramUpdates.Focus();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnWindowLoadedEvent(object sender, RoutedEventArgs e)
        {
            if (App.SettingsHandler.SettingsInMemory.Cached.LastWindowState != null)
                WindowState = (WindowState)App.SettingsHandler.SettingsInMemory.Cached.LastWindowState;
            if (App.SettingsHandler.SettingsInMemory.Cached.LastWindowSize != null)
            {
                Width = ((Size)(App.SettingsHandler.SettingsInMemory.Cached.LastWindowSize)).Width;
                Height = ((Size)(App.SettingsHandler.SettingsInMemory.Cached.LastWindowSize)).Height;
            }
            if (App.SettingsHandler.SettingsInMemory.Cached.LastWindowLocation != null)
            {
                Left = ((Point)(App.SettingsHandler.SettingsInMemory.Cached.LastWindowLocation)).X;
                Top = ((Point)(App.SettingsHandler.SettingsInMemory.Cached.LastWindowLocation)).Y;
            }
            WindowsUtilities.MoveWindowIntoScreenBoundaries(this, true);
            IsAutomaticScanningForInstalledProgramsEnabled = App.SettingsHandler.SettingsInMemory.Global.EnableScanningForInstalledPrograms;
            ((CustomGridViewColumnHeader)((((GridView)listView_programs.View).Columns[2]).Header)).RaiseEvent(new RoutedEventArgs(CustomGridViewColumnHeader.ClickEvent));
            if (CurrentError == Error.None)
            {
                if (App.SettingsHandler.SettingsInMemory.Global.EnableScanningForInstalledPrograms && App.SettingsHandler.SettingsInMemory.Global.ScanForInstalledProgramsAutomaticallyOnStart)
                    StartProgramDatabaseUpdatingTask();
                else
                    RefreshListViewAndAllMessages(true);
            }
        }
        private void OnWindowClosingEvent(object sender, CancelEventArgs e)
        {
            if (!resettingAllSettingsAndData)
            {
                if (CurrentOperation == Operation.CancellingOperation || CurrentOperation == Operation.UpdatingProgramDatabase || CurrentOperation == Operation.CheckingForProgramUpdates)
                {
                    e.Cancel = true;
                    if ((CurrentOperation == Operation.CancellingOperation && !closeInQueue) || CurrentOperation == Operation.UpdatingProgramDatabase || CurrentOperation == Operation.CheckingForProgramUpdates)
                        CancelOperation(true);
                    else if (CurrentOperation == Operation.CancellingOperation && closeInQueue)
                        if (DialogsUtilities.ShowQuestionDialog(QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_CLOSING_SCRUPDATE_FORCEFULLY, QUESTION_DIALOG_MESSAGE__ARE_YOU_SURE_YOU_WANT_TO_CLOSE_SCRUPDATE_FORCEFULLY, this) == true)
                            PrepareWindowForClosing(true);
                }
                else
                    PrepareWindowForClosing();
            }
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_hideSelectedPrograms)
                HideSelectedProgramsInDatabaseAndListView();
            else if (senderButton == button_unhideSelectedPrograms)
                UnhideSelectedProgramsInDatabaseAndListView();
            else if (senderButton == button_removeSelectedPrograms)
            {
                if (DialogsUtilities.ShowQuestionDialog(QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_REMOVING_THE_SELECTED_PROGRAMS_FROM_THE_LIST, QUESTION_DIALOG_MESSAGE__REMOVE_THE_SELECTED_PROGRAMS_FROM_THE_LIST, this) == true)
                    RemoveSelectedProgramsFromDatabaseAndListView();
            }
            else if (senderButton == button_addNewProgram)
            {
                Program newProgram;
                if (OpenProgramAdditionOrEditionWindowAsDialogForAddingAProgram(out newProgram) == true)
                    AddNewProgramToDatabaseAndListView(newProgram);
            }
            else if (senderButton == button_checkForProgramUpdates)
                StartProgramUpdatesCheckTask();
            else if (senderButton == button_rescanForInstalledPrograms)
                StartProgramDatabaseUpdatingTask();
            else if (senderButton == button_cancel)
                CancelOperation(false);
            else if (senderButton == button_about)
                OpenAboutWindowAsDialog();
            else if (senderButton == button_help)
                OpenSingleHelpWindow();
            else if (senderButton == button_settings)
            {
                Settings updatedSettings;
                if (OpenSettingsWindowAsDialog(out updatedSettings) == true)
                {
                    if (updatedSettings != null)
                        ApplyUpdatedSettings(updatedSettings);
                    else
                    {
                        resettingAllSettingsAndData = true;
                        programDatabase?.Close();
                        App.SettingsHandler?.Dispose();
                        App.SettingsHandler = null;
                        if (!ApplicationUtilities.ResetAll())
                            DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__FAILED_TO_RESET_ONE_OR_MORE_COMPONENTS, this);
                        Application.Current.Shutdown();
                    }
                }
            }
        }
        private void OnTextBoxTextChangedEvent(object sender, TextChangedEventArgs e)
        {
            CustomTextBox senderTextBox = (CustomTextBox)sender;
            if (senderTextBox == textBox_programListSearchingPhrase)
            {
                if (Array.TrueForAll(textBox_programListSearchingPhrase.Text.ToCharArray(), (char programListSearchingPhraseTextBoxTextCharacter) => (char.IsWhiteSpace(programListSearchingPhraseTextBoxTextCharacter))))
                    textBox_programListSearchingPhrase.Text = "";
                RefreshListViewAndAllMessages();
            }
        }
        private void OnCheckBoxClickEvent(object sender, RoutedEventArgs e)
        {
            CustomCheckBox senderCheckBox = (CustomCheckBox)sender;
            if (senderCheckBox == checkBox_filterProgramList)
            {
                if (checkBox_filterProgramList.IsChecked == false)
                    comboBox_programListFilteringOption.SelectedItem = StringsUtilities.GetSpaceSeparatedWordsStringFromPascalCasedWordsString(Settings.CachedSettings.ProgramFilteringOption.All.ToString());
                RefreshListViewAndAllMessages();
            }
            else if (senderCheckBox == checkBox_showHiddenPrograms)
                RefreshListViewAndAllMessages();
        }
        private void OnComboBoxSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            CustomComboBox senderComboBox = (CustomComboBox)sender;
            if (senderComboBox == comboBox_programListFilteringOption)
                RefreshListViewAndAllMessages();
        }
        private void OnListViewSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            ListView senderListView = (ListView)sender;
            if (senderListView == listView_programs)
                ((CheckBox)(gridViewColumnHeader_programSelectionCheckBox.Content)).IsChecked = (listView_programs.Items.Count != 0 && (listView_programs.SelectedItems.Count == listView_programs.Items.Count));
        }
        private void OnListViewKeyDownEvent(object sender, KeyEventArgs e)
        {
            ListView senderListView = (ListView)sender;
            if (senderListView == listView_programs)
            {
                if (e.Key == Key.Enter)
                {
                    if (listView_programs.SelectedItems.Count == 1)
                    {
                        ProgramListViewItem programListViewItemOfProgramToEdit = (ProgramListViewItem)listView_programs.SelectedItems[0];
                        Program updatedProgram;
                        if (OpenProgramAdditionOrEditionWindowAsDialogForEditingAProgram(programListViewItemOfProgramToEdit.UnderlyingProgram.Name, out updatedProgram) == true)
                            UpdateProgramInDatabaseAndListView(programListViewItemOfProgramToEdit, updatedProgram);
                    }
                }
                else if (e.Key == Key.Delete)
                {
                    if (listView_programs.SelectedItems.Count > 0)
                        if (DialogsUtilities.ShowQuestionDialog(QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_REMOVING_THE_SELECTED_PROGRAMS_FROM_THE_LIST, QUESTION_DIALOG_MESSAGE__REMOVE_THE_SELECTED_PROGRAMS_FROM_THE_LIST, this) == true)
                            RemoveSelectedProgramsFromDatabaseAndListView();
                }
            }
        }
        private void OnGridViewColumnHeaderPreviewMouseMoveEvent(object sender, MouseEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_programSelectionCheckBox || senderGridViewColumnHeader == gridViewColumnHeader_programUpdateCheckConfigurationStatus)
                e.Handled = true;
        }
        private void OnGridViewColumnHeaderPreviewMouseDoubleClickEvent(object sender, MouseButtonEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_programSelectionCheckBox || senderGridViewColumnHeader == gridViewColumnHeader_programUpdateCheckConfigurationStatus)
                e.Handled = true;
        }
        private void OnGridViewColumnHeaderClickEvent(object sender, RoutedEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_programSelectionCheckBox)
            {
                if (listView_programs.SelectedItems.Count != listView_programs.Items.Count)
                    listView_programs.SelectAll();
                else
                    listView_programs.UnselectAll();
            }
            else
            {
                if (senderGridViewColumnHeader.ListViewItemsSortingOrder == CustomGridViewColumnHeader.SortingOrder.Ascending)
                    senderGridViewColumnHeader.ListViewItemsSortingOrder = CustomGridViewColumnHeader.SortingOrder.Descending;
                else if (senderGridViewColumnHeader.ListViewItemsSortingOrder == CustomGridViewColumnHeader.SortingOrder.Descending)
                    senderGridViewColumnHeader.ListViewItemsSortingOrder = CustomGridViewColumnHeader.SortingOrder.Ascending;
                else
                {
                    foreach (GridViewColumn gridViewColumn in ((GridView)listView_programs.View).Columns)
                        ((CustomGridViewColumnHeader)gridViewColumn.Header).ListViewItemsSortingOrder = CustomGridViewColumnHeader.SortingOrder.None;
                    senderGridViewColumnHeader.ListViewItemsSortingOrder = CustomGridViewColumnHeader.SortingOrder.Ascending;
                }
                listView_programs.Items.SortDescriptions.Clear();
                listView_programs.Items.SortDescriptions.Add(new SortDescription((string)senderGridViewColumnHeader.Tag, (senderGridViewColumnHeader.ListViewItemsSortingOrder == CustomGridViewColumnHeader.SortingOrder.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending)));
                RefreshListViewAndAllMessages();
            }
        }
        private void OnListViewItemMouseDoubleClickEvent(object sender, MouseButtonEventArgs e)
        {
            ListViewItem senderListViewItem = (ListViewItem)sender;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (listView_programs.SelectedItems.Count == 1)
                {
                    ProgramListViewItem programListViewItemOfProgramToEdit = (ProgramListViewItem)senderListViewItem.Content;
                    if (programListViewItemOfProgramToEdit == listView_programs.SelectedItems[0])
                    {
                        Program updatedProgram;
                        if (OpenProgramAdditionOrEditionWindowAsDialogForEditingAProgram(programListViewItemOfProgramToEdit.UnderlyingProgram.Name, out updatedProgram) == true)
                            UpdateProgramInDatabaseAndListView(programListViewItemOfProgramToEdit, updatedProgram);
                    }
                }
            }
        }
        private void OnMenuItemClickEvent(object sender, RoutedEventArgs e)
        {
            MenuItem senderMenuItem = (MenuItem)sender;
            if (senderMenuItem.Header.Equals("Edit"))
            {
                ProgramListViewItem programListViewItemOfProgramToEdit = (ProgramListViewItem)listView_programs.SelectedItems[0];
                Program updatedProgram;
                if (OpenProgramAdditionOrEditionWindowAsDialogForEditingAProgram(programListViewItemOfProgramToEdit.UnderlyingProgram.Name, out updatedProgram) == true)
                    UpdateProgramInDatabaseAndListView(programListViewItemOfProgramToEdit, updatedProgram);
            }
            else if (senderMenuItem.Header.Equals("Hide") || senderMenuItem.Header.Equals("Hide Selected"))
                HideSelectedProgramsInDatabaseAndListView();
            else if (senderMenuItem.Header.Equals("Unhide") || senderMenuItem.Header.Equals("Unhide Selected"))
                UnhideSelectedProgramsInDatabaseAndListView();
            else if (senderMenuItem.Header.Equals("Remove") || senderMenuItem.Header.Equals("Remove Selected"))
            {
                if (DialogsUtilities.ShowQuestionDialog(QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_REMOVING_THE_SELECTED_PROGRAMS_FROM_THE_LIST, QUESTION_DIALOG_MESSAGE__REMOVE_THE_SELECTED_PROGRAMS_FROM_THE_LIST, this) == true)
                    RemoveSelectedProgramsFromDatabaseAndListView();
            }
        }
        private void OnHyperlinkClickEvent(object sender, RoutedEventArgs e)
        {
            Hyperlink senderHyperlink = (Hyperlink)sender;
            if (senderHyperlink == hyperlink_fixProgramDatabaseCannotBeOpened || senderHyperlink == hyperlink_fixProgramDatabaseCorruption)
            {
                if (senderHyperlink == hyperlink_fixProgramDatabaseCorruption)
                {
                    if (!DialogsUtilities.ShowQuestionDialog(QUESTION_DIALOG_TITLE__CONFIRMATION_FOR_RECREATING_THE_PROGRAM_DATABASE, QUESTION_DIALOG_MESSAGE__DO_YOU_WANT_TO_RECREATE_THE_PROGRAM_DATABASE, this) == true)
                        return;
                    else
                    {
                        if (!ApplicationUtilities.ResetProgramDatabase(App.SettingsHandler))
                        {
                            DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__PROGRAM_DATABASE_RECREATION_WAS_FAILED, this);
                            return;
                        }
                    }
                }
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
                    {
                        CurrentError = Error.CanNotOpenProgramDatabase;
                        if (programDatabaseFileIsCorrupted)
                            CurrentError = Error.ProgramDatabaseIsCorrupted;
                        return;
                    }
                    else if (programDatabaseFileIsCorrupted)
                    {
                        CurrentError = Error.ProgramDatabaseIsCorrupted;
                        return;
                    }
                }
                catch
                {
                    CurrentError = Error.CanNotOpenProgramDatabase;
                    return;
                }
                CurrentError = Error.None;
                if (App.SettingsHandler.SettingsInMemory.Global.EnableScanningForInstalledPrograms && App.SettingsHandler.SettingsInMemory.Global.ScanForInstalledProgramsAutomaticallyOnStart)
                    StartProgramDatabaseUpdatingTask();
                else
                    RefreshListViewAndAllMessages(true);
            }
        }
        private void OnGridViewColumnsCollectionCollectionChangedEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            GridViewColumnCollection senderGridViewColumnCollection = (GridViewColumnCollection)sender;
            senderGridViewColumnCollection.CollectionChanged -= OnGridViewColumnsCollectionCollectionChangedEvent;
            senderGridViewColumnCollection.Remove(gridViewColumn_programSelectionCheckBox);
            senderGridViewColumnCollection.Insert(0, gridViewColumn_programSelectionCheckBox);
            senderGridViewColumnCollection.Remove(gridViewColumn_programUpdateCheckConfigurationStatus);
            senderGridViewColumnCollection.Insert(1, gridViewColumn_programUpdateCheckConfigurationStatus);
            senderGridViewColumnCollection.CollectionChanged += OnGridViewColumnsCollectionCollectionChangedEvent;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool? OpenSettingsWindowAsDialog(out Settings updatedSettings)
        {
            updatedSettings = null;
            bool? returnValue = null;
            SettingsWindow settingsWindow = null;
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                settingsWindow = new SettingsWindow(App.SettingsHandler.SettingsInMemory, (programDatabase == null ? false : programDatabase.IsOpen()));
                settingsWindow.Owner = this;
                returnValue = settingsWindow.ShowDialog();
            });
            if (returnValue == true)
                updatedSettings = settingsWindow.GetUpdatedSettings();
            return returnValue;
        }
        private void OpenSingleHelpWindow()
        {
            bool helpWindowIsAlreadyOpen = false;
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType().Equals(typeof(HelpWindow)))
                {
                    window.WindowState = WindowState.Normal;
                    window.Activate();
                    helpWindowIsAlreadyOpen = true;
                    break;
                }
            }
            if (!helpWindowIsAlreadyOpen)
            {
                ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
                {
                    HelpWindow helpWindow = new HelpWindow();
                    helpWindow.Owner = this;
                    helpWindow.Show();
                });
            }
        }
        private bool? OpenAboutWindowAsDialog()
        {
            bool? returnValue = null;
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                AboutWindow aboutWindow = new AboutWindow();
                aboutWindow.Owner = this;
                returnValue = aboutWindow.ShowDialog();
            });
            return returnValue;
        }
        private bool? OpenProgramAdditionOrEditionWindowAsDialogForAddingAProgram(out Program newProgram)
        {
            newProgram = null;
            bool? returnValue = null;
            ProgramAdditionOrEditionWindow programAdditionOrEditionWindow = null;
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                Dictionary<string, Program> programsAlreadyInDatabase = programDatabase.GetPrograms();
                programAdditionOrEditionWindow = new ProgramAdditionOrEditionWindow(programsAlreadyInDatabase);
                programAdditionOrEditionWindow.Owner = this;
                returnValue = programAdditionOrEditionWindow.ShowDialog();
            });
            if (returnValue == true)
                newProgram = programAdditionOrEditionWindow.GetNewOrUpdatedProgram();
            return returnValue;
        }
        private bool? OpenProgramAdditionOrEditionWindowAsDialogForEditingAProgram(string nameOfProgramToEdit, out Program updatedProgram)
        {
            updatedProgram = null;
            bool? returnValue = null;
            ProgramAdditionOrEditionWindow programAdditionOrEditionWindow = null;
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                Dictionary<string, Program> programsAlreadyInDatabase = programDatabase.GetPrograms();
                Program programToEdit = programsAlreadyInDatabase[nameOfProgramToEdit];
                programAdditionOrEditionWindow = new ProgramAdditionOrEditionWindow(programsAlreadyInDatabase, programToEdit);
                programAdditionOrEditionWindow.Owner = this;
                returnValue = programAdditionOrEditionWindow.ShowDialog();
            });
            if (returnValue == true)
                updatedProgram = programAdditionOrEditionWindow.GetNewOrUpdatedProgram();
            return returnValue;
        }
        private void RefreshListViewAndAllMessages()
        {
            RefreshListViewAndAllMessages(false);
        }
        private void RefreshListViewAndAllMessages(bool retrieveNewProgramInformationFromDatabase)
        {
            if (retrieveNewProgramInformationFromDatabase)
            {
                programListViewItems.Clear();
                for (Dictionary<string, Program>.Enumerator i = programDatabase.GetPrograms().GetEnumerator(); i.MoveNext();)
                {
                    Program program = i.Current.Value;
                    programListViewItems.Add(new ProgramListViewItem(program));
                }
                programListViewItems.Sort(((ProgramListViewItem programListViewItem1, ProgramListViewItem programListViewItem2) => (string.Compare(programListViewItem1.UnderlyingProgram.Name, programListViewItem2.UnderlyingProgram.Name))));
            }
            updatesCount = 0;
            errorsCount = 0;
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                string programSearchPhrase = textBox_programListSearchingPhrase.Text.Trim();
                bool isFilteringPrograms = (bool)checkBox_filterProgramList.IsChecked;
                Settings.CachedSettings.ProgramFilteringOption selectedProgramFilteringOption = (!isFilteringPrograms ? Settings.CachedSettings.ProgramFilteringOption.All : (Settings.CachedSettings.ProgramFilteringOption)Enum.Parse(typeof(Settings.CachedSettings.ProgramFilteringOption), ((string)comboBox_programListFilteringOption.SelectedItem).Replace(" ", "")));
                bool isShowingHiddenPrograms = (bool)checkBox_showHiddenPrograms.IsChecked;
                listView_programs.Items.Filter = new Predicate<object>((object o) =>
                {
                    ProgramListViewItem programListViewItem = (ProgramListViewItem)o;
                    Program program = programListViewItem.UnderlyingProgram;
                    programListViewItem.ProgramInstalledVersionToDisplay = (program.InstalledVersion.Equals("") ? "" : VersionsUtilities.TrimVersion(program.InstalledVersion, App.SettingsHandler.SettingsInMemory.Appearance.MinimumVersionSegments, App.SettingsHandler.SettingsInMemory.Appearance.MaximumVersionSegments, App.SettingsHandler.SettingsInMemory.Appearance.RemoveTrailingZeroSegmentsOfVersions));
                    programListViewItem.ProgramLatestVersionToDisplay = (program.LatestVersion.Equals("") ? "" : VersionsUtilities.TrimVersion(program.LatestVersion, App.SettingsHandler.SettingsInMemory.Appearance.MinimumVersionSegments, App.SettingsHandler.SettingsInMemory.Appearance.MaximumVersionSegments, App.SettingsHandler.SettingsInMemory.Appearance.RemoveTrailingZeroSegmentsOfVersions));
                    bool? thereIsANewerVersion = null;
                    if (program.InstallationScope != Program.ProgramInstallationScope.None)
                        thereIsANewerVersion = (program.InstalledVersion.Equals("") || program.LatestVersion.Equals("") ? false : VersionsUtilities.IsVersionNewer(program.LatestVersion, program.InstalledVersion));
                    programListViewItem.Foreground = (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH);
                    if (!program.IsAutomaticallyAdded)
                        programListViewItem.Foreground = (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__GRAY_SOLID_COLOR_BRUSH);
                    if (thereIsANewerVersion == true)
                    {
                        if (!program.IsHidden || (program.IsHidden && isShowingHiddenPrograms))
                            updatesCount++;
                        programListViewItem.Foreground = (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH);
                    }
                    if (program.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Invalid)
                        if (!program.IsHidden || (program.IsHidden && isShowingHiddenPrograms))
                            errorsCount++;
                    if (program.InstallationScope == Program.ProgramInstallationScope.None)
                        programListViewItem.Foreground = (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH);
                    if (program.IsHidden)
                        programListViewItem.Foreground = (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__LIGHT_GRAY_SOLID_COLOR_BRUSH_2);
                    if (isFilteringPrograms)
                    {
                        switch (selectedProgramFilteringOption)
                        {
                            default:
                            case Settings.CachedSettings.ProgramFilteringOption.All:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)));
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyUpdates:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && program.IsUpdateCheckConfigured && program.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Valid && thereIsANewerVersion == true);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyUpToDate:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && program.IsUpdateCheckConfigured && program.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Valid && thereIsANewerVersion == false);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyAutomaticallyAdded:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && program.IsAutomaticallyAdded);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyManuallyAdded:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && !program.IsAutomaticallyAdded);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyInstalled:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && program.InstallationScope != Program.ProgramInstallationScope.None);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyUninstalled:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && program.InstallationScope == Program.ProgramInstallationScope.None);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyValid:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && program.IsUpdateCheckConfigured && program.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Valid);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyInvalid:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && program.IsUpdateCheckConfigured && program.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Invalid);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyNotChecked:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && program.IsUpdateCheckConfigured && program.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Unknown);
                            case Settings.CachedSettings.ProgramFilteringOption.OnlyNotConfigured:
                                return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)) && !program.IsUpdateCheckConfigured);
                        }
                    }
                    return (!(program.IsHidden && !isShowingHiddenPrograms) && (programSearchPhrase.Equals("") || program.Name.Contains(programSearchPhrase, StringComparison.CurrentCultureIgnoreCase)));
                });
                ((CheckBox)(gridViewColumnHeader_programSelectionCheckBox.Content)).IsChecked = (listView_programs.Items.Count != 0 && (listView_programs.SelectedItems.Count == listView_programs.Items.Count));
            });
            string statusMessage = (updatesCount > 0 ? (updatesCount > 1 ? STATUS_MESSAGE__THERE_ARE_N_UPDATES.Replace("{*}", Convert.ToString(updatesCount)) : STATUS_MESSAGE__THERE_IS_AN_UPDATE) : STATUS_MESSAGE__NO_UPDATES_WERE_FOUND);
            Brush statusMessageForegroundColor = (updatesCount > 0 ? (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH) : (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__LIGHT_BLACK_SOLID_COLOR_BRUSH));
            string additionalStatusMessage = (errorsCount > 0 ? (errorsCount > 1 ? ADDITIONAL_STATUS_MESSAGE__THERE_ARE_N_ERRORS.Replace("{*}", Convert.ToString(errorsCount)) : ADDITIONAL_STATUS_MESSAGE__THERE_IS_AN_ERROR) : "");
            Brush additionalStatusMessageForegroundColor = (errorsCount > 0 ? (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH) : (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
            ChangeStatusMessages(statusMessage, statusMessageForegroundColor, additionalStatusMessage, additionalStatusMessageForegroundColor);
            ChangeTimeToShowInLastProgramUpdatesCheckTimeMessage(App.SettingsHandler.SettingsInMemory.Cached.LastProgramUpdatesCheckTime);
        }
        private void AddNewProgramToDatabaseAndListView(Program newProgram)
        {
            App.SettingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = "";
            App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
            programDatabase.AddNewProgram(newProgram);
            programListViewItems.Add(new ProgramListViewItem(newProgram));
            programListViewItems.Sort(((ProgramListViewItem programListViewItem1, ProgramListViewItem programListViewItem2) => (string.Compare(programListViewItem1.UnderlyingProgram.Name, programListViewItem2.UnderlyingProgram.Name))));
            RefreshListViewAndAllMessages();
        }
        private void UpdateProgramInDatabaseAndListView(ProgramListViewItem selectedProgramListViewItem, Program updatedProgram)
        {
            Program selectedProgram = selectedProgramListViewItem.UnderlyingProgram;
            if (!selectedProgram.Name.Equals(updatedProgram.Name))
            {
                App.SettingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = "";
                App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
            }
            programDatabase.UpdateProgram(selectedProgram.Name, updatedProgram);
            programListViewItems.Remove(selectedProgramListViewItem);
            programListViewItems.Add(new ProgramListViewItem(updatedProgram));
            programListViewItems.Sort(((ProgramListViewItem programListViewItem1, ProgramListViewItem programListViewItem2) => (string.Compare(programListViewItem1.UnderlyingProgram.Name, programListViewItem2.UnderlyingProgram.Name))));
            bool? selectedProgramHasAnUpdate = false;
            if (selectedProgram.InstallationScope != Program.ProgramInstallationScope.None)
                selectedProgramHasAnUpdate = (selectedProgram.LatestVersion.Equals("") ? false : (selectedProgram.InstalledVersion.Equals("") ? true : VersionsUtilities.IsVersionNewer(selectedProgram.LatestVersion, selectedProgram.InstalledVersion)));
            bool? updatedProgramHasAnUpdate = false;
            if (updatedProgram.InstallationScope != Program.ProgramInstallationScope.None)
                updatedProgramHasAnUpdate = (updatedProgram.LatestVersion.Equals("") ? false : (updatedProgram.InstalledVersion.Equals("") ? true : VersionsUtilities.IsVersionNewer(updatedProgram.LatestVersion, updatedProgram.InstalledVersion)));
            if (selectedProgramHasAnUpdate != true && updatedProgramHasAnUpdate == true)
                updatesCount++;
            else if (selectedProgramHasAnUpdate == true && updatedProgramHasAnUpdate != true)
                updatesCount--;
            if (selectedProgram.UpdateCheckConfigurationStatus != Program.ProgramUpdateCheckConfigurationStatus.Invalid && updatedProgram.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Invalid)
                errorsCount++;
            else if (selectedProgram.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Invalid && updatedProgram.UpdateCheckConfigurationStatus != Program.ProgramUpdateCheckConfigurationStatus.Invalid)
                errorsCount--;
            RefreshListViewAndAllMessages();
        }
        private void HideSelectedProgramsInDatabaseAndListView()
        {
            if (listView_programs.SelectedItems.Count > 0)
            {
                programDatabase.BeginTransaction();
                foreach (ProgramListViewItem selectedProgramListViewItem in listView_programs.SelectedItems)
                {
                    Program selectedProgram = selectedProgramListViewItem.UnderlyingProgram;
                    programDatabase.HideProgram(selectedProgram.Name);
                    selectedProgram.IsHidden = true;
                }
                programDatabase.CommitTransaction();
                RefreshListViewAndAllMessages();
            }
        }
        private void UnhideSelectedProgramsInDatabaseAndListView()
        {
            if (listView_programs.SelectedItems.Count > 0)
            {
                programDatabase.BeginTransaction();
                foreach (ProgramListViewItem selectedProgramListViewItem in listView_programs.SelectedItems)
                {
                    Program selectedProgram = selectedProgramListViewItem.UnderlyingProgram;
                    programDatabase.UnhideProgram(selectedProgram.Name);
                    selectedProgram.IsHidden = false;
                }
                programDatabase.CommitTransaction();
                RefreshListViewAndAllMessages();
            }
        }
        private void RemoveSelectedProgramsFromDatabaseAndListView()
        {
            if (listView_programs.SelectedItems.Count > 0)
            {
                App.SettingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = "";
                App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
                programDatabase.BeginTransaction();
                foreach (ProgramListViewItem selectedProgramListViewItem in listView_programs.SelectedItems)
                {
                    Program selectedProgram = selectedProgramListViewItem.UnderlyingProgram;
                    programListViewItems.Remove(selectedProgramListViewItem);
                    programDatabase.RemoveProgram(selectedProgram.Name);
                    bool? selectedProgramHasAnUpdate = false;
                    if (selectedProgram.InstallationScope != Program.ProgramInstallationScope.None)
                        selectedProgramHasAnUpdate = (selectedProgram.LatestVersion.Equals("") ? false : (selectedProgram.InstalledVersion.Equals("") ? true : VersionsUtilities.IsVersionNewer(selectedProgram.LatestVersion, selectedProgram.InstalledVersion)));
                    if (selectedProgramHasAnUpdate == true)
                        updatesCount--;
                    if (selectedProgram.UpdateCheckConfigurationStatus == Program.ProgramUpdateCheckConfigurationStatus.Invalid)
                        errorsCount--;
                }
                programDatabase.CommitTransaction();
                RefreshListViewAndAllMessages();
            }
        }
        private void ChangeStatusMessages(string statusMessage, Brush statusMessageForegroundColor)
        {
            ChangeStatusMessages(statusMessage, statusMessageForegroundColor, "", (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH));
        }
        private void ChangeStatusMessages(string statusMessage, Brush statusMessageForegroundColor, string additionalStatusMessage, Brush additionalStatusMessageForegroundColor)
        {
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                label_statusMessage.Content = statusMessage;
                label_statusMessage.Foreground = statusMessageForegroundColor;
                label_additionalStatusMessage.Content = additionalStatusMessage;
                label_additionalStatusMessage.Foreground = additionalStatusMessageForegroundColor;
            });
        }
        private void ChangeTimeToShowInLastProgramUpdatesCheckTimeMessage(DateTime timeToShowInLastProgramUpdatesCheckTimeMessage)
        {
            ThreadsUtilities.RunOnAnotherThread(Dispatcher, () =>
            {
                string lastCheckTimeString = timeToShowInLastProgramUpdatesCheckTimeMessage.ToString();
                label_lastProgramUpdatesCheckTimeMessage.Content = LAST_CHECK_TIME_MESSAGE__LAST_CHECK.Replace("{*}", (!lastCheckTimeString.Equals((new DateTime()).ToString()) ? lastCheckTimeString : "Never"));
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
                }
                else
                {
                    progressBar_progress.IsIndeterminate = false;
                    progressBar_progress.ChangeValueSmoothly(progressBarValue, new Duration(new TimeSpan(0, 0, 0, 1)));
                }
            });
        }
        private void ApplyUpdatedSettings(Settings updatedSettings)
        {
            Settings backupOfSettingsInMemory = App.SettingsHandler.SettingsInMemory;
            App.SettingsHandler.SettingsInMemory = updatedSettings;
            if (!App.SettingsHandler.SettingsInMemory.Global.RememberLastProgramListOptions)
            {
                App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringState = false;
                App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringOption = Settings.CachedSettings.ProgramFilteringOption.All;
                App.SettingsHandler.SettingsInMemory.Cached.LastShowHiddenProgramsState = false;
            }
            if (!App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile())
            {
                App.SettingsHandler.SettingsInMemory = backupOfSettingsInMemory;
                DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__FAILED_TO_SAVE_SETTINGS, this);
            }
            else
            {
                if (App.SettingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor != backupOfSettingsInMemory.Appearance.WindowsScalingFactor)
                {
                    double newWindowsRenderingScale = WindowsUtilities.GetWindowsRenderingScale(App.SettingsHandler);
                    ApplicationUtilities.ChangeRenderingScaleOfAllOpenWindowsAndMoveThemIntoScreenBoundaries(newWindowsRenderingScale);
                }
                IsAutomaticScanningForInstalledProgramsEnabled = App.SettingsHandler.SettingsInMemory.Global.EnableScanningForInstalledPrograms;
                if (CurrentError == Error.None)
                {
                    if (!App.SettingsHandler.SettingsInMemory.Global.EnableScanningForInstalledPrograms)
                        programDatabase.ConvertAllProgramsToManuallyInstalledPrograms();
                    RefreshListViewAndAllMessages(true);
                }
            }
        }
        private void StartProgramDatabaseUpdatingTask()
        {
            CurrentOperation = Operation.UpdatingProgramDatabase;
            programDatabaseUpdatingCancellableThread = new CancellableThread((CancellationToken cancellationToken) =>
            {
                Dispatcher.Invoke(() => listView_programs.SelectedItems.Clear());
                ChangeStatusMessages(STATUS_MESSAGE__SCANNING_INSTALLED_PROGRAMS, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                ChangeProgressBarValue(-1);
                if (!closeInQueue && !cancellationToken.IsCancellationRequested)
                    ProgramsScanAndUpdatesCheckUtilities.ScanForInstalledProgramsAndUpdateProgramDatabase(programDatabase, App.SettingsHandler, cancellationToken);
                RefreshListViewAndAllMessages(true);
                CurrentOperation = Operation.None;
                ChangeProgressBarValue(-1);
                if (closeInQueue)
                    PrepareWindowForClosing(true);
                programDatabaseUpdatingCancellableThread = null;
            });
            programDatabaseUpdatingCancellableThread.Start();
        }
        private void StartProgramUpdatesCheckTask()
        {
            CurrentOperation = Operation.CheckingForProgramUpdates;
            programUpdatesCheckCancellableThread = new CancellableThread((CancellationToken cancellationToken) =>
            {
                Dispatcher.Invoke(() => listView_programs.SelectedItems.Clear());
                Exception programUpdatesCheckException = null;
                ChangeStatusMessages(STATUS_MESSAGE__CHECKING_FOR_PROGRAM_UPDATES, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                ChangeProgressBarValue(-1);
                if (!closeInQueue && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        List<Program> programsToCheck = new List<Program>();
                        foreach (ProgramListViewItem programListViewItem in programListViewItems)
                            if (programListViewItem.UnderlyingProgram.IsUpdateCheckConfigured)
                                programsToCheck.Add(programListViewItem.UnderlyingProgram);
                        ProgramsScanAndUpdatesCheckUtilities.CheckForProgramUpdatesAndUpdateDatabase(programsToCheck, programDatabase, App.SettingsHandler, ChangeProgressBarValue, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        programUpdatesCheckException = e;
                    }
                }
                RefreshListViewAndAllMessages(true);
                CurrentOperation = Operation.None;
                ChangeProgressBarValue(-1);
                if (closeInQueue)
                    PrepareWindowForClosing(true);
                if (programUpdatesCheckException != null)
                {
                    if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.NoChromeDriverIsInstalledException)))
                        DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__NO_CHROMEDRIVER_IS_INSTALLED, this);
                    else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.UnableToAccessChromeDriverExecutableFileException)))
                        DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__UNABLE_TO_ACCESS_THE_CHROMEDRIVER_EXECUTABLE_FILE, this);
                    else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.GoogleChromeBrowserIsNotInstalledException)))
                        DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__GOOGLE_CHROME_BROWSER_IS_NOT_INSTALLED, this);
                    else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.UnableToAccessGoogleChromeBrowserExecutableFileException)))
                        DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__UNABLE_TO_ACCESS_THE_GOOGLE_CHROME_BROWSER_EXECUTABLE_FILE, this);
                    else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.UnableToGetDefaultChromeDriverUserAgentStringException)))
                        DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__UNABLE_TO_GET_DEFAULT_CHROMEDRIVER_USER_AGENT_STRING, this);
                    else if (programUpdatesCheckException.GetType().Equals(typeof(ProgramsScanAndUpdatesCheckUtilities.ChromeDriverIsNotCompatibleOrGoogleChromeBrowserCannotBeOpenedException)))
                        DialogsUtilities.ShowErrorDialog(ERROR_DIALOG_TITLE__ERROR, ERROR_DIALOG_MESSAGE__THE_CHROMEDRIVER_VERSION_IS_NOT_COMPATIBLE_OR_THE_GOOGLE_CHROME_BROWSER_CANNOT_BE_OPENED, this);
                }
                programUpdatesCheckCancellableThread = null;
            });
            programUpdatesCheckCancellableThread.Start();
        }
        private void CancelOperation(bool queueWindowToBeClosedAfterCancelling)
        {
            if (CurrentOperation != Operation.None)
            {
                if (CurrentOperation != Operation.CancellingOperation)
                {
                    CurrentOperation = Operation.CancellingOperation;
                    closeInQueue = queueWindowToBeClosedAfterCancelling;
                    programDatabaseUpdatingCancellableThread?.RequestCancellation();
                    programUpdatesCheckCancellableThread?.RequestCancellation();
                    if (queueWindowToBeClosedAfterCancelling)
                        ChangeStatusMessages(STATUS_MESSAGE__CANCELLING_AND_CLOSING, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                    else
                        ChangeStatusMessages(STATUS_MESSAGE__CANCELLING, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                    ChangeProgressBarValue(-1);
                }
                else
                {
                    closeInQueue = queueWindowToBeClosedAfterCancelling;
                    if (queueWindowToBeClosedAfterCancelling)
                        ChangeStatusMessages(STATUS_MESSAGE__CANCELLING_AND_CLOSING, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                    else
                        ChangeStatusMessages(STATUS_MESSAGE__CANCELLING, (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH));
                }
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
                ((GridView)listView_programs.View).Columns.CollectionChanged -= OnGridViewColumnsCollectionCollectionChangedEvent;
                programDatabaseUpdatingCancellableThread?.RequestCancellation();
                programUpdatesCheckCancellableThread?.RequestCancellation();
                programDatabase?.Close();
                if (App.SettingsHandler != null)
                {
                    try
                    {
                        App.SettingsHandler.SettingsInMemory.Cached.LastWindowState = WindowState;
                        App.SettingsHandler.SettingsInMemory.Cached.LastWindowSize = new Size(Width, Height);
                        App.SettingsHandler.SettingsInMemory.Cached.LastWindowLocation = new Point(Left, Top);
                        if (App.SettingsHandler.SettingsInMemory.Global.RememberLastProgramListOptions)
                        {
                            App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringState = (bool)checkBox_filterProgramList.IsChecked;
                            App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringOption = (Settings.CachedSettings.ProgramFilteringOption)(Enum.Parse(typeof(Settings.CachedSettings.ProgramFilteringOption), ((string)comboBox_programListFilteringOption.SelectedItem).Replace(" ", "")));
                            App.SettingsHandler.SettingsInMemory.Cached.LastShowHiddenProgramsState = (bool)checkBox_showHiddenPrograms.IsChecked;
                        }
                        else
                        {
                            App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringState = false;
                            App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringOption = Settings.CachedSettings.ProgramFilteringOption.Unknown;
                            App.SettingsHandler.SettingsInMemory.Cached.LastShowHiddenProgramsState = false;
                        }
                        App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
                    }
                    catch { }
                }
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
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
