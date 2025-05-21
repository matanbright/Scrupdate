// Copyright © 2021-2024 Matan Brightbert
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
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
        private const int MINIMUM_PROGRAM_LIST_COLUMN_WIDTH = 30;
        private const string DIRECTORY_NAME__DOCS = "docs";
        private const string FILE_NAME__USER_MANUAL = "Scrupdate User Manual.pdf";
        private const string ERROR_DIALOG_TITLE__ERROR = "Error";
        private const string ERROR_DIALOG_MESSAGE__UNABLE_TO_OPEN_THE_USER_MANUAL_FILE = "Unable to Open the User-Manual File!";
        private const string ERROR_DIALOG_MESSAGE__PROGRAM_DATABASE_RECREATION_WAS_FAILED = "Program Database Recreation Was Failed!\r\n\r\n•  If this error persists, try to restart your computer or reinstall Scrupdate.";
        private const string ERROR_DIALOG_MESSAGE__FAILED_TO_RESET_ONE_OR_MORE_COMPONENTS = "Failed to Reset One or More Components!";
        private const string ERROR_DIALOG_MESSAGE__FAILED_TO_SAVE_SETTINGS = "Failed to Save Settings!";
        private const string QUESTION_DIALOG_MESSAGE__ARE_YOU_SURE_YOU_WANT_TO_CLOSE_SCRUPDATE_FORCEFULLY = "Are You Sure You Want to Close Scrupdate Forcefully?\r\n\r\n•  If you close Scrupdate forcefully, ChromeDriver will not have a chance to delete its temporary files.";
        private const string QUESTION_DIALOG_MESSAGE__DO_YOU_WANT_TO_RECREATE_THE_PROGRAM_DATABASE = "Do You Want to Recreate the Program Database?\r\n\r\n•  All the program information and configurations will be lost.";
        private const string QUESTION_DIALOG_MESSAGE__REMOVE_THE_SELECTED_PROGRAMS_FROM_THE_LIST = "Remove the Selected Program(s) from the List?";
        private const string STATUS_MESSAGE__SCANNING_INSTALLED_PROGRAMS = "Scanning Installed Programs…";
        private const string STATUS_MESSAGE__CHECKING_FOR_PROGRAM_UPDATES = "Checking for Program Updates…";
        private const string STATUS_MESSAGE__CANCELLING = "Cancelling…";
        private const string STATUS_MESSAGE__CANCELLING_AND_CLOSING = "Cancelling and Closing…";
        private const string STATUS_MESSAGE__NO_UPDATES_WERE_FOUND = "No Updates Were Found";
        private const string STATUS_MESSAGE__THERE_IS_AN_UPDATE = "There Is an Update";
        private const string STATUS_MESSAGE__THERE_ARE_N_UPDATES = "There Are {*} Updates";
        private const string ADDITIONAL_STATUS_MESSAGE__THERE_IS_AN_ERROR = "There Is an Error";
        private const string ADDITIONAL_STATUS_MESSAGE__THERE_ARE_N_ERRORS = "There Are {*} Errors";
        private const string LAST_CHECK_TIME_MESSAGE__LAST_CHECK = "Last Check: {*}";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__EDIT = "Edit";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__HIDE = "Hide";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__HIDE_SELECTED = "Hide Selected";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNHIDE = "Unhide";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNHIDE_SELECTED = "Unhide Selected";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE = "Remove";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED = "Remove Selected";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__SKIP_VERSION = "Skip Version";
        private const string PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNSKIP_VERSION = "Unskip Version";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum Error
        {
            None,
            CanNotOpenProgramDatabase,
            ProgramDatabaseIsCorrupted,
            ProgramDatabaseIsNotCompatible
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
        public static readonly string userManualFilePath =
            (new StringBuilder(
                256 + 1 + DIRECTORY_NAME__DOCS.Length + 1 + FILE_NAME__USER_MANUAL.Length
            ))
                .Append(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .Append('\\')
                .Append(DIRECTORY_NAME__DOCS)
                .Append('\\')
                .Append(FILE_NAME__USER_MANUAL)
                .ToString();
        public static readonly DependencyProperty CurrentErrorProperty = DependencyProperty.Register(
            nameof(CurrentError),
            typeof(Error),
            typeof(MainWindow),
            new PropertyMetadata(Error.None)
        );
        public static readonly DependencyProperty IsAutomaticScanningForInstalledProgramsEnabledProperty = DependencyProperty.Register(
            nameof(IsAutomaticScanningForInstalledProgramsEnabled),
            typeof(bool),
            typeof(MainWindow),
            new PropertyMetadata(false)
        );
        public static readonly DependencyProperty CurrentOperationProperty = DependencyProperty.Register(
            nameof(CurrentOperation),
            typeof(Operation),
            typeof(MainWindow),
            new PropertyMetadata(Operation.None)
        );
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
        public Size BaseSize { get; private set; }
        public Error CurrentError
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Error)GetValue(CurrentErrorProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(CurrentErrorProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(CurrentError))
                            );
                        }
                );
            }
        }
        public bool IsAutomaticScanningForInstalledProgramsEnabled
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (bool)GetValue(IsAutomaticScanningForInstalledProgramsEnabledProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(IsAutomaticScanningForInstalledProgramsEnabledProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(IsAutomaticScanningForInstalledProgramsEnabled))
                            );
                        }
                );
            }
        }
        public Operation CurrentOperation
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Operation)GetValue(CurrentOperationProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(CurrentOperationProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(CurrentOperation))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public MainWindow() : this(Settings.CachedSettings.ProgramFilteringOption.Unknown, null) { }
        public MainWindow(Settings.CachedSettings.ProgramFilteringOption programFilteringOptionOnStart,
                          bool? isShowingHiddenProgramsOnStart)
        {
            try
            {
                ApplicationUtilities.CreateDataFolderIfNotExists();
                if (!File.Exists(ApplicationUtilities.programDatabaseFilePath))
                {
                    App.SettingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = "";
                    App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
                }
                programDatabase = new ProgramDatabase(
                    ApplicationUtilities.programDatabaseFilePath,
                    ApplicationUtilities.programDatabaseChecksumFilePath
                );
                ConfigError programDatabaseFileError;
                if (!programDatabase.Open(true, true, out programDatabaseFileError))
                {
                    CurrentError = Error.CanNotOpenProgramDatabase;
                    switch (programDatabaseFileError)
                    {
                        case ConfigError.Corrupted:
                            CurrentError = Error.ProgramDatabaseIsCorrupted;
                            break;
                        case ConfigError.NotCompatible:
                            CurrentError = Error.ProgramDatabaseIsNotCompatible;
                            break;
                    }
                }
            }
            catch
            {
                CurrentError = Error.CanNotOpenProgramDatabase;
            }
            programListViewItems = new List<ProgramListViewItem>();
            InitializeComponent();
            BaseSize = new Size(Width, Height);
            WindowUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSize,
                App.WindowsRenderingScale
            );
            ((GridView)listView_programs.View).Columns.CollectionChanged +=
                OnGridViewColumnsCollectionCollectionChangedEvent;
            listView_programs.ItemsSource = programListViewItems;
            label_appVersion.Content =
                ((string)label_appVersion.Content).Replace(
                    "{*}",
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(3)
                );
            foreach (Settings.CachedSettings.ProgramFilteringOption programFilteringOption in
                     Enum.GetValues(typeof(Settings.CachedSettings.ProgramFilteringOption)))
            {
                if (programFilteringOption != Settings.CachedSettings.ProgramFilteringOption.Unknown)
                {
                    comboBox_programListFilteringOption.Items.Add(
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            programFilteringOption
                        )
                    );
                }
            }
            comboBox_programListFilteringOption.SelectedItem =
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    Settings.CachedSettings.ProgramFilteringOption.All
                );
            if (App.SettingsHandler.SettingsInMemory.General.RememberLastProgramListOptions)
            {
                checkBox_filterProgramList.IsChecked =
                    App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringState;
                comboBox_programListFilteringOption.SelectedItem =
                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                        App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringOption
                    );
                checkBox_showHiddenPrograms.IsChecked =
                    App.SettingsHandler.SettingsInMemory.Cached.LastShowHiddenProgramsState;
            }
            if (programFilteringOptionOnStart != Settings.CachedSettings.ProgramFilteringOption.Unknown)
            {
                checkBox_filterProgramList.IsChecked = true;
                comboBox_programListFilteringOption.SelectedItem =
                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                        programFilteringOptionOnStart
                    );
            }
            if (isShowingHiddenProgramsOnStart != null)
                checkBox_showHiddenPrograms.IsChecked = isShowingHiddenProgramsOnStart;
            button_checkForProgramUpdates.Focus();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnLoadedEvent(object sender, RoutedEventArgs e)
        {
            if (App.SettingsHandler.SettingsInMemory.Cached.LastWindowState != null)
                WindowState = (WindowState)App.SettingsHandler.SettingsInMemory.Cached.LastWindowState;
            if (App.SettingsHandler.SettingsInMemory.Cached.LastWindowSize != null)
            {
                Width = ((Size)App.SettingsHandler.SettingsInMemory.Cached.LastWindowSize).Width;
                Height = ((Size)App.SettingsHandler.SettingsInMemory.Cached.LastWindowSize).Height;
            }
            if (App.SettingsHandler.SettingsInMemory.Cached.LastWindowLocation != null)
            {
                Left = ((Point)App.SettingsHandler.SettingsInMemory.Cached.LastWindowLocation).X;
                Top = ((Point)App.SettingsHandler.SettingsInMemory.Cached.LastWindowLocation).Y;
            }
            WindowUtilities.MoveWindowIntoScreenBoundaries(this, true);
            IsAutomaticScanningForInstalledProgramsEnabled =
                App.SettingsHandler.SettingsInMemory.General.EnableScanningForInstalledPrograms;
            ((CustomGridViewColumnHeader)((GridView)listView_programs.View).Columns[2].Header).RaiseEvent(
                new RoutedEventArgs(CustomGridViewColumnHeader.ClickEvent)
            );
            if (CurrentError == Error.None)
            {
                if (App.SettingsHandler.SettingsInMemory.General.EnableScanningForInstalledPrograms &&
                    App.SettingsHandler.SettingsInMemory.General.ScanForInstalledProgramsAutomaticallyOnStart)
                {
                    StartProgramDatabaseUpdatingTask();
                }
                else
                    RefreshListViewAndAllMessages(true);
            }
        }
        private void OnClosingEvent(object sender, CancelEventArgs e)
        {
            if (!resettingAllSettingsAndData)
            {
                if (CurrentOperation == Operation.CancellingOperation ||
                    CurrentOperation == Operation.UpdatingProgramDatabase ||
                    CurrentOperation == Operation.CheckingForProgramUpdates)
                {
                    e.Cancel = true;
                    if (CurrentOperation == Operation.CancellingOperation && closeInQueue)
                    {
                        if (DialogUtilities.ShowQuestionDialog(
                                "",
                                QUESTION_DIALOG_MESSAGE__ARE_YOU_SURE_YOU_WANT_TO_CLOSE_SCRUPDATE_FORCEFULLY,
                                this
                            ) == true)
                        {
                            PrepareWindowForClosing(true);
                        }
                    }
                    else
                        CancelOperation(true);
                }
                else
                    PrepareWindowForClosing();
            }
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_hideSelectedPrograms)
                HideOrUnhideSelectedProgramsInDatabaseAndListView(true);
            else if (senderButton == button_unhideSelectedPrograms)
                HideOrUnhideSelectedProgramsInDatabaseAndListView(false);
            else if (senderButton == button_removeSelectedPrograms)
            {
                if (DialogUtilities.ShowQuestionDialog(
                        "",
                        QUESTION_DIALOG_MESSAGE__REMOVE_THE_SELECTED_PROGRAMS_FROM_THE_LIST,
                        this
                    ) == true)
                {
                    RemoveSelectedProgramsFromDatabaseAndListView();
                }
            }
            else if (senderButton == button_addNewProgram)
            {
                Program newProgram;
                if (OpenProgramAddingOrEditingWindowAsDialogForAddingAProgram(out newProgram) == true)
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
                OpenUserManualFile();
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
                        if (App.SettingsHandler != null)
                        {
                            App.SettingsHandler.Dispose();
                            App.SettingsHandler = null;
                        }
                        if (!ApplicationUtilities.ResetAll())
                        {
                            DialogUtilities.ShowErrorDialog(
                                ERROR_DIALOG_TITLE__ERROR,
                                ERROR_DIALOG_MESSAGE__FAILED_TO_RESET_ONE_OR_MORE_COMPONENTS,
                                this
                            );
                        }
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
                if (Array.TrueForAll(
                        textBox_programListSearchingPhrase.Text.ToCharArray(),
                        c => char.IsWhiteSpace(c)
                    ))
                {
                    textBox_programListSearchingPhrase.Text = "";
                }
                RefreshListViewAndAllMessages();
            }
        }
        private void OnCheckBoxClickEvent(object sender, RoutedEventArgs e)
        {
            CustomCheckBox senderCheckBox = (CustomCheckBox)sender;
            if (senderCheckBox == checkBox_filterProgramList)
            {
                if (checkBox_filterProgramList.IsChecked == false)
                {
                    comboBox_programListFilteringOption.SelectedItem =
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            Settings.CachedSettings.ProgramFilteringOption.All
                        );
                }
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
            {
                ((CheckBox)gridViewColumnHeader_programSelectionCheckBox.Content).IsChecked =
                    (listView_programs.Items.Count != 0 &&
                     (listView_programs.SelectedItems.Count == listView_programs.Items.Count));
            }
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
                        ProgramListViewItem programListViewItemOfProgramToEdit =
                            (ProgramListViewItem)listView_programs.SelectedItems[0];
                        Program updatedProgram;
                        if (OpenProgramAddingOrEditingWindowAsDialogForEditingAProgram(
                                programListViewItemOfProgramToEdit.UnderlyingProgram.Name,
                                out updatedProgram
                            ) == true)
                        {
                            UpdateProgramInDatabaseAndListView(
                                programListViewItemOfProgramToEdit,
                                updatedProgram
                            );
                        }
                    }
                }
                else if (e.Key == Key.Delete)
                {
                    if (listView_programs.SelectedItems.Count > 0)
                    {
                        if (DialogUtilities.ShowQuestionDialog(
                                "",
                                QUESTION_DIALOG_MESSAGE__REMOVE_THE_SELECTED_PROGRAMS_FROM_THE_LIST,
                                this
                            ) == true)
                        {
                            RemoveSelectedProgramsFromDatabaseAndListView();
                        }
                    }
                }
            }
        }
        private void OnGridViewColumnHeaderPreviewMouseMoveEvent(object sender, MouseEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_programSelectionCheckBox ||
                senderGridViewColumnHeader == gridViewColumnHeader_programUpdateCheckConfigurationStatus)
            {
                e.Handled = true;
            }
        }
        private void OnGridViewColumnHeaderPreviewMouseDoubleClickEvent(object sender, MouseButtonEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_programSelectionCheckBox ||
                senderGridViewColumnHeader == gridViewColumnHeader_programUpdateCheckConfigurationStatus)
            {
                e.Handled = true;
            }
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
                    {
                        ((CustomGridViewColumnHeader)gridViewColumn.Header).ListViewItemsSortingOrder =
                            CustomGridViewColumnHeader.SortingOrder.None;
                    }
                    senderGridViewColumnHeader.ListViewItemsSortingOrder = CustomGridViewColumnHeader.SortingOrder.Ascending;
                }
                listView_programs.Items.SortDescriptions.Clear();
                listView_programs.Items.SortDescriptions.Add(
                    new SortDescription(
                        (string)senderGridViewColumnHeader.Tag,
                        (senderGridViewColumnHeader.ListViewItemsSortingOrder == CustomGridViewColumnHeader.SortingOrder.Descending ?
                            ListSortDirection.Descending :
                            ListSortDirection.Ascending)
                    )
                );
                RefreshListViewAndAllMessages();
            }
        }
        private void OnGridViewColumnHeaderSizeChangedEvent(object sender, SizeChangedEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_programName ||
                senderGridViewColumnHeader == gridViewColumnHeader_programInstalledVersion ||
                senderGridViewColumnHeader == gridViewColumnHeader_programLatestVersion ||
                senderGridViewColumnHeader == gridViewColumnHeader_installationScope ||
                senderGridViewColumnHeader == gridViewColumnHeader_notes)
            {
                if (senderGridViewColumnHeader.Column.Width < MINIMUM_PROGRAM_LIST_COLUMN_WIDTH)
                {
                    senderGridViewColumnHeader.Column.Width = MINIMUM_PROGRAM_LIST_COLUMN_WIDTH;
                    e.Handled = true;
                }
            }
        }
        private void OnGridViewColumnCellTextBlockSizeChangedEvent(object sender, SizeChangedEventArgs e)
        {
            TextBlock senderTextBlock = (TextBlock)sender;
            senderTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (senderTextBlock.ActualWidth < senderTextBlock.DesiredSize.Width)
                senderTextBlock.ToolTip = senderTextBlock.Text;
            else
                senderTextBlock.ToolTip = null;
        }
        private void OnListViewItemMouseDoubleClickEvent(object sender, MouseButtonEventArgs e)
        {
            ListViewItem senderListViewItem = (ListViewItem)sender;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (listView_programs.SelectedItems.Count == 1)
                {
                    ProgramListViewItem programListViewItemOfProgramToEdit =
                        (ProgramListViewItem)senderListViewItem.Content;
                    if (programListViewItemOfProgramToEdit == listView_programs.SelectedItems[0])
                    {
                        Program updatedProgram;
                        if (OpenProgramAddingOrEditingWindowAsDialogForEditingAProgram(
                                programListViewItemOfProgramToEdit.UnderlyingProgram.Name,
                                out updatedProgram
                            ) == true)
                        {
                            UpdateProgramInDatabaseAndListView(
                                programListViewItemOfProgramToEdit,
                                updatedProgram
                            );
                        }
                    }
                }
            }
        }
        private void OnListViewItemContextMenuOpeningEvent(object sender, ContextMenuEventArgs e)
        {
            ListViewItem senderListViewItem = (ListViewItem)sender;
            Program senderListViewItemUnderlyingProgram =
                ((ProgramListViewItem)senderListViewItem.Content).UnderlyingProgram;
            List<object> menuItems = new List<object>();
            int selectedListViewItemsCount = listView_programs.SelectedItems.Count;
            if (selectedListViewItemsCount == 1)
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        FontWeight = FontWeights.Bold,
                        Header = PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__EDIT
                    }
                );
                menuItems.Add(new Separator());
            }
            menuItems.Add(
                new MenuItem()
                {
                    Header =
                        (selectedListViewItemsCount > 1 ?
                            PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__HIDE_SELECTED :
                            PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__HIDE)
                }
            );
            if (checkBox_showHiddenPrograms.IsChecked == true)
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header =
                            (selectedListViewItemsCount > 1 ?
                                PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNHIDE_SELECTED :
                                PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNHIDE)
                    }
                );
            }
            menuItems.Add(
                new MenuItem()
                {
                    Header =
                        (selectedListViewItemsCount > 1 ?
                            PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED :
                            PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE)
                }
            );
            if (selectedListViewItemsCount == 1)
            {
                if (!senderListViewItemUnderlyingProgram.SkippedVersion.Equals(""))
                {
                    menuItems.Add(new Separator());
                    menuItems.Add(new MenuItem() { Header = PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNSKIP_VERSION });
                }
                else if (VersionUtilities.IsVersionNewer(
                             senderListViewItemUnderlyingProgram.LatestVersion,
                             senderListViewItemUnderlyingProgram.InstalledVersion
                         ))
                {
                    menuItems.Add(new Separator());
                    menuItems.Add(new MenuItem() { Header = PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__SKIP_VERSION });
                }
            }
            senderListViewItem.ContextMenu.Items.Clear();
            foreach (object menuItem in menuItems)
            {
                if (menuItem.GetType() == typeof(MenuItem))
                    ((MenuItem)menuItem).Click += OnMenuItemClickEvent;
                senderListViewItem.ContextMenu.Items.Add(menuItem);
            }
        }
        private void OnMenuItemClickEvent(object sender, RoutedEventArgs e)
        {
            MenuItem senderMenuItem = (MenuItem)sender;
            if (senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__EDIT))
            {
                ProgramListViewItem programListViewItemOfProgramToEdit =
                    (ProgramListViewItem)listView_programs.SelectedItems[0];
                Program updatedProgram;
                if (OpenProgramAddingOrEditingWindowAsDialogForEditingAProgram(
                        programListViewItemOfProgramToEdit.UnderlyingProgram.Name,
                        out updatedProgram
                    ) == true)
                {
                    UpdateProgramInDatabaseAndListView(
                        programListViewItemOfProgramToEdit,
                        updatedProgram
                    );
                }
            }
            else if (senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__HIDE) ||
                     senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__HIDE_SELECTED))
            {
                HideOrUnhideSelectedProgramsInDatabaseAndListView(true);
            }
            else if (senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNHIDE) ||
                     senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNHIDE_SELECTED))
            {
                HideOrUnhideSelectedProgramsInDatabaseAndListView(false);
            }
            else if (senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE) ||
                     senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED))
            {
                if (DialogUtilities.ShowQuestionDialog(
                        "",
                        QUESTION_DIALOG_MESSAGE__REMOVE_THE_SELECTED_PROGRAMS_FROM_THE_LIST,
                        this
                    ) == true)
                {
                    RemoveSelectedProgramsFromDatabaseAndListView();
                }
            }
            else if (senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__SKIP_VERSION))
                SkipOrUnskipVersionOfSelectedProgram(true);
            else if (senderMenuItem.Header.Equals(PROGRAM_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__UNSKIP_VERSION))
                SkipOrUnskipVersionOfSelectedProgram(false);
        }
        private void OnHyperlinkClickEvent(object sender, RoutedEventArgs e)
        {
            Hyperlink senderHyperlink = (Hyperlink)sender;
            if (senderHyperlink == hyperlink_fixProgramDatabase)
            {
                if (CurrentError == Error.ProgramDatabaseIsCorrupted ||
                    CurrentError == Error.ProgramDatabaseIsNotCompatible)
                {
                    if (!DialogUtilities.ShowQuestionDialog(
                            "",
                            QUESTION_DIALOG_MESSAGE__DO_YOU_WANT_TO_RECREATE_THE_PROGRAM_DATABASE,
                            this
                        ) == true)
                    {
                        return;
                    }
                    else
                    {
                        if (!ApplicationUtilities.ResetProgramDatabase(App.SettingsHandler))
                        {
                            DialogUtilities.ShowErrorDialog(
                                ERROR_DIALOG_TITLE__ERROR,
                                ERROR_DIALOG_MESSAGE__PROGRAM_DATABASE_RECREATION_WAS_FAILED,
                                this
                            );
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
                    programDatabase = new ProgramDatabase(
                        ApplicationUtilities.programDatabaseFilePath,
                        ApplicationUtilities.programDatabaseChecksumFilePath
                    );
                    ConfigError programDatabaseFileError;
                    if (!programDatabase.Open(true, true, out programDatabaseFileError))
                    {
                        CurrentError = Error.CanNotOpenProgramDatabase;
                        switch (programDatabaseFileError)
                        {
                            case ConfigError.Corrupted:
                                CurrentError = Error.ProgramDatabaseIsCorrupted;
                                break;
                            case ConfigError.NotCompatible:
                                CurrentError = Error.ProgramDatabaseIsNotCompatible;
                                break;
                        }
                        return;
                    }
                }
                catch
                {
                    CurrentError = Error.CanNotOpenProgramDatabase;
                    return;
                }
                CurrentError = Error.None;
                if (App.SettingsHandler.SettingsInMemory.General.EnableScanningForInstalledPrograms &&
                    App.SettingsHandler.SettingsInMemory.General.ScanForInstalledProgramsAutomaticallyOnStart)
                {
                    StartProgramDatabaseUpdatingTask();
                }
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
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        settingsWindow = new SettingsWindow(
                            App.SettingsHandler.SettingsInMemory,
                            (programDatabase == null ? false : programDatabase.IsOpen())
                        );
                        settingsWindow.Owner = this;
                        returnValue = settingsWindow.ShowDialog();
                    }
            );
            if (returnValue == true)
                updatedSettings = settingsWindow.GetUpdatedSettings();
            return returnValue;
        }
        private void OpenUserManualFile()
        {
            try
            {
                ProcessUtilities.RunFile(
                    userManualFilePath,
                    null,
                    true,
                    false,
                    false,
                    -1,
                    false,
                    false,
                    out _
                );
            }
            catch
            {
                DialogUtilities.ShowErrorDialog(
                    ERROR_DIALOG_TITLE__ERROR,
                    ERROR_DIALOG_MESSAGE__UNABLE_TO_OPEN_THE_USER_MANUAL_FILE,
                    this
                );
            }
        }
        private bool? OpenAboutWindowAsDialog()
        {
            bool? returnValue = null;
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        AboutWindow aboutWindow = new AboutWindow();
                        aboutWindow.Owner = this;
                        returnValue = aboutWindow.ShowDialog();
                    }
            );
            return returnValue;
        }
        private bool? OpenProgramAddingOrEditingWindowAsDialogForAddingAProgram(out Program newProgram)
        {
            newProgram = null;
            bool? returnValue = null;
            ProgramAddingOrEditingWindow programAddingOrEditingWindow = null;
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        Dictionary<string, Program> programsAlreadyInDatabase = programDatabase.GetPrograms();
                        programAddingOrEditingWindow = new ProgramAddingOrEditingWindow(programsAlreadyInDatabase);
                        programAddingOrEditingWindow.Owner = this;
                        returnValue = programAddingOrEditingWindow.ShowDialog();
                    }
            );
            if (returnValue == true)
                newProgram = programAddingOrEditingWindow.GetNewOrUpdatedProgram();
            return returnValue;
        }
        private bool? OpenProgramAddingOrEditingWindowAsDialogForEditingAProgram(string nameOfProgramToEdit,
                                                                                 out Program updatedProgram)
        {
            updatedProgram = null;
            bool? returnValue = null;
            ProgramAddingOrEditingWindow programAddingOrEditingWindow = null;
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        Dictionary<string, Program> programsAlreadyInDatabase = programDatabase.GetPrograms();
                        Program programToEdit = programsAlreadyInDatabase[nameOfProgramToEdit];
                        programAddingOrEditingWindow = new ProgramAddingOrEditingWindow(
                            programsAlreadyInDatabase,
                            programToEdit
                        );
                        programAddingOrEditingWindow.Owner = this;
                        returnValue = programAddingOrEditingWindow.ShowDialog();
                    }
            );
            if (returnValue == true)
                updatedProgram = programAddingOrEditingWindow.GetNewOrUpdatedProgram();
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
                for (Dictionary<string, Program>.Enumerator i = programDatabase.GetPrograms().GetEnumerator();
                     i.MoveNext();)
                {
                    Program program = i.Current.Value;
                    programListViewItems.Add(new ProgramListViewItem(program));
                }
                programListViewItems.Sort(
                    (programListViewItem1, programListViewItem2) =>
                        string.Compare(
                            programListViewItem1.UnderlyingProgram.Name,
                            programListViewItem2.UnderlyingProgram.Name
                        )
                );
            }
            updatesCount = 0;
            errorsCount = 0;
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        string programSearchPhrase = textBox_programListSearchingPhrase.Text.Trim();
                        bool isFilteringPrograms = checkBox_filterProgramList.IsChecked.Value;
                        Settings.CachedSettings.ProgramFilteringOption selectedProgramFilteringOption =
                            (!isFilteringPrograms ?
                                Settings.CachedSettings.ProgramFilteringOption.All :
                                EnumUtilities.GetEnumItemFromHumanReadableString<Settings.CachedSettings.ProgramFilteringOption>(
                                    (string)comboBox_programListFilteringOption.SelectedItem
                                ));
                        bool isShowingHiddenPrograms = checkBox_showHiddenPrograms.IsChecked.Value;
                        listView_programs.Items.Filter = new Predicate<object>(
                            o =>
                                {
                                    ProgramListViewItem programListViewItem = (ProgramListViewItem)o;
                                    Program program = programListViewItem.UnderlyingProgram;
                                    bool? thereIsANewerVersion = null;
                                    if (program.InstallationScope != Program._InstallationScope.None)
                                    {
                                        thereIsANewerVersion =
                                            ((program.InstalledVersion.Equals("") || program.LatestVersion.Equals("")) ?
                                                false :
                                                VersionUtilities.IsVersionNewer(
                                                    program.LatestVersion,
                                                    program.InstalledVersion
                                                ));
                                    }
                                    programListViewItem.ProgramInstalledVersionToDisplay =
                                        (program.InstalledVersion.Equals("") ?
                                            "" :
                                            VersionUtilities.NormalizeAndTrimVersion(
                                                program.InstalledVersion,
                                                App.SettingsHandler.SettingsInMemory.Appearance.MinimumVersionSegments,
                                                App.SettingsHandler.SettingsInMemory.Appearance.MaximumVersionSegments,
                                                App.SettingsHandler.SettingsInMemory.Appearance.RemoveTrailingZeroSegmentsOfVersions
                                            ));
                                    programListViewItem.ProgramLatestVersionToDisplay =
                                        (program.LatestVersion.Equals("") ?
                                            "" :
                                            VersionUtilities.NormalizeAndTrimVersion(
                                                program.LatestVersion,
                                                App.SettingsHandler.SettingsInMemory.Appearance.MinimumVersionSegments,
                                                App.SettingsHandler.SettingsInMemory.Appearance.MaximumVersionSegments,
                                                App.SettingsHandler.SettingsInMemory.Appearance.RemoveTrailingZeroSegmentsOfVersions
                                            ));
                                    programListViewItem.Notes =
                                        (program.SkippedVersion.Equals("") ?
                                            "" :
                                            (new StringBuilder(
                                                10 + programListViewItem.UnderlyingProgram.SkippedVersion.Length
                                            ))
                                                .Append("Skipping: ")
                                                .Append(programListViewItem.UnderlyingProgram.SkippedVersion)
                                                .ToString());
                                    Brush programListViewItemForeground = (SolidColorBrush)Application.Current.FindResource(
                                        App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH
                                    );
                                    if (!program.IsAutomaticallyAdded)
                                    {
                                        programListViewItemForeground = (SolidColorBrush)Application.Current.FindResource(
                                            App.RESOURCE_KEY__GRAY_SOLID_COLOR_BRUSH
                                        );
                                    }
                                    if (thereIsANewerVersion == true &&
                                        (program.SkippedVersion.Equals("") ||
                                         VersionUtilities.IsVersionNewer(
                                             program.LatestVersion,
                                             program.SkippedVersion
                                         )))
                                    {
                                        if (!program.IsHidden || (program.IsHidden && isShowingHiddenPrograms))
                                            updatesCount++;
                                        programListViewItemForeground = (SolidColorBrush)Application.Current.FindResource(
                                            App.RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH
                                        );
                                    }
                                    if (program.UpdateCheckConfigurationStatus == Program._UpdateCheckConfigurationStatus.Invalid)
                                        if (!program.IsHidden || (program.IsHidden && isShowingHiddenPrograms))
                                            errorsCount++;
                                    if (program.InstallationScope == Program._InstallationScope.None)
                                    {
                                        programListViewItemForeground = (SolidColorBrush)Application.Current.FindResource(
                                            App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH
                                        );
                                    }
                                    if (program.IsHidden)
                                    {
                                        programListViewItemForeground = (SolidColorBrush)Application.Current.FindResource(
                                            App.RESOURCE_KEY__LIGHT_GRAY_SOLID_COLOR_BRUSH_2
                                        );
                                    }
                                    programListViewItem.Foreground = programListViewItemForeground;
                                    if (program.IsHidden && !isShowingHiddenPrograms)
                                        return false;
                                    if (!programSearchPhrase.Equals("") &&
                                        !program.Name.Contains(
                                            programSearchPhrase,
                                            StringComparison.CurrentCultureIgnoreCase
                                        ))
                                    {
                                        return false;
                                    }
                                    if (isFilteringPrograms)
                                    {
                                        switch (selectedProgramFilteringOption)
                                        {
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyUpdates:
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyUpToDate:
                                                if (!program.IsUpdateCheckConfigured ||
                                                    program.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Valid)
                                                {
                                                    return false;
                                                }
                                                switch (selectedProgramFilteringOption)
                                                {
                                                    case Settings.CachedSettings.ProgramFilteringOption.OnlyUpdates:
                                                        if (thereIsANewerVersion != true ||
                                                            (thereIsANewerVersion == true &&
                                                             !(program.SkippedVersion.Equals("") ||
                                                               VersionUtilities.IsVersionNewer(
                                                                   program.LatestVersion,
                                                                   program.SkippedVersion
                                                               ))))
                                                        {
                                                            return false;
                                                        }
                                                        break;
                                                    default:
                                                        if (thereIsANewerVersion != false)
                                                            return false;
                                                        break;
                                                }
                                                break;
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyAutomaticallyAdded:
                                                if (!program.IsAutomaticallyAdded)
                                                    return false;
                                                break;
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyManuallyAdded:
                                                if (program.IsAutomaticallyAdded)
                                                    return false;
                                                break;
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyInstalled:
                                                if (program.InstallationScope == Program._InstallationScope.None)
                                                    return false;
                                                break;
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyUninstalled:
                                                if (program.InstallationScope != Program._InstallationScope.None)
                                                    return false;
                                                break;
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlySkipped:
                                                if (program.SkippedVersion.Equals(""))
                                                    return false;
                                                break;
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyValid:
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyInvalid:
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyNotChecked:
                                                if (!program.IsUpdateCheckConfigured)
                                                    return false;
                                                switch (selectedProgramFilteringOption)
                                                {
                                                    case Settings.CachedSettings.ProgramFilteringOption.OnlyValid:
                                                        if (program.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Valid)
                                                            return false;
                                                        break;
                                                    case Settings.CachedSettings.ProgramFilteringOption.OnlyInvalid:
                                                        if (program.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Invalid)
                                                            return false;
                                                        break;
                                                    default:
                                                        if (program.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Unknown)
                                                            return false;
                                                        break;
                                                }
                                                break;
                                            case Settings.CachedSettings.ProgramFilteringOption.OnlyNotConfigured:
                                                if (program.IsUpdateCheckConfigured)
                                                    return false;
                                                break;
                                        }
                                    }
                                    return true;
                                }
                        );
                        ((CheckBox)gridViewColumnHeader_programSelectionCheckBox.Content).IsChecked =
                            (listView_programs.Items.Count != 0 &&
                             listView_programs.SelectedItems.Count == listView_programs.Items.Count);
                    }
            );
            string statusMessage =
                (updatesCount > 0 ?
                    (updatesCount > 1 ?
                        STATUS_MESSAGE__THERE_ARE_N_UPDATES.Replace("{*}", Convert.ToString(updatesCount)) :
                        STATUS_MESSAGE__THERE_IS_AN_UPDATE) :
                    STATUS_MESSAGE__NO_UPDATES_WERE_FOUND);
            Brush statusMessageForegroundColor =
                (updatesCount > 0 ?
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH
                    ) :
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__LIGHT_BLACK_SOLID_COLOR_BRUSH
                    ));
            string additionalStatusMessage =
                (errorsCount > 0 ?
                    (errorsCount > 1 ?
                        ADDITIONAL_STATUS_MESSAGE__THERE_ARE_N_ERRORS.Replace("{*}", Convert.ToString(errorsCount)) :
                        ADDITIONAL_STATUS_MESSAGE__THERE_IS_AN_ERROR) :
                    "");
            Brush additionalStatusMessageForegroundColor =
                (errorsCount > 0 ?
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH
                    ) :
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH
                    ));
            ChangeStatusMessages(
                statusMessage,
                statusMessageForegroundColor,
                additionalStatusMessage,
                additionalStatusMessageForegroundColor
            );
            ChangeTimeToShowInLastProgramUpdatesCheckTimeMessage(
                App.SettingsHandler.SettingsInMemory.Cached.LastProgramUpdatesCheckTime
            );
        }
        private void AddNewProgramToDatabaseAndListView(Program newProgram)
        {
            App.SettingsHandler.SettingsInMemory.Cached.LastHashOfAllInstalledPrograms = "";
            App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
            programDatabase.AddNewProgram(newProgram);
            programListViewItems.Add(new ProgramListViewItem(newProgram));
            programListViewItems.Sort(
                (programListViewItem1, programListViewItem2) =>
                    string.Compare(
                        programListViewItem1.UnderlyingProgram.Name,
                        programListViewItem2.UnderlyingProgram.Name
                    )
            );
            RefreshListViewAndAllMessages();
        }
        private void UpdateProgramInDatabaseAndListView(ProgramListViewItem selectedProgramListViewItem,
                                                        Program updatedProgram)
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
            programListViewItems.Sort(
                (programListViewItem1, programListViewItem2) =>
                    string.Compare(
                        programListViewItem1.UnderlyingProgram.Name,
                        programListViewItem2.UnderlyingProgram.Name
                    )
            );
            bool? selectedProgramHasAnUpdate = null;
            if (selectedProgram.InstallationScope != Program._InstallationScope.None)
            {
                selectedProgramHasAnUpdate =
                    (selectedProgram.LatestVersion.Equals("") ?
                        false :
                        (selectedProgram.InstalledVersion.Equals("") ?
                            true :
                            VersionUtilities.IsVersionNewer(
                                selectedProgram.LatestVersion,
                                selectedProgram.InstalledVersion
                            )));
            }
            bool? updatedProgramHasAnUpdate = null;
            if (updatedProgram.InstallationScope != Program._InstallationScope.None)
            {
                updatedProgramHasAnUpdate =
                    (updatedProgram.LatestVersion.Equals("") ?
                        false :
                        (updatedProgram.InstalledVersion.Equals("") ?
                            true :
                            VersionUtilities.IsVersionNewer(
                                updatedProgram.LatestVersion,
                                updatedProgram.InstalledVersion
                            )));
            }
            if (selectedProgramHasAnUpdate != true && updatedProgramHasAnUpdate == true)
                updatesCount++;
            else if (selectedProgramHasAnUpdate == true && updatedProgramHasAnUpdate != true)
                updatesCount--;
            if (selectedProgram.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Invalid &&
                updatedProgram.UpdateCheckConfigurationStatus == Program._UpdateCheckConfigurationStatus.Invalid)
            {
                errorsCount++;
            }
            else if (selectedProgram.UpdateCheckConfigurationStatus == Program._UpdateCheckConfigurationStatus.Invalid &&
                     updatedProgram.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Invalid)
            {
                errorsCount--;
            }
            RefreshListViewAndAllMessages();
        }
        private void SkipOrUnskipVersionOfSelectedProgram(bool skip)
        {
            if (listView_programs.SelectedItem != null)
            {
                ProgramListViewItem selectedProgramListViewItem = (ProgramListViewItem)listView_programs.SelectedItem;
                Program selectedProgram = selectedProgramListViewItem.UnderlyingProgram;
                if (skip)
                {
                    string skippedVersion = selectedProgram.LatestVersion;
                    programDatabase.SkipVersionOfProgram(selectedProgram.Name, skippedVersion);
                    selectedProgram.SkippedVersion = skippedVersion;
                }
                else
                {
                    programDatabase.UnskipVersionOfProgram(selectedProgram.Name);
                    selectedProgram.SkippedVersion = "";
                }
                RefreshListViewAndAllMessages();
            }
        }
        private void HideOrUnhideSelectedProgramsInDatabaseAndListView(bool hide)
        {
            if (listView_programs.SelectedItems.Count > 0)
            {
                programDatabase.BeginTransaction();
                foreach (ProgramListViewItem selectedProgramListViewItem in listView_programs.SelectedItems)
                {
                    Program selectedProgram = selectedProgramListViewItem.UnderlyingProgram;
                    if (hide)
                        programDatabase.HideProgram(selectedProgram.Name);
                    else
                        programDatabase.UnhideProgram(selectedProgram.Name);
                    selectedProgram.IsHidden = hide;
                }
                programDatabase.EndTransaction();
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
                    bool? selectedProgramHasAnUpdate = null;
                    if (selectedProgram.InstallationScope != Program._InstallationScope.None)
                    {
                        selectedProgramHasAnUpdate =
                            (selectedProgram.LatestVersion.Equals("") ?
                                false :
                                (selectedProgram.InstalledVersion.Equals("") ?
                                    true :
                                    VersionUtilities.IsVersionNewer(
                                        selectedProgram.LatestVersion,
                                        selectedProgram.InstalledVersion
                                    )));
                    }
                    if (selectedProgramHasAnUpdate == true)
                        updatesCount--;
                    if (selectedProgram.UpdateCheckConfigurationStatus == Program._UpdateCheckConfigurationStatus.Invalid)
                        errorsCount--;
                }
                programDatabase.EndTransaction();
                RefreshListViewAndAllMessages();
            }
        }
        private void ChangeStatusMessage(string statusMessage, Brush statusMessageForegroundColor)
        {
            ChangeStatusMessages(
                statusMessage,
                statusMessageForegroundColor,
                "",
                (SolidColorBrush)Application.Current.FindResource(
                    App.RESOURCE_KEY__TRANSPARENT_SOLID_COLOR_BRUSH
                )
            );
        }
        private void ChangeStatusMessages(string statusMessage,
                                          Brush statusMessageForegroundColor,
                                          string additionalStatusMessage,
                                          Brush additionalStatusMessageForegroundColor)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        label_statusMessage.Content = statusMessage;
                        label_statusMessage.Foreground = statusMessageForegroundColor;
                        label_additionalStatusMessage.Content = additionalStatusMessage;
                        label_additionalStatusMessage.Foreground = additionalStatusMessageForegroundColor;
                    }
            );
        }
        private void ChangeTimeToShowInLastProgramUpdatesCheckTimeMessage(DateTime timeToShowInLastProgramUpdatesCheckTimeMessage)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        string lastCheckTimeString =
                            timeToShowInLastProgramUpdatesCheckTimeMessage.ToString();
                        label_lastProgramUpdatesCheckTimeMessage.Content =
                            LAST_CHECK_TIME_MESSAGE__LAST_CHECK.Replace(
                                "{*}",
                                (!lastCheckTimeString.Equals((new DateTime()).ToString()) ?
                                    lastCheckTimeString :
                                    "Never")
                            );
                    }
            );
        }
        private void ChangeProgressBarValue(double progressBarValue)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        if (progressBarValue < 0)
                        {
                            progressBar_progress.IsIndeterminate = true;
                            progressBar_progress.ChangeValueSmoothly(
                                0,
                                new Duration(new TimeSpan(0, 0, 0, 0))
                            );
                        }
                        else
                        {
                            progressBar_progress.IsIndeterminate = false;
                            progressBar_progress.ChangeValueSmoothly(
                                progressBarValue,
                                new Duration(new TimeSpan(0, 0, 0, 1))
                            );
                        }
                    }
            );
        }
        private void ApplyUpdatedSettings(Settings updatedSettings)
        {
            Settings backupOfSettingsInMemory = App.SettingsHandler.SettingsInMemory;
            App.SettingsHandler.SettingsInMemory = updatedSettings;
            if (!App.SettingsHandler.SettingsInMemory.General.RememberLastProgramListOptions)
            {
                App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringState = false;
                App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringOption =
                    Settings.CachedSettings.ProgramFilteringOption.Unknown;
                App.SettingsHandler.SettingsInMemory.Cached.LastShowHiddenProgramsState = false;
            }
            if (!App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile())
            {
                App.SettingsHandler.SettingsInMemory = backupOfSettingsInMemory;
                DialogUtilities.ShowErrorDialog(
                    ERROR_DIALOG_TITLE__ERROR,
                    ERROR_DIALOG_MESSAGE__FAILED_TO_SAVE_SETTINGS,
                    this
                );
            }
            else
            {
                if (App.SettingsHandler.SettingsInMemory.Appearance.WindowsScalingFactor != backupOfSettingsInMemory.Appearance.WindowsScalingFactor)
                {
                    double newWindowsRenderingScale =
                        WindowUtilities.GetWindowsRenderingScale(App.SettingsHandler);
                    ApplicationUtilities.ChangeRenderingScaleOfAllOpenWindowsAndMoveThemIntoScreenBoundaries(
                        newWindowsRenderingScale
                    );
                }
                IsAutomaticScanningForInstalledProgramsEnabled =
                    App.SettingsHandler.SettingsInMemory.General.EnableScanningForInstalledPrograms;
                if (CurrentError == Error.None)
                {
                    if (!App.SettingsHandler.SettingsInMemory.General.EnableScanningForInstalledPrograms)
                        programDatabase.ConvertAllProgramsToManuallyInstalledPrograms();
                    RefreshListViewAndAllMessages(true);
                }
            }
        }
        private void StartProgramDatabaseUpdatingTask()
        {
            CurrentOperation = Operation.UpdatingProgramDatabase;
            programDatabaseUpdatingCancellableThread = new CancellableThread(
                cancellationToken =>
                    {
                        ThreadingUtilities.RunOnAnotherThread(
                            Dispatcher,
                            () => listView_programs.SelectedItems.Clear()
                        );
                        ChangeStatusMessage(
                            STATUS_MESSAGE__SCANNING_INSTALLED_PROGRAMS,
                            (SolidColorBrush)Application.Current.FindResource(
                                App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH
                            )
                        );
                        ChangeProgressBarValue(-1);
                        if (!closeInQueue && !cancellationToken.IsCancellationRequested)
                        {
                            ProgramsScanAndUpdatesCheckUtilities.ScanForInstalledProgramsAndUpdateProgramDatabase(
                                programDatabase,
                                App.SettingsHandler,
                                cancellationToken
                            );
                        }
                        RefreshListViewAndAllMessages(true);
                        CurrentOperation = Operation.None;
                        ChangeProgressBarValue(-1);
                        if (closeInQueue)
                            PrepareWindowForClosing(true);
                        programDatabaseUpdatingCancellableThread = null;
                    }
            );
            programDatabaseUpdatingCancellableThread.Start();
        }
        private void StartProgramUpdatesCheckTask()
        {
            CurrentOperation = Operation.CheckingForProgramUpdates;
            programUpdatesCheckCancellableThread = new CancellableThread(
                cancellationToken =>
                    {
                        ThreadingUtilities.RunOnAnotherThread(
                            Dispatcher,
                            () => listView_programs.SelectedItems.Clear()
                        );
                        Exception programUpdatesCheckException = null;
                        ChangeStatusMessage(
                            STATUS_MESSAGE__CHECKING_FOR_PROGRAM_UPDATES,
                            (SolidColorBrush)Application.Current.FindResource(
                                App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH
                            )
                        );
                        ChangeProgressBarValue(-1);
                        if (!closeInQueue && !cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                List<Program> programsToCheck = new List<Program>();
                                foreach (ProgramListViewItem programListViewItem in programListViewItems)
                                    if (programListViewItem.UnderlyingProgram.IsUpdateCheckConfigured)
                                        programsToCheck.Add(programListViewItem.UnderlyingProgram);
                                ProgramsScanAndUpdatesCheckUtilities.CheckForProgramUpdatesAndUpdateDatabase(
                                    programsToCheck,
                                    programDatabase,
                                    App.SettingsHandler,
                                    ChangeProgressBarValue,
                                    cancellationToken
                                );
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
                            string errorDialogMessage = "";
                            if (programUpdatesCheckException.GetType().Equals(
                                    typeof(ProgramsScanAndUpdatesCheckUtilities.ProgramUpdatesCheckWasFailedException)
                                ))
                            {
                                errorDialogMessage =
                                    ((ProgramsScanAndUpdatesCheckUtilities.ProgramUpdatesCheckWasFailedException)programUpdatesCheckException)
                                        .GetLongReasonString();
                            }
                            DialogUtilities.ShowErrorDialog(
                                ERROR_DIALOG_TITLE__ERROR,
                                errorDialogMessage,
                                this
                            );
                        }
                        programUpdatesCheckCancellableThread = null;
                    }
            );
            programUpdatesCheckCancellableThread.Start();
        }
        private void CancelOperation(bool queueWindowToBeClosedAfterCancelling)
        {
            if (CurrentOperation != Operation.None)
            {
                CurrentOperation = Operation.CancellingOperation;
                closeInQueue = queueWindowToBeClosedAfterCancelling;
                programDatabaseUpdatingCancellableThread?.RequestCancellation();
                programUpdatesCheckCancellableThread?.RequestCancellation();
                string statusMessage =
                    (queueWindowToBeClosedAfterCancelling ?
                        STATUS_MESSAGE__CANCELLING_AND_CLOSING :
                        STATUS_MESSAGE__CANCELLING);
                ChangeStatusMessage(
                    statusMessage,
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH
                    )
                );
                ChangeProgressBarValue(-1);
            }
        }
        private void PrepareWindowForClosing()
        {
            PrepareWindowForClosing(false);
        }
        private void PrepareWindowForClosing(bool forceCloseApplication)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        ((GridView)listView_programs.View).Columns.CollectionChanged -=
                            OnGridViewColumnsCollectionCollectionChangedEvent;
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
                                if (App.SettingsHandler.SettingsInMemory.General.RememberLastProgramListOptions)
                                {
                                    App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringState =
                                        checkBox_filterProgramList.IsChecked.Value;
                                    App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringOption =
                                        EnumUtilities.GetEnumItemFromHumanReadableString<Settings.CachedSettings.ProgramFilteringOption>(
                                            (string)comboBox_programListFilteringOption.SelectedItem
                                        );
                                    App.SettingsHandler.SettingsInMemory.Cached.LastShowHiddenProgramsState =
                                        checkBox_showHiddenPrograms.IsChecked.Value;
                                }
                                else
                                {
                                    App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringState = false;
                                    App.SettingsHandler.SettingsInMemory.Cached.LastProgramFilteringOption =
                                        Settings.CachedSettings.ProgramFilteringOption.Unknown;
                                    App.SettingsHandler.SettingsInMemory.Cached.LastShowHiddenProgramsState = false;
                                }
                                App.SettingsHandler.SaveSettingsFromMemoryToSettingsFile();
                            }
                            catch { }
                        }
                        if (forceCloseApplication)
                        {
                            if (App.SettingsHandler != null)
                            {
                                App.SettingsHandler.Dispose();
                                App.SettingsHandler = null;
                            }
                            try
                            {
                                foreach (Process chromeDriverProcess in
                                         Process.GetProcessesByName(
                                             Path.GetFileNameWithoutExtension(
                                                 ChromeDriverUtilities.chromeDriverExecutableFilePath
                                             )
                                         ))
                                {
                                    chromeDriverProcess.Kill(true);
                                }
                            }
                            catch { }
                            Process.GetCurrentProcess().Kill();
                        }
                    }
            );
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
