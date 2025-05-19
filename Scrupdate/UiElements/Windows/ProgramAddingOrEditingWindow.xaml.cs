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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Scrupdate.Classes.Objects;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;


namespace Scrupdate.UiElements.Windows
{
    /// <summary>
    /// Interaction logic for ProgramAddingOrEditingWindow.xaml
    /// </summary>
    public partial class ProgramAddingOrEditingWindow : Window, INotifyPropertyChanged
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const int MAX_COUNT_OF_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON = 5;
        private const int MINIMUM_WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_COLUMN_WIDTH = 30;
        private const string WINDOW_TITLE__PROGRAM_ADDING = "Scrupdate - Program Adding";
        private const string WINDOW_TITLE__PROGRAM_EDITING = "Scrupdate - Program Editing [{*}]";
        private const string LAST_PROGRAM_UPDATE_CHECK_STATUS_MESSAGE__LAST_CHECK_WAS_SUCCEEDED = "Last Check Was Succeeded";
        private const string LAST_PROGRAM_UPDATE_CHECK_STATUS_MESSAGE__LAST_CHECK_WAS_FAILED = "Last Check Was Failed (Reason: {*})";
        private const string ERROR_DIALOG_TITLE__ERROR = "Error";
        private const string ERROR_DIALOG_MESSAGE__NO_NAME = "The 'Name' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__INVALID_INSTALLED_VERSION = "The 'Installed Version' Field's Value Is Invalid!";
        private const string ERROR_DIALOG_MESSAGE__NO_WEB_PAGE_URL = "The 'Web Page URL' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__NO_METHOD_OF_VERSION_SEARCH = "The 'Version Search Method' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__NO_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON = "No Web Page Elements to Simulate a Click On!";
        private const string ERROR_DIALOG_MESSAGE__NAME_ALREADY_EXISTS = "A Program with That Name Already Exists!";
        private const string QUESTION_DIALOG_MESSAGE__CONVERT_THE_PROGRAM_TO_A_MANUALLY_ADDED_PROGRAM = "Convert the Program to a Manually-Added Program?\r\n\r\n•  You will need to update the program's information manually,\r\n    every time you install a new version of the program or remove the program.\r\n•  It cannot be undone automatically.";
        private const string WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__COPY_ARGUMENT = "Copy Locating Method Argument";
        private const string WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_UP = "Move Up";
        private const string WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_DOWN = "Move Down";
        private const string WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE = "Remove";
        private const string WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED = "Remove Selected";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum WindowVariation
        {
            Unknown,
            ProgramAddingWindow,
            ProgramEditingWindow
        }
        public enum ListViewItemMovingDirection
        {
            Up,
            Down
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty CurrentWindowVariationProperty = DependencyProperty.Register(
            nameof(CurrentWindowVariation),
            typeof(WindowVariation),
            typeof(ProgramAddingOrEditingWindow),
            new PropertyMetadata(WindowVariation.Unknown)
        );
        public static readonly DependencyProperty LastProgramUpdateCheckConfigurationStatusProperty = DependencyProperty.Register(
            nameof(LastProgramUpdateCheckConfigurationStatus),
            typeof(Program._UpdateCheckConfigurationStatus),
            typeof(ProgramAddingOrEditingWindow),
            new PropertyMetadata(Program._UpdateCheckConfigurationStatus.Unknown)
        );
        private Dictionary<string, Program> programsAlreadyInDatabase;
        private Program programToEdit;
        private Program newOrUpdatedProgram;
        private volatile List<WebPageElementLocatingInstructionListViewItem> locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSize { get; private set; }
        public WindowVariation CurrentWindowVariation
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (WindowVariation)GetValue(CurrentWindowVariationProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(CurrentWindowVariationProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(CurrentWindowVariation))
                            );
                        }
                );
            }
        }
        public Program._UpdateCheckConfigurationStatus LastProgramUpdateCheckConfigurationStatus
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Program._UpdateCheckConfigurationStatus)GetValue(LastProgramUpdateCheckConfigurationStatusProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(LastProgramUpdateCheckConfigurationStatusProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(LastProgramUpdateCheckConfigurationStatus))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ProgramAddingOrEditingWindow(Dictionary<string, Program> programsAlreadyInDatabase) : this(programsAlreadyInDatabase, null) { }
        public ProgramAddingOrEditingWindow(Dictionary<string, Program> programsAlreadyInDatabase,
                                            Program programToEdit)
        {
            this.programsAlreadyInDatabase = programsAlreadyInDatabase;
            this.programToEdit = programToEdit;
            newOrUpdatedProgram = null;
            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn =
                new List<WebPageElementLocatingInstructionListViewItem>();
            InitializeComponent();
            BaseSize = new Size(Width, Height);
            WindowUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSize,
                App.WindowsRenderingScale
            );
            foreach (Program._InstallationScope installationScope in
                     Enum.GetValues(typeof(Program._InstallationScope)))
            {
                comboBox_installedFor.Items.Add(
                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                        installationScope
                    )
                );
            }
            comboBox_installedFor.SelectedItem =
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    Program._InstallationScope.Everyone
                );
            foreach (Program._WebPagePostLoadDelay webPagePostLoadDelay in
                     Enum.GetValues(typeof(Program._WebPagePostLoadDelay)))
            {
                comboBox_webPagePostLoadDelay.Items.Add(
                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                        webPagePostLoadDelay
                    ).Replace(" Ms", "ms")
                );
            }
            comboBox_webPagePostLoadDelay.SelectedItem =
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    Program._WebPagePostLoadDelay.None
                ).Replace(" Ms", "ms");
            ((GridView)listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.View).Columns.CollectionChanged +=
                OnGridViewColumnsCollectionCollectionChangedEvent;
            listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.ItemsSource =
                locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn;
            foreach (WebPageElementLocatingInstruction._LocatingMethod locatingMethod in
                     Enum.GetValues(typeof(WebPageElementLocatingInstruction._LocatingMethod)))
            {
                if (locatingMethod == WebPageElementLocatingInstruction._LocatingMethod.Unspecified)
                    comboBox_webPageElementLocatingMethod.Items.Add("");
                else
                {
                    comboBox_webPageElementLocatingMethod.Items.Add(
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            locatingMethod
                        ).Replace("Id", "ID").Replace("Html", "HTML").Replace("X Path", "XPath")
                    );
                }
            }
            comboBox_webPageElementLocatingMethod.SelectedItem = "";
            foreach (WebPageElementLocatingInstruction._LocatingInterval locatingInterval in
                     Enum.GetValues(typeof(WebPageElementLocatingInstruction._LocatingInterval)))
            {
                if (locatingInterval == WebPageElementLocatingInstruction._LocatingInterval.Unspecified)
                    comboBox_webPageElementLocatingInterval.Items.Add("");
                else
                {
                    comboBox_webPageElementLocatingInterval.Items.Add(
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            locatingInterval
                        ).Replace(" Ms", "ms")
                    );
                }
            }
            comboBox_webPageElementLocatingInterval.SelectedItem = "";
            CalculateWindowDynamicSizeAndResizeWindow();
            if (programToEdit == null)
            {
                CurrentWindowVariation = WindowVariation.ProgramAddingWindow;
                Title = WINDOW_TITLE__PROGRAM_ADDING;
            }
            else
            {
                CurrentWindowVariation = WindowVariation.ProgramEditingWindow;
                Title = WINDOW_TITLE__PROGRAM_EDITING.Replace("{*}", programToEdit.Name);
                ApplyProgramToUiControlsValues(programToEdit);
            }
            button_addOrSave.Focus();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnClosingEvent(object sender, CancelEventArgs e)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    ((GridView)listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.View).Columns.CollectionChanged -=
                        OnGridViewColumnsCollectionCollectionChangedEvent
            );
            Owner?.Activate();
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_addNewWebPageElement)
                AddTypedWebPageElementLocatingInstructionToListView();
            else if (senderButton == button_removeWebPageElement)
                RemoveSelectedWebPageElementLocatingInstructionsFromListView();
            else if (senderButton == button_moveDownWebPageElement)
                MoveSelectedWebPageElementLocatingInstructionInListView(ListViewItemMovingDirection.Down);
            else if (senderButton == button_moveUpWebPageElement)
                MoveSelectedWebPageElementLocatingInstructionInListView(ListViewItemMovingDirection.Up);
            else if (senderButton == button_addOrSave)
            {
                string errorDialogMessage;
                if (!CheckFields(true, true, out errorDialogMessage))
                {
                    DialogUtilities.ShowErrorDialog(
                        ERROR_DIALOG_TITLE__ERROR,
                        errorDialogMessage,
                        this
                    );
                    return;
                }
                if (!textBox_installedVersion.Text.Trim().Equals(""))
                {
                    textBox_installedVersion.Text = VersionUtilities.NormalizeAndTrimVersion(
                        textBox_installedVersion.Text.Trim(),
                        VersionUtilities.MINIMUM_VERSION_SEGMENTS,
                        VersionUtilities.MAXIMUM_VERSION_SEGMENTS
                    );
                }
                Program._InstallationScope installationScope = GetInstallationScope();
                string versionSearchMethodArgument1;
                string versionSearchMethodArgument2;
                Program._VersionSearchMethod versionSearchMethod = GetVersionSearchMethod(
                    out versionSearchMethodArgument1,
                    out versionSearchMethodArgument2
                );
                Program._VersionSearchBehavior versionSearchBehavior = GetVersionSearchBehavior();
                Program._WebPagePostLoadDelay webPagePostLoadDelay = GetWebPagePostLoadDelay();
                List<WebPageElementLocatingInstruction> locatingInstructionsOfWebPageElementsToSimulateAClickOn =
                    GetLocatingInstructionsOfWebPageElementsToSimulateAClickOn();
                if (programToEdit != null &&
                    (checkBox_detectAutomatically.IsChecked == programToEdit.IsAutomaticallyAdded &&
                     textBox_name.Text.Trim().Equals(programToEdit.Name) &&
                     textBox_installedVersion.Text.Trim().Equals(programToEdit.InstalledVersion) &&
                     installationScope == programToEdit.InstallationScope &&
                     checkBox_configureUpdateCheck.IsChecked == programToEdit.IsUpdateCheckConfigured &&
                     textBox_webPageUrl.Text.Trim().Equals(programToEdit.WebPageUrl) &&
                     versionSearchMethod == programToEdit.VersionSearchMethod &&
                     versionSearchMethodArgument1.Equals(programToEdit.VersionSearchMethodArgument1) &&
                     versionSearchMethodArgument2.Equals(programToEdit.VersionSearchMethodArgument2) &&
                     checkBox_treatAStandaloneNumberAsAVersion.IsChecked == programToEdit.TreatAStandaloneNumberAsAVersion &&
                     versionSearchBehavior == programToEdit.VersionSearchBehavior &&
                     webPagePostLoadDelay == programToEdit.WebPagePostLoadDelay &&
                     locatingInstructionsOfWebPageElementsToSimulateAClickOn.SequenceEqual(
                         programToEdit.LocatingInstructionsOfWebPageElementsToSimulateAClickOn
                     )))
                {
                    Close();
                }
                else
                {
                    if (programToEdit != null &&
                        (programToEdit.IsAutomaticallyAdded &&
                         checkBox_detectAutomatically.IsChecked == false))
                    {
                        if (DialogUtilities.ShowQuestionDialog(
                                "",
                                QUESTION_DIALOG_MESSAGE__CONVERT_THE_PROGRAM_TO_A_MANUALLY_ADDED_PROGRAM,
                                this
                            ) == false)
                        {
                            return;
                        }
                    }
                    newOrUpdatedProgram = GetProgramFromUiControlsValues();
                    DialogResult = true;
                    Close();
                }
            }
            else if (senderButton == button_cancel)
                Close();
        }
        private void OnCheckBoxClickEvent(object sender, RoutedEventArgs e)
        {
            CustomCheckBox senderCheckBox = (CustomCheckBox)sender;
            if (senderCheckBox == checkBox_detectAutomatically)
            {
                if (checkBox_detectAutomatically.IsChecked == true)
                {
                    textBox_name.Text = programToEdit.Name;
                    textBox_installedVersion.Text = programToEdit.InstalledVersion;
                    comboBox_installedFor.SelectedItem =
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            programToEdit.InstallationScope
                        );
                }
            }
            else if (senderCheckBox == checkBox_configureUpdateCheck)
            {
                if (checkBox_configureUpdateCheck.IsChecked == false)
                {
                    textBox_webPageUrl.Text = "";
                    textBox_searchWithinTheHtmlElementWithIdParameter.Text = "";
                    textBox_searchWithinTheHtmlElementsThatMatchXPathParameter.Text = "";
                    textBox_searchFromTextWithinTheWebPageParameter.Text = "";
                    textBox_searchUntilTextWithinTheWebPageParameter.Text = "";
                    checkBox_searchFromTextWithinTheWebPage.IsChecked = false;
                    checkBox_searchUntilTextWithinTheWebPage.IsChecked = false;
                    radioButton_searchWithinTheHtmlElementWithId.IsChecked = true;
                    checkBox_treatAStandaloneNumberAsAVersion.IsChecked = false;
                    radioButton_getTheFirstVersionThatIsFound.IsChecked = true;
                    expander_advancedOptions.IsExpanded = false;
                    comboBox_webPagePostLoadDelay.SelectedItem =
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            Program._WebPagePostLoadDelay.None
                        ).Replace(" Ms", "ms");
                    RemoveAllWebPageElementLocatingInstructionsFromListView();
                    comboBox_webPageElementLocatingMethod.SelectedItem = "";
                    checkBox_webPageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
                    textBox_webPageElementLocatingMethodArgument.Text = "";
                    comboBox_webPageElementLocatingInterval.SelectedItem = "";
                    checkBox_simulateWebPageElementClicks.IsChecked = false;
                }
            }
            else if (senderCheckBox == checkBox_searchFromTextWithinTheWebPage)
            {
                if (checkBox_searchFromTextWithinTheWebPage.IsChecked == false)
                    textBox_searchFromTextWithinTheWebPageParameter.Text = "";
            }
            else if (senderCheckBox == checkBox_searchUntilTextWithinTheWebPage)
            {
                if (checkBox_searchUntilTextWithinTheWebPage.IsChecked == false)
                    textBox_searchUntilTextWithinTheWebPageParameter.Text = "";
            }
            else if (senderCheckBox == checkBox_simulateWebPageElementClicks)
            {
                if (checkBox_simulateWebPageElementClicks.IsChecked == false)
                {
                    RemoveAllWebPageElementLocatingInstructionsFromListView();
                    comboBox_webPageElementLocatingMethod.SelectedItem = "";
                    checkBox_webPageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
                    textBox_webPageElementLocatingMethodArgument.Text = "";
                    comboBox_webPageElementLocatingInterval.SelectedItem = "";
                }
            }
        }
        private void OnComboBoxSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            CustomComboBox senderComboBox = (CustomComboBox)sender;
            if (senderComboBox == comboBox_installedFor)
            {
                if (comboBox_installedFor.SelectedIndex == 0)
                    textBox_installedVersion.Text = "";
            }
            else if (senderComboBox == comboBox_webPageElementLocatingMethod)
                checkBox_webPageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
        }
        private void OnRadioButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomRadioButton senderRadioButton = (CustomRadioButton)sender;
            if (senderRadioButton == radioButton_searchWithinTheHtmlElementWithId ||
                senderRadioButton == radioButton_searchWithinTheHtmlElementsThatMatchXPath ||
                senderRadioButton == radioButton_searchGloballyWithinTheWebPage)
            {
                if (senderRadioButton != radioButton_searchWithinTheHtmlElementWithId)
                    textBox_searchWithinTheHtmlElementWithIdParameter.Text = "";
                if (senderRadioButton != radioButton_searchWithinTheHtmlElementsThatMatchXPath)
                    textBox_searchWithinTheHtmlElementsThatMatchXPathParameter.Text = "";
                if (senderRadioButton != radioButton_searchGloballyWithinTheWebPage)
                {
                    textBox_searchFromTextWithinTheWebPageParameter.Text = "";
                    textBox_searchUntilTextWithinTheWebPageParameter.Text = "";
                    checkBox_searchFromTextWithinTheWebPage.IsChecked = false;
                    checkBox_searchUntilTextWithinTheWebPage.IsChecked = false;
                }
            }
        }
        private void OnListViewKeyDownEvent(object sender, KeyEventArgs e)
        {
            ListView senderListView = (ListView)sender;
            if (senderListView == listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn)
                if (e.Key == Key.Delete)
                    if (listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems.Count > 0)
                        RemoveSelectedWebPageElementLocatingInstructionsFromListView();
        }
        private void OnGridViewColumnHeaderPreviewMouseMoveEvent(object sender, MouseEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_webPageElementLocatingInstructionIndex)
                e.Handled = true;
        }
        private void OnGridViewColumnHeaderPreviewMouseDoubleClickEvent(object sender, MouseButtonEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_webPageElementLocatingInstructionIndex)
                e.Handled = true;
        }
        private void OnGridViewColumnHeaderSizeChangedEvent(object sender, SizeChangedEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_webPageElementLocatingMethod ||
                senderGridViewColumnHeader == gridViewColumnHeader_webPageElementLocatingMethodArgument ||
                senderGridViewColumnHeader == gridViewColumnHeader_webPageElementLocatingInterval)
            {
                if (senderGridViewColumnHeader.Column.Width < MINIMUM_WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_COLUMN_WIDTH)
                {
                    senderGridViewColumnHeader.Column.Width =
                        MINIMUM_WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_COLUMN_WIDTH;
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
        private void OnGridViewColumnsCollectionCollectionChangedEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            GridViewColumnCollection senderGridViewColumnCollection = (GridViewColumnCollection)sender;
            senderGridViewColumnCollection.CollectionChanged -= OnGridViewColumnsCollectionCollectionChangedEvent;
            senderGridViewColumnCollection.Remove(gridViewColumn_webPageElementLocatingInstructionIndex);
            senderGridViewColumnCollection.Insert(0, gridViewColumn_webPageElementLocatingInstructionIndex);
            senderGridViewColumnCollection.CollectionChanged += OnGridViewColumnsCollectionCollectionChangedEvent;
        }
        private void OnListViewItemContextMenuOpeningEvent(object sender, ContextMenuEventArgs e)
        {
            ListViewItem senderListViewItem = (ListViewItem)sender;
            List<object> menuItems = new List<object>();
            int selectedListViewItemsCount =
                listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems.Count;
            if (selectedListViewItemsCount == 1)
            {
                menuItems.Add(new MenuItem() { Header = WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__COPY_ARGUMENT });
                menuItems.Add(new MenuItem() { Header = WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_UP });
                menuItems.Add(new MenuItem() { Header = WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_DOWN });
            }
            menuItems.Add(
                new MenuItem()
                {
                    Header =
                        (selectedListViewItemsCount > 1 ?
                            WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED :
                            WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE)
                }
            );
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
            if (senderMenuItem.Header.Equals(WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__COPY_ARGUMENT))
            {
                WebPageElementLocatingInstructionListViewItem selectedWebPageElementLocatingInstructionListViewItem =
                    (WebPageElementLocatingInstructionListViewItem)listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems[0];
                Clipboard.SetText(
                    selectedWebPageElementLocatingInstructionListViewItem.UnderlyingWebPageElementLocatingInstruction.MethodArgument
                );
                Clipboard.Flush();
            }
            else if (senderMenuItem.Header.Equals(WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_UP))
                MoveSelectedWebPageElementLocatingInstructionInListView(ListViewItemMovingDirection.Up);
            else if (senderMenuItem.Header.Equals(WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_DOWN))
                MoveSelectedWebPageElementLocatingInstructionInListView(ListViewItemMovingDirection.Down);
            else if (senderMenuItem.Header.Equals(WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE) ||
                     senderMenuItem.Header.Equals(WEB_PAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED))
            {
                RemoveSelectedWebPageElementLocatingInstructionsFromListView();
            }
        }
        private void OnComboBoxKeyDownEvent(object sender, KeyEventArgs e)
        {
            CustomComboBox senderComboBox = (CustomComboBox)sender;
            if (senderComboBox == comboBox_webPageElementLocatingMethod ||
                senderComboBox == comboBox_webPageElementLocatingInterval)
            {
                if (e.Key == Key.Enter)
                    AddTypedWebPageElementLocatingInstructionToListView();
            }
        }
        private void OnCheckBoxKeyDownEvent(object sender, KeyEventArgs e)
        {
            CustomCheckBox senderCheckBox = (CustomCheckBox)sender;
            if (senderCheckBox == checkBox_webPageElementLocatingMethodArgumentMatchExactText)
                if (e.Key == Key.Enter)
                    AddTypedWebPageElementLocatingInstructionToListView();
        }
        private void OnTextBoxTextChangedEvent(object sender, TextChangedEventArgs e)
        {
            CustomTextBox senderTextBox = (CustomTextBox)sender;
            if (senderTextBox == textBox_name ||
                senderTextBox == textBox_installedVersion ||
                senderTextBox == textBox_webPageUrl ||
                senderTextBox == textBox_searchWithinTheHtmlElementWithIdParameter ||
                senderTextBox == textBox_searchWithinTheHtmlElementsThatMatchXPathParameter ||
                senderTextBox == textBox_searchFromTextWithinTheWebPageParameter ||
                senderTextBox == textBox_searchUntilTextWithinTheWebPageParameter ||
                senderTextBox == textBox_webPageElementLocatingMethodArgument)
            {
                if (Array.TrueForAll(
                        senderTextBox.Text.ToCharArray(),
                        c => char.IsWhiteSpace(c)
                    ))
                {
                    senderTextBox.Text = "";
                }
            }
        }
        private void OnTextBoxKeyDownEvent(object sender, KeyEventArgs e)
        {
            CustomTextBox senderTextBox = (CustomTextBox)sender;
            if (senderTextBox == textBox_webPageElementLocatingMethodArgument)
                if (e.Key == Key.Enter)
                    AddTypedWebPageElementLocatingInstructionToListView();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private Program._InstallationScope GetInstallationScope()
        {
            return EnumUtilities.GetEnumItemFromHumanReadableString<Program._InstallationScope>(
                (string)comboBox_installedFor.SelectedItem
            );
        }
        private Program._VersionSearchMethod GetVersionSearchMethod(out string versionSearchMethodArgument1,
                                                                    out string versionSearchMethodArgument2)
        {
            versionSearchMethodArgument1 = null;
            versionSearchMethodArgument2 = null;
            Program._VersionSearchMethod versionSearchMethod =
                Program._VersionSearchMethod.Unknown;
            if (checkBox_configureUpdateCheck.IsChecked == true)
            {
                if (radioButton_searchWithinTheHtmlElementWithId.IsChecked == true)
                {
                    versionSearchMethod =
                        Program._VersionSearchMethod.SearchWithinTheHtmlElementWithId;
                    versionSearchMethodArgument1 =
                        textBox_searchWithinTheHtmlElementWithIdParameter.Text.Trim();
                    versionSearchMethodArgument2 = "";
                }
                else if (radioButton_searchWithinTheHtmlElementsThatMatchXPath.IsChecked == true)
                {
                    versionSearchMethod =
                        Program._VersionSearchMethod.SearchWithinTheHtmlElementsThatMatchXPath;
                    versionSearchMethodArgument1 =
                        textBox_searchWithinTheHtmlElementsThatMatchXPathParameter.Text.Trim();
                    versionSearchMethodArgument2 = "";
                }
                else if (radioButton_searchGloballyWithinTheWebPage.IsChecked == true)
                {
                    if (checkBox_searchFromTextWithinTheWebPage.IsChecked == true &&
                        checkBox_searchUntilTextWithinTheWebPage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyFromTextUntilTextWithinTheWebPage;
                        versionSearchMethodArgument1 =
                            textBox_searchFromTextWithinTheWebPageParameter.Text.Trim();
                        versionSearchMethodArgument2 =
                            textBox_searchUntilTextWithinTheWebPageParameter.Text.Trim();
                    }
                    else if (checkBox_searchFromTextWithinTheWebPage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyFromTextWithinTheWebPage;
                        versionSearchMethodArgument1 =
                            textBox_searchFromTextWithinTheWebPageParameter.Text.Trim();
                        versionSearchMethodArgument2 = "";
                    }
                    else if (checkBox_searchUntilTextWithinTheWebPage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyUntilTextWithinTheWebPage;
                        versionSearchMethodArgument1 =
                            textBox_searchUntilTextWithinTheWebPageParameter.Text.Trim();
                        versionSearchMethodArgument2 = "";
                    }
                    else
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyWithinTheWebPage;
                        versionSearchMethodArgument1 = "";
                        versionSearchMethodArgument2 = "";
                    }
                }
            }
            else
            {
                versionSearchMethodArgument1 = "";
                versionSearchMethodArgument2 = "";
            }
            return versionSearchMethod;
        }
        private Program._VersionSearchBehavior GetVersionSearchBehavior()
        {
            Program._VersionSearchBehavior versionSearchBehavior =
                Program._VersionSearchBehavior.Unknown;
            if (checkBox_configureUpdateCheck.IsChecked == true)
            {
                if (radioButton_getTheFirstVersionThatIsFound.IsChecked == true)
                {
                    versionSearchBehavior =
                        Program._VersionSearchBehavior.GetTheFirstVersionThatIsFound;
                }
                else if (radioButton_getTheFirstVersionThatIsFoundFromTheEnd.IsChecked == true)
                {
                    versionSearchBehavior =
                        Program._VersionSearchBehavior.GetTheFirstVersionThatIsFoundFromTheEnd;
                }
                else if (radioButton_getTheLatestVersionFromAllTheVersionsThatAreFound.IsChecked == true)
                {
                    versionSearchBehavior =
                        Program._VersionSearchBehavior.GetTheLatestVersionFromAllTheVersionsThatAreFound;
                }
            }
            return versionSearchBehavior;
        }
        private Program._WebPagePostLoadDelay GetWebPagePostLoadDelay()
        {
            Program._WebPagePostLoadDelay webPagePostLoadDelay =
                Program._WebPagePostLoadDelay.None;
            if (checkBox_configureUpdateCheck.IsChecked == true)
            {
                webPagePostLoadDelay =
                    EnumUtilities.GetEnumItemFromHumanReadableString<Program._WebPagePostLoadDelay>(
                        ((string)comboBox_webPagePostLoadDelay.SelectedItem)
                            .Replace("ms", "Ms")
                    );
            }
            return webPagePostLoadDelay;
        }
        private List<WebPageElementLocatingInstruction> GetLocatingInstructionsOfWebPageElementsToSimulateAClickOn()
        {
            List<WebPageElementLocatingInstruction> locatingInstructionsOfWebPageElementsToSimulateAClickOn =
                new List<WebPageElementLocatingInstruction>();
            foreach (WebPageElementLocatingInstructionListViewItem webPageElementLocatingInstructionListViewItem in
                     listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items)
            {
                locatingInstructionsOfWebPageElementsToSimulateAClickOn.Add(
                    webPageElementLocatingInstructionListViewItem.UnderlyingWebPageElementLocatingInstruction
                );
            }
            return locatingInstructionsOfWebPageElementsToSimulateAClickOn;
        }
        private void RefreshListView()
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items.Refresh();
                        grid_webElementAdditionField.IsEnabled =
                            (listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items.Count < MAX_COUNT_OF_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON);
                    }
            );
        }
        private void AddTypedWebPageElementLocatingInstructionToListView()
        {
            if (listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items.Count < MAX_COUNT_OF_LOCATING_INSTRUCTIONS_OF_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON &&
                (!comboBox_webPageElementLocatingMethod.SelectedItem.Equals("") &&
                 textBox_webPageElementLocatingMethodArgument.Text.Trim().Length > 0 &&
                 !comboBox_webPageElementLocatingInterval.SelectedItem.Equals("")))
            {
                WebPageElementLocatingInstruction._LocatingMethod webPageElementLocatingMethod =
                    EnumUtilities.GetEnumItemFromHumanReadableString<WebPageElementLocatingInstruction._LocatingMethod>(
                        ((string)comboBox_webPageElementLocatingMethod.SelectedItem)
                            .Replace("ID", "Id")
                            .Replace("HTML", "Html")
                    );
                WebPageElementLocatingInstruction._LocatingInterval webPageElementLocatingInterval =
                    EnumUtilities.GetEnumItemFromHumanReadableString<WebPageElementLocatingInstruction._LocatingInterval>(
                        ((string)comboBox_webPageElementLocatingInterval.SelectedItem)
                            .Replace("ms", "Ms")
                    );
                WebPageElementLocatingInstruction typedWebPageElementLocatingInstructionToListView =
                    new WebPageElementLocatingInstruction(
                        webPageElementLocatingMethod,
                        textBox_webPageElementLocatingMethodArgument.Text.Trim(),
                        (bool)checkBox_webPageElementLocatingMethodArgumentMatchExactText.IsChecked,
                        webPageElementLocatingInterval
                    );
                locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.Add(
                    new WebPageElementLocatingInstructionListViewItem(
                        listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items.Count + 1,
                        typedWebPageElementLocatingInstructionToListView
                    )
                );
                RefreshListView();
                listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedIndex = -1;
                listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.ScrollIntoView(
                    listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items[
                        listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items.Count - 1
                    ]
                );
                comboBox_webPageElementLocatingMethod.SelectedItem = "";
                checkBox_webPageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
                textBox_webPageElementLocatingMethodArgument.Text = "";
                comboBox_webPageElementLocatingInterval.SelectedItem = "";
            }
        }
        private void MoveSelectedWebPageElementLocatingInstructionInListView(ListViewItemMovingDirection movingDirection)
        {
            if (listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems.Count == 1)
            {
                WebPageElementLocatingInstructionListViewItem selectedWebPageElementLocatingInstructionListViewItem =
                    (WebPageElementLocatingInstructionListViewItem)listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems[0];
                int indexOfSelectedWebPageElementLocatingInstructionListViewItem =
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.IndexOf(
                        selectedWebPageElementLocatingInstructionListViewItem
                    );
                switch (movingDirection)
                {
                    case ListViewItemMovingDirection.Up:
                        if (indexOfSelectedWebPageElementLocatingInstructionListViewItem > 0)
                        {
                            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                indexOfSelectedWebPageElementLocatingInstructionListViewItem
                            ].WebPageElementLocatingInstructionIndex--;
                            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                indexOfSelectedWebPageElementLocatingInstructionListViewItem - 1
                            ].WebPageElementLocatingInstructionIndex++;
                            WebPageElementLocatingInstructionListViewItem tempWebPageElementLocatingInstructionListViewItem =
                                locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                    indexOfSelectedWebPageElementLocatingInstructionListViewItem
                                ];
                            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                indexOfSelectedWebPageElementLocatingInstructionListViewItem
                            ] =
                                locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                    indexOfSelectedWebPageElementLocatingInstructionListViewItem - 1
                                ];
                            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                indexOfSelectedWebPageElementLocatingInstructionListViewItem - 1
                            ] =
                                tempWebPageElementLocatingInstructionListViewItem;
                        }
                        break;
                    case ListViewItemMovingDirection.Down:
                        if (indexOfSelectedWebPageElementLocatingInstructionListViewItem < locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.Count - 1)
                        {
                            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                indexOfSelectedWebPageElementLocatingInstructionListViewItem
                            ].WebPageElementLocatingInstructionIndex++;
                            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                indexOfSelectedWebPageElementLocatingInstructionListViewItem + 1
                            ].WebPageElementLocatingInstructionIndex--;
                            WebPageElementLocatingInstructionListViewItem tempWebPageElementLocatingInstructionListViewItem =
                                locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                    indexOfSelectedWebPageElementLocatingInstructionListViewItem
                                ];
                            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                indexOfSelectedWebPageElementLocatingInstructionListViewItem
                            ] =
                                locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                    indexOfSelectedWebPageElementLocatingInstructionListViewItem + 1
                                ];
                            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                                indexOfSelectedWebPageElementLocatingInstructionListViewItem + 1
                            ] =
                                tempWebPageElementLocatingInstructionListViewItem;
                        }
                        break;
                }
                RefreshListView();
                listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.ScrollIntoView(
                    listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems[0]
                );
            }
        }
        private void RemoveSelectedWebPageElementLocatingInstructionsFromListView()
        {
            if (listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems.Count > 0)
            {
                foreach (WebPageElementLocatingInstructionListViewItem selectedWebPageElementLocatingInstructionListViewItem in
                         listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems)
                {
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.Remove(
                        selectedWebPageElementLocatingInstructionListViewItem
                    );
                }
                for (int i = 0; i < locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.Count; i++)
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[i].WebPageElementLocatingInstructionIndex = i + 1;
                RefreshListView();
            }
        }
        private void RemoveAllWebPageElementLocatingInstructionsFromListView()
        {
            locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.Clear();
            RefreshListView();
        }
        private void ApplyProgramToUiControlsValues(Program program)
        {
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
            checkBox_detectAutomatically.IsEnabled = program.IsAutomaticallyAdded;
            checkBox_detectAutomatically.IsChecked = program.IsAutomaticallyAdded;
            textBox_name.Text = program.Name;
            textBox_installedVersion.Text = program.InstalledVersion;
            label_latestVersion.Content =
                ((!program.SkippedVersion.Equals("") &&
                  !VersionUtilities.IsVersionNewer(
                      program.LatestVersion,
                      program.SkippedVersion
                  )) ?
                    (new StringBuilder(program.LatestVersion.Length + 10))
                        .Append(program.LatestVersion)
                        .Append(" [Skipped]")
                        .ToString() :
                    program.LatestVersion);
            label_latestVersion.Foreground =
                ((thereIsANewerVersion == true &&
                  (program.SkippedVersion.Equals("") ||
                   VersionUtilities.IsVersionNewer(
                       program.LatestVersion,
                       program.SkippedVersion
                   ))) ?
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH
                    ) :
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH
                    ));
            comboBox_installedFor.SelectedItem =
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    program.InstallationScope
                );
            checkBox_configureUpdateCheck.IsChecked = false;
            textBox_webPageUrl.Text = "";
            if (program.IsUpdateCheckConfigured)
            {
                checkBox_configureUpdateCheck.IsChecked = true;
                textBox_webPageUrl.Text = program.WebPageUrl;
                switch (program.VersionSearchMethod)
                {
                    case Program._VersionSearchMethod.SearchWithinTheHtmlElementWithId:
                        radioButton_searchWithinTheHtmlElementWithId.IsChecked = true;
                        textBox_searchWithinTheHtmlElementWithIdParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchWithinTheHtmlElementsThatMatchXPath:
                        radioButton_searchWithinTheHtmlElementsThatMatchXPath.IsChecked = true;
                        textBox_searchWithinTheHtmlElementsThatMatchXPathParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyWithinTheWebPage:
                        radioButton_searchGloballyWithinTheWebPage.IsChecked = true;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyFromTextWithinTheWebPage:
                        radioButton_searchGloballyWithinTheWebPage.IsChecked = true;
                        checkBox_searchFromTextWithinTheWebPage.IsChecked = true;
                        textBox_searchFromTextWithinTheWebPageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyUntilTextWithinTheWebPage:
                        radioButton_searchGloballyWithinTheWebPage.IsChecked = true;
                        checkBox_searchUntilTextWithinTheWebPage.IsChecked = true;
                        textBox_searchUntilTextWithinTheWebPageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyFromTextUntilTextWithinTheWebPage:
                        radioButton_searchGloballyWithinTheWebPage.IsChecked = true;
                        checkBox_searchFromTextWithinTheWebPage.IsChecked = true;
                        checkBox_searchUntilTextWithinTheWebPage.IsChecked = true;
                        textBox_searchFromTextWithinTheWebPageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        textBox_searchUntilTextWithinTheWebPageParameter.Text =
                            program.VersionSearchMethodArgument2;
                        break;
                }
                checkBox_treatAStandaloneNumberAsAVersion.IsChecked =
                    program.TreatAStandaloneNumberAsAVersion;
                switch (program.VersionSearchBehavior)
                {
                    case Program._VersionSearchBehavior.GetTheFirstVersionThatIsFound:
                        radioButton_getTheFirstVersionThatIsFound.IsChecked = true;
                        break;
                    case Program._VersionSearchBehavior.GetTheFirstVersionThatIsFoundFromTheEnd:
                        radioButton_getTheFirstVersionThatIsFoundFromTheEnd.IsChecked = true;
                        break;
                    case Program._VersionSearchBehavior.GetTheLatestVersionFromAllTheVersionsThatAreFound:
                        radioButton_getTheLatestVersionFromAllTheVersionsThatAreFound.IsChecked = true;
                        break;
                }
                comboBox_webPagePostLoadDelay.SelectedItem =
                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                        program.WebPagePostLoadDelay
                    ).Replace(" Ms", "ms");
                if (program.LocatingInstructionsOfWebPageElementsToSimulateAClickOn.Count > 0)
                {
                    checkBox_simulateWebPageElementClicks.IsChecked = true;
                    for (int i = 0; i < program.LocatingInstructionsOfWebPageElementsToSimulateAClickOn.Count; i++)
                    {
                        locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.Add(
                            new WebPageElementLocatingInstructionListViewItem(
                                i + 1,
                                program.LocatingInstructionsOfWebPageElementsToSimulateAClickOn[i]
                            )
                        );
                    }
                    RefreshListView();
                }
                ChangeLastProgramUpdateCheckConfigurationStatusMessageAndIconAccordingToError(
                    (program.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Unknown ?
                        program.UpdateCheckConfigurationError :
                        null)
                );
            }
        }
        private Program GetProgramFromUiControlsValues()
        {
            string latestVersion = "";
            Program._InstallationScope installationScope = GetInstallationScope();
            string versionSearchMethodArgument1;
            string versionSearchMethodArgument2;
            Program._VersionSearchMethod versionSearchMethod = GetVersionSearchMethod(
                out versionSearchMethodArgument1,
                out versionSearchMethodArgument2
            );
            Program._VersionSearchBehavior versionSearchBehavior = GetVersionSearchBehavior();
            Program._WebPagePostLoadDelay webPagePostLoadDelay = GetWebPagePostLoadDelay();
            List<WebPageElementLocatingInstruction> locatingInstructionsOfWebPageElementsToSimulateAClickOn =
                GetLocatingInstructionsOfWebPageElementsToSimulateAClickOn();
            Program._UpdateCheckConfigurationStatus updateCheckConfigurationStatus =
                Program._UpdateCheckConfigurationStatus.Unknown;
            Program._UpdateCheckConfigurationError updateCheckConfigurationError =
                Program._UpdateCheckConfigurationError.None;
            string skippedVersion = "";
            if (programToEdit != null)
            {
                if (checkBox_configureUpdateCheck.IsChecked == programToEdit.IsUpdateCheckConfigured &&
                    textBox_webPageUrl.Text.Trim().Equals(programToEdit.WebPageUrl) &&
                    versionSearchMethod == programToEdit.VersionSearchMethod &&
                    versionSearchMethodArgument1.Equals(programToEdit.VersionSearchMethodArgument1) &&
                    versionSearchMethodArgument2.Equals(programToEdit.VersionSearchMethodArgument2) &&
                    checkBox_treatAStandaloneNumberAsAVersion.IsChecked == programToEdit.TreatAStandaloneNumberAsAVersion &&
                    versionSearchBehavior == programToEdit.VersionSearchBehavior &&
                    webPagePostLoadDelay == programToEdit.WebPagePostLoadDelay &&
                    locatingInstructionsOfWebPageElementsToSimulateAClickOn.SequenceEqual(
                        programToEdit.LocatingInstructionsOfWebPageElementsToSimulateAClickOn
                    ))
                {
                    latestVersion = programToEdit.LatestVersion;
                    updateCheckConfigurationStatus = programToEdit.UpdateCheckConfigurationStatus;
                    updateCheckConfigurationError = programToEdit.UpdateCheckConfigurationError;
                }
                if (!programToEdit.SkippedVersion.Equals("") &&
                    (textBox_installedVersion.Text.Trim().Equals("") ||
                     VersionUtilities.IsVersionNewer(
                         programToEdit.SkippedVersion,
                         textBox_installedVersion.Text.Trim()
                     )))
                {
                    skippedVersion = programToEdit.SkippedVersion;
                }
            }
            return new Program(
                textBox_name.Text.Trim(),
                textBox_installedVersion.Text.Trim(),
                latestVersion,
                installationScope,
                (bool)checkBox_detectAutomatically.IsChecked,
                (bool)checkBox_configureUpdateCheck.IsChecked,
                textBox_webPageUrl.Text.Trim(),
                versionSearchMethod,
                versionSearchMethodArgument1,
                versionSearchMethodArgument2,
                (bool)checkBox_treatAStandaloneNumberAsAVersion.IsChecked,
                versionSearchBehavior,
                webPagePostLoadDelay,
                locatingInstructionsOfWebPageElementsToSimulateAClickOn,
                updateCheckConfigurationStatus,
                updateCheckConfigurationError,
                skippedVersion,
                (programToEdit != null ? programToEdit.IsHidden : false)
            );
        }
        private void ChangeLastProgramUpdateCheckConfigurationStatusMessageAndIconAccordingToError(Program._UpdateCheckConfigurationError? updateCheckConfigurationError)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        if (updateCheckConfigurationError == null)
                        {
                            LastProgramUpdateCheckConfigurationStatus =
                                Program._UpdateCheckConfigurationStatus.Unknown;
                            label_lastProgramUpdateCheckConfigurationStatusMessage.Content = "";
                        }
                        else if (updateCheckConfigurationError == Program._UpdateCheckConfigurationError.None)
                        {
                            LastProgramUpdateCheckConfigurationStatus =
                                Program._UpdateCheckConfigurationStatus.Valid;
                            label_lastProgramUpdateCheckConfigurationStatusMessage.Content =
                                LAST_PROGRAM_UPDATE_CHECK_STATUS_MESSAGE__LAST_CHECK_WAS_SUCCEEDED;
                        }
                        else
                        {
                            LastProgramUpdateCheckConfigurationStatus =
                                Program._UpdateCheckConfigurationStatus.Invalid;
                            label_lastProgramUpdateCheckConfigurationStatusMessage.Content =
                                LAST_PROGRAM_UPDATE_CHECK_STATUS_MESSAGE__LAST_CHECK_WAS_FAILED.Replace(
                                    "{*}",
                                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                                        updateCheckConfigurationError
                                    )
                                );
                        }
                    }
            );
        }
        public void CalculateWindowDynamicSizeAndResizeWindow()
        {
            double calculatedWindowHeight = Math.Floor(
                (programToEdit == null ?
                    628 :
                    (programToEdit.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Valid ?
                        658 :
                        688)) * App.WindowsRenderingScale
            );
            calculatedWindowHeight +=
                SystemParameters.WindowNonClientFrameThickness.Top +
                SystemParameters.WindowNonClientFrameThickness.Bottom +
                SystemParameters.WindowResizeBorderThickness.Top +
                SystemParameters.WindowResizeBorderThickness.Bottom;
            MinHeight = calculatedWindowHeight;
            Height = calculatedWindowHeight;
            MaxHeight = calculatedWindowHeight;
        }
        private bool CheckFields(bool checkProgramProperties,
                                 bool checkProgramUpdateCheckConfiguration,
                                 out string errorDialogMessage)
        {
            errorDialogMessage = null;
            if (checkProgramProperties)
            {
                if (textBox_name.Text.Trim().Equals(""))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_NAME;
                    return false;
                }
                if (!textBox_installedVersion.Text.Trim().Equals("") &&
                    !VersionUtilities.IsVersion(
                        textBox_installedVersion.Text.Trim(),
                        VersionUtilities.VersionValidation.ValidateVersionSegmentsCountButTreatAStandaloneNumberAsAVersion
                    ))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__INVALID_INSTALLED_VERSION;
                    return false;
                }
            }
            if (checkProgramUpdateCheckConfiguration)
            {
                if (checkBox_configureUpdateCheck.IsChecked == true &&
                    textBox_webPageUrl.Text.Trim().Equals(""))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_WEB_PAGE_URL;
                    return false;
                }
                if (checkBox_configureUpdateCheck.IsChecked == true &&
                    ((radioButton_searchWithinTheHtmlElementWithId.IsChecked == true &&
                      textBox_searchWithinTheHtmlElementWithIdParameter.Text.Trim().Equals("")) ||
                     (radioButton_searchWithinTheHtmlElementsThatMatchXPath.IsChecked == true &&
                      textBox_searchWithinTheHtmlElementsThatMatchXPathParameter.Text.Trim().Equals("")) ||
                     (checkBox_searchFromTextWithinTheWebPage.IsChecked == true &&
                      textBox_searchFromTextWithinTheWebPageParameter.Text.Trim().Equals("")) ||
                     (checkBox_searchUntilTextWithinTheWebPage.IsChecked == true &&
                      textBox_searchUntilTextWithinTheWebPageParameter.Text.Trim().Equals(""))))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_METHOD_OF_VERSION_SEARCH;
                    return false;
                }
                if (checkBox_configureUpdateCheck.IsChecked == true &&
                    (checkBox_simulateWebPageElementClicks.IsChecked == true &&
                     listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items.Count == 0))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_WEB_PAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON;
                    return false;
                }
            }
            if (checkProgramProperties)
            {
                if ((programToEdit == null &&
                     programsAlreadyInDatabase.ContainsKey(textBox_name.Text.Trim())) ||
                    (programToEdit != null &&
                     !textBox_name.Text.Trim().Equals(programToEdit.Name) &&
                     programsAlreadyInDatabase.ContainsKey(textBox_name.Text.Trim())))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NAME_ALREADY_EXISTS;
                    return false;
                }
            }
            return true;
        }
        public Program GetNewOrUpdatedProgram()
        {
            return newOrUpdatedProgram;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
