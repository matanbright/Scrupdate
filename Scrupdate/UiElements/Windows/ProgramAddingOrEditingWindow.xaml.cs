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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
    public partial class ProgramAddingOrEditingWindow : Window, INotifyPropertyChanged
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const int MAX_COUNT_OF_LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON = 5;
        private const int MINIMUM_WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_COLUMN_WIDTH = 30;
        private const string WINDOW_TITLE__PROGRAM_ADDING = "Scrupdate - Program Adding";
        private const string WINDOW_TITLE__PROGRAM_EDITING = "Scrupdate - Program Editing [{*}]";
        private const string LAST_PROGRAM_UPDATE_CHECK_STATUS_MESSAGE__LAST_CHECK_WAS_SUCCEEDED = "Last Check Was Succeeded";
        private const string LAST_PROGRAM_UPDATE_CHECK_STATUS_MESSAGE__LAST_CHECK_WAS_FAILED = "Last Check Was Failed (Reason: {*})";
        private const string ERROR_DIALOG_MESSAGE__UNKNOWN = "Unknown!";
        private const string ERROR_DIALOG_MESSAGE__NO_NAME = "The 'Name' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__INVALID_INSTALLED_VERSION = "The 'Installed Version' Field's Value Is Invalid!";
        private const string ERROR_DIALOG_MESSAGE__NO_WEBPAGE_URL = "The 'Webpage URL' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__NO_METHOD_OF_VERSION_SEARCH = "The 'Version Search Method' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__NO_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON = "No Webpage Elements to Simulate a Click On!";
        private const string ERROR_DIALOG_MESSAGE__NAME_ALREADY_EXISTS = "A Program with That Name Already Exists!";
        private const string WARNING_DIALOG_MESSAGE__CHECK_WAS_FAILED = "Check Was Failed\r\n(Reason: {*})";
        private const string INFORMATION_DIALOG_MESSAGE__FOUND_VERSION = "Found Version: {*}";
        private const string QUESTION_DIALOG_MESSAGE__CONVERT_THE_PROGRAM_TO_A_MANUALLY_ADDED_PROGRAM = "Convert the Program to a Manually-Added Program?\r\n\r\n•  You will need to update the program's information manually,\r\n    every time you install a new version of the program or remove the program.\r\n•  It cannot be undone automatically.";
        private const string WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__COPY_ARGUMENT = "Copy Locating Method Argument";
        private const string WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_UP = "Move Up";
        private const string WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_DOWN = "Move Down";
        private const string WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE = "Remove";
        private const string WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED = "Remove Selected";
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
        private volatile List<WebpageElementLocatingInstructionListViewItem> locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn;
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
            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn =
                new List<WebpageElementLocatingInstructionListViewItem>();
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
            foreach (Program._WebpagePostLoadDelay webpagePostLoadDelay in
                     Enum.GetValues(typeof(Program._WebpagePostLoadDelay)))
            {
                comboBox_webpagePostLoadDelay.Items.Add(
                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                        webpagePostLoadDelay
                    ).Replace(" Ms", "ms")
                );
            }
            comboBox_webpagePostLoadDelay.SelectedItem =
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    Program._WebpagePostLoadDelay.None
                ).Replace(" Ms", "ms");
            ((GridView)listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.View).Columns.CollectionChanged +=
                OnGridViewColumnsCollectionCollectionChangedEvent;
            listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.ItemsSource =
                locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn;
            foreach (WebpageElementLocatingInstruction._LocatingMethod locatingMethod in
                     Enum.GetValues(typeof(WebpageElementLocatingInstruction._LocatingMethod)))
            {
                if (locatingMethod == WebpageElementLocatingInstruction._LocatingMethod.Unspecified)
                    comboBox_webpageElementLocatingMethod.Items.Add("");
                else
                {
                    comboBox_webpageElementLocatingMethod.Items.Add(
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            locatingMethod
                        ).Replace("Id", "ID").Replace("Html", "HTML").Replace("X Path", "XPath")
                    );
                }
            }
            comboBox_webpageElementLocatingMethod.SelectedItem = "";
            foreach (WebpageElementLocatingInstruction._LocatingInterval locatingInterval in
                     Enum.GetValues(typeof(WebpageElementLocatingInstruction._LocatingInterval)))
            {
                if (locatingInterval == WebpageElementLocatingInstruction._LocatingInterval.Unspecified)
                    comboBox_webpageElementLocatingInterval.Items.Add("");
                else
                {
                    comboBox_webpageElementLocatingInterval.Items.Add(
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            locatingInterval
                        ).Replace(" Ms", "ms")
                    );
                }
            }
            comboBox_webpageElementLocatingInterval.SelectedItem = "";
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
                    ((GridView)listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.View).Columns.CollectionChanged -=
                        OnGridViewColumnsCollectionCollectionChangedEvent
            );
            Owner?.Activate();
        }
        private void OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_addNewWebpageElement)
                AddTypedWebpageElementLocatingInstructionToListView();
            else if (senderButton == button_removeWebpageElement)
                RemoveSelectedWebpageElementLocatingInstructionsFromListView();
            else if (senderButton == button_moveDownWebpageElement)
                MoveSelectedWebpageElementLocatingInstructionInListView(ListViewItemMovingDirection.Down);
            else if (senderButton == button_moveUpWebpageElement)
                MoveSelectedWebpageElementLocatingInstructionInListView(ListViewItemMovingDirection.Up);
            else if (senderButton == button_addOrSave)
            {
                string errorDialogMessage;
                if (!CheckFields(true, true, out errorDialogMessage))
                {
                    DialogUtilities.ShowErrorDialog(
                        "",
                        errorDialogMessage,
                        this
                    );
                    return;
                }
                string installedVersion = textBox_installedVersion.Text.Trim();
                if (!installedVersion.Equals(""))
                {
                    installedVersion = VersionUtilities.NormalizeAndTrimVersion(
                        installedVersion,
                        VersionUtilities.MINIMUM_VERSION_SEGMENTS,
                        VersionUtilities.MAXIMUM_VERSION_SEGMENTS
                    );
                    textBox_installedVersion.Text = installedVersion;
                }
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
                    textBox_webpageUrl.Text = "";
                    textBox_searchWithinTheHtmlElementWithIdParameter.Text = "";
                    textBox_searchWithinTheHtmlElementsThatMatchXPathParameter.Text = "";
                    textBox_searchFromTextWithinTheWebpageParameter.Text = "";
                    textBox_searchUntilTextWithinTheWebpageParameter.Text = "";
                    checkBox_searchFromTextWithinTheWebpage.IsChecked = false;
                    checkBox_searchUntilTextWithinTheWebpage.IsChecked = false;
                    radioButton_searchWithinTheHtmlElementWithId.IsChecked = true;
                    checkBox_treatAStandaloneNumberAsAVersion.IsChecked = false;
                    radioButton_getTheFirstVersionThatIsFound.IsChecked = true;
                    expander_advancedOptions.IsExpanded = false;
                    comboBox_webpagePostLoadDelay.SelectedItem =
                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                            Program._WebpagePostLoadDelay.None
                        ).Replace(" Ms", "ms");
                    RemoveAllWebpageElementLocatingInstructionsFromListView();
                    comboBox_webpageElementLocatingMethod.SelectedItem = "";
                    checkBox_webpageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
                    textBox_webpageElementLocatingMethodArgument.Text = "";
                    comboBox_webpageElementLocatingInterval.SelectedItem = "";
                    checkBox_simulateWebpageElementClicks.IsChecked = false;
                }
            }
            else if (senderCheckBox == checkBox_searchFromTextWithinTheWebpage)
            {
                if (checkBox_searchFromTextWithinTheWebpage.IsChecked == false)
                    textBox_searchFromTextWithinTheWebpageParameter.Text = "";
            }
            else if (senderCheckBox == checkBox_searchUntilTextWithinTheWebpage)
            {
                if (checkBox_searchUntilTextWithinTheWebpage.IsChecked == false)
                    textBox_searchUntilTextWithinTheWebpageParameter.Text = "";
            }
            else if (senderCheckBox == checkBox_simulateWebpageElementClicks)
            {
                if (checkBox_simulateWebpageElementClicks.IsChecked == false)
                {
                    RemoveAllWebpageElementLocatingInstructionsFromListView();
                    comboBox_webpageElementLocatingMethod.SelectedItem = "";
                    checkBox_webpageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
                    textBox_webpageElementLocatingMethodArgument.Text = "";
                    comboBox_webpageElementLocatingInterval.SelectedItem = "";
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
            else if (senderComboBox == comboBox_webpageElementLocatingMethod)
                checkBox_webpageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
        }
        private void OnRadioButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomRadioButton senderRadioButton = (CustomRadioButton)sender;
            if (senderRadioButton == radioButton_searchWithinTheHtmlElementWithId ||
                senderRadioButton == radioButton_searchWithinTheHtmlElementsThatMatchXPath ||
                senderRadioButton == radioButton_searchGloballyWithinTheWebpage)
            {
                if (senderRadioButton != radioButton_searchWithinTheHtmlElementWithId)
                    textBox_searchWithinTheHtmlElementWithIdParameter.Text = "";
                if (senderRadioButton != radioButton_searchWithinTheHtmlElementsThatMatchXPath)
                    textBox_searchWithinTheHtmlElementsThatMatchXPathParameter.Text = "";
                if (senderRadioButton != radioButton_searchGloballyWithinTheWebpage)
                {
                    textBox_searchFromTextWithinTheWebpageParameter.Text = "";
                    textBox_searchUntilTextWithinTheWebpageParameter.Text = "";
                    checkBox_searchFromTextWithinTheWebpage.IsChecked = false;
                    checkBox_searchUntilTextWithinTheWebpage.IsChecked = false;
                }
            }
        }
        private void OnListViewKeyDownEvent(object sender, KeyEventArgs e)
        {
            ListView senderListView = (ListView)sender;
            if (senderListView == listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn)
                if (e.Key == Key.Delete)
                    if (listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedItems.Count > 0)
                        RemoveSelectedWebpageElementLocatingInstructionsFromListView();
        }
        private void OnGridViewColumnHeaderPreviewMouseMoveEvent(object sender, MouseEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_webpageElementLocatingInstructionIndex)
                e.Handled = true;
        }
        private void OnGridViewColumnHeaderPreviewMouseDoubleClickEvent(object sender, MouseButtonEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_webpageElementLocatingInstructionIndex)
                e.Handled = true;
        }
        private void OnGridViewColumnHeaderSizeChangedEvent(object sender, SizeChangedEventArgs e)
        {
            CustomGridViewColumnHeader senderGridViewColumnHeader = (CustomGridViewColumnHeader)sender;
            if (senderGridViewColumnHeader == gridViewColumnHeader_webpageElementLocatingMethod ||
                senderGridViewColumnHeader == gridViewColumnHeader_webpageElementLocatingMethodArgument ||
                senderGridViewColumnHeader == gridViewColumnHeader_webpageElementLocatingInterval)
            {
                if (senderGridViewColumnHeader.Column.Width < MINIMUM_WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_COLUMN_WIDTH)
                {
                    senderGridViewColumnHeader.Column.Width =
                        MINIMUM_WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_COLUMN_WIDTH;
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
            senderGridViewColumnCollection.Remove(gridViewColumn_webpageElementLocatingInstructionIndex);
            senderGridViewColumnCollection.Insert(0, gridViewColumn_webpageElementLocatingInstructionIndex);
            senderGridViewColumnCollection.CollectionChanged += OnGridViewColumnsCollectionCollectionChangedEvent;
        }
        private void OnListViewItemContextMenuOpeningEvent(object sender, ContextMenuEventArgs e)
        {
            ListViewItem senderListViewItem = (ListViewItem)sender;
            List<object> menuItems = new List<object>();
            int selectedListViewItemsCount =
                listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedItems.Count;
            if (selectedListViewItemsCount == 1)
            {
                menuItems.Add(new MenuItem() { Header = WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__COPY_ARGUMENT });
                menuItems.Add(new MenuItem() { Header = WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_UP });
                menuItems.Add(new MenuItem() { Header = WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_DOWN });
            }
            menuItems.Add(
                new MenuItem()
                {
                    Header =
                        (selectedListViewItemsCount > 1 ?
                            WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED :
                            WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE)
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
            if (senderMenuItem.Header.Equals(WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__COPY_ARGUMENT))
            {
                WebpageElementLocatingInstructionListViewItem selectedWebpageElementLocatingInstructionListViewItem =
                    (WebpageElementLocatingInstructionListViewItem)listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedItems[0];
                Clipboard.SetText(
                    selectedWebpageElementLocatingInstructionListViewItem.UnderlyingWebpageElementLocatingInstruction.MethodArgument
                );
                Clipboard.Flush();
            }
            else if (senderMenuItem.Header.Equals(WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_UP))
                MoveSelectedWebpageElementLocatingInstructionInListView(ListViewItemMovingDirection.Up);
            else if (senderMenuItem.Header.Equals(WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__MOVE_DOWN))
                MoveSelectedWebpageElementLocatingInstructionInListView(ListViewItemMovingDirection.Down);
            else if (senderMenuItem.Header.Equals(WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE) ||
                     senderMenuItem.Header.Equals(WEBPAGE_ELEMENT_LOCATING_INSTRUCTION_LIST_ITEM_CONTEXT_MENU_ITEM_NAME__REMOVE_SELECTED))
            {
                RemoveSelectedWebpageElementLocatingInstructionsFromListView();
            }
        }
        private void OnHyperlinkClickEvent(object sender, RoutedEventArgs e)
        {
            Hyperlink senderHyperlink = (Hyperlink)sender;
            if (senderHyperlink == hyperlink_checkConfiguration)
            {
                string errorDialogMessage;
                if (!CheckFields(false, true, out errorDialogMessage))
                {
                    DialogUtilities.ShowErrorDialog(
                        "",
                        errorDialogMessage,
                        this
                    );
                    return;
                }
                Exception programUpdateCheckConfigurationCheckingException;
                Program._UpdateCheckConfigurationError programUpdateCheckConfigurationError;
                string foundProgramVersionString;
                if (OpenProgramUpdateCheckConfigurationCheckingWindowAsDialog(
                        (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)),
                        out programUpdateCheckConfigurationCheckingException,
                        out programUpdateCheckConfigurationError,
                        out foundProgramVersionString
                    ) == true)
                {
                    if (programUpdateCheckConfigurationCheckingException == null)
                    {
                        if (programUpdateCheckConfigurationError == Program._UpdateCheckConfigurationError.None)
                        {
                            DialogUtilities.ShowInformationDialog(
                                "",
                                INFORMATION_DIALOG_MESSAGE__FOUND_VERSION.Replace(
                                    "{*}",
                                    foundProgramVersionString
                                ),
                                this
                            );
                        }
                        else
                        {
                            DialogUtilities.ShowWarningDialog(
                                "",
                                WARNING_DIALOG_MESSAGE__CHECK_WAS_FAILED.Replace(
                                    "{*}",
                                    StringUtilities.GetTextAsASentence(
                                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                                            programUpdateCheckConfigurationError
                                        )
                                    )
                                ),
                                this
                            );
                        }
                    }
                    else
                    {
                        errorDialogMessage =
                            (programUpdateCheckConfigurationCheckingException.GetType().Equals(
                                 typeof(ProgramsScanAndUpdatesCheckUtilities.ProgramUpdatesCheckWasFailedException)
                             ) ?
                                ((ProgramsScanAndUpdatesCheckUtilities.ProgramUpdatesCheckWasFailedException)programUpdateCheckConfigurationCheckingException)
                                    .GetLongReasonString() :
                                ERROR_DIALOG_MESSAGE__UNKNOWN);
                        DialogUtilities.ShowErrorDialog(
                            "",
                            errorDialogMessage,
                            this
                        );
                    }
                }
            }
        }
        private void OnComboBoxKeyDownEvent(object sender, KeyEventArgs e)
        {
            CustomComboBox senderComboBox = (CustomComboBox)sender;
            if (senderComboBox == comboBox_webpageElementLocatingMethod ||
                senderComboBox == comboBox_webpageElementLocatingInterval)
            {
                if (e.Key == Key.Enter)
                    AddTypedWebpageElementLocatingInstructionToListView();
            }
        }
        private void OnCheckBoxKeyDownEvent(object sender, KeyEventArgs e)
        {
            CustomCheckBox senderCheckBox = (CustomCheckBox)sender;
            if (senderCheckBox == checkBox_webpageElementLocatingMethodArgumentMatchExactText)
                if (e.Key == Key.Enter)
                    AddTypedWebpageElementLocatingInstructionToListView();
        }
        private void OnTextBoxTextChangedEvent(object sender, TextChangedEventArgs e)
        {
            CustomTextBox senderTextBox = (CustomTextBox)sender;
            if (senderTextBox == textBox_name ||
                senderTextBox == textBox_installedVersion ||
                senderTextBox == textBox_webpageUrl ||
                senderTextBox == textBox_searchWithinTheHtmlElementWithIdParameter ||
                senderTextBox == textBox_searchWithinTheHtmlElementsThatMatchXPathParameter ||
                senderTextBox == textBox_searchFromTextWithinTheWebpageParameter ||
                senderTextBox == textBox_searchUntilTextWithinTheWebpageParameter ||
                senderTextBox == textBox_webpageElementLocatingMethodArgument)
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
            if (senderTextBox == textBox_webpageElementLocatingMethodArgument)
                if (e.Key == Key.Enter)
                    AddTypedWebpageElementLocatingInstructionToListView();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool? OpenProgramUpdateCheckConfigurationCheckingWindowAsDialog(bool showBrowserWindow,
                                                                                out Exception programUpdateCheckConfigurationCheckingException,
                                                                                out Program._UpdateCheckConfigurationError programUpdateCheckConfigurationError,
                                                                                out string foundProgramVersionString)
        {
            programUpdateCheckConfigurationCheckingException = null;
            programUpdateCheckConfigurationError = Program._UpdateCheckConfigurationError.None;
            foundProgramVersionString = null;
            bool? returnValue = null;
            ProgramUpdateCheckConfigurationCheckingWindow programUpdateCheckConfigurationCheckingWindow = null;
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        Program programToCheck = GetProgramFromUiControlsValues();
                        programUpdateCheckConfigurationCheckingWindow =
                            new ProgramUpdateCheckConfigurationCheckingWindow(
                                programToCheck,
                                showBrowserWindow
                            );
                        programUpdateCheckConfigurationCheckingWindow.Owner = this;
                        returnValue = programUpdateCheckConfigurationCheckingWindow.ShowDialog();
                    }
            );
            if (returnValue == true)
            {
                programUpdateCheckConfigurationCheckingException =
                    programUpdateCheckConfigurationCheckingWindow.GetProgramUpdateCheckConfigurationCheckingException();
                programUpdateCheckConfigurationError =
                    programUpdateCheckConfigurationCheckingWindow.GetProgramUpdateCheckConfigurationError();
                foundProgramVersionString =
                    programUpdateCheckConfigurationCheckingWindow.GetFoundProgramVersionString();
            }
            return returnValue;
        }
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
                else if (radioButton_searchGloballyWithinTheWebpage.IsChecked == true)
                {
                    if (checkBox_searchFromTextWithinTheWebpage.IsChecked == true &&
                        checkBox_searchUntilTextWithinTheWebpage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyFromTextUntilTextWithinTheWebpage;
                        versionSearchMethodArgument1 =
                            textBox_searchFromTextWithinTheWebpageParameter.Text.Trim();
                        versionSearchMethodArgument2 =
                            textBox_searchUntilTextWithinTheWebpageParameter.Text.Trim();
                    }
                    else if (checkBox_searchFromTextWithinTheWebpage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyFromTextWithinTheWebpage;
                        versionSearchMethodArgument1 =
                            textBox_searchFromTextWithinTheWebpageParameter.Text.Trim();
                        versionSearchMethodArgument2 = "";
                    }
                    else if (checkBox_searchUntilTextWithinTheWebpage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyUntilTextWithinTheWebpage;
                        versionSearchMethodArgument1 =
                            textBox_searchUntilTextWithinTheWebpageParameter.Text.Trim();
                        versionSearchMethodArgument2 = "";
                    }
                    else
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyWithinTheWebpage;
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
        private Program._WebpagePostLoadDelay GetWebpagePostLoadDelay()
        {
            Program._WebpagePostLoadDelay webpagePostLoadDelay =
                Program._WebpagePostLoadDelay.None;
            if (checkBox_configureUpdateCheck.IsChecked == true)
            {
                webpagePostLoadDelay =
                    EnumUtilities.GetEnumItemFromHumanReadableString<Program._WebpagePostLoadDelay>(
                        ((string)comboBox_webpagePostLoadDelay.SelectedItem)
                            .Replace("ms", "Ms")
                    );
            }
            return webpagePostLoadDelay;
        }
        private List<WebpageElementLocatingInstruction> GetLocatingInstructionsOfWebpageElementsToSimulateAClickOn()
        {
            List<WebpageElementLocatingInstruction> locatingInstructionsOfWebpageElementsToSimulateAClickOn =
                new List<WebpageElementLocatingInstruction>();
            foreach (WebpageElementLocatingInstructionListViewItem webpageElementLocatingInstructionListViewItem in
                     listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.Items)
            {
                locatingInstructionsOfWebpageElementsToSimulateAClickOn.Add(
                    webpageElementLocatingInstructionListViewItem.UnderlyingWebpageElementLocatingInstruction
                );
            }
            return locatingInstructionsOfWebpageElementsToSimulateAClickOn;
        }
        private void RefreshListView()
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.Items.Refresh();
                        grid_webElementAdditionField.IsEnabled =
                            (listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.Items.Count < MAX_COUNT_OF_LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON);
                    }
            );
        }
        private void AddTypedWebpageElementLocatingInstructionToListView()
        {
            string webpageElementLocatingMethodArgument =
                textBox_webpageElementLocatingMethodArgument.Text.Trim();
            if (listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.Items.Count < MAX_COUNT_OF_LOCATING_INSTRUCTIONS_OF_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON &&
                (!comboBox_webpageElementLocatingMethod.SelectedItem.Equals("") &&
                 webpageElementLocatingMethodArgument.Length > 0 &&
                 !comboBox_webpageElementLocatingInterval.SelectedItem.Equals("")))
            {
                WebpageElementLocatingInstruction._LocatingMethod webpageElementLocatingMethod =
                    EnumUtilities.GetEnumItemFromHumanReadableString<WebpageElementLocatingInstruction._LocatingMethod>(
                        ((string)comboBox_webpageElementLocatingMethod.SelectedItem)
                            .Replace("ID", "Id")
                            .Replace("HTML", "Html")
                    );
                WebpageElementLocatingInstruction._LocatingInterval webpageElementLocatingInterval =
                    EnumUtilities.GetEnumItemFromHumanReadableString<WebpageElementLocatingInstruction._LocatingInterval>(
                        ((string)comboBox_webpageElementLocatingInterval.SelectedItem)
                            .Replace("ms", "Ms")
                    );
                WebpageElementLocatingInstruction typedWebpageElementLocatingInstructionToListView =
                    new WebpageElementLocatingInstruction(
                        webpageElementLocatingMethod,
                        webpageElementLocatingMethodArgument,
                        checkBox_webpageElementLocatingMethodArgumentMatchExactText.IsChecked.Value,
                        webpageElementLocatingInterval
                    );
                locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn.Add(
                    new WebpageElementLocatingInstructionListViewItem(
                        listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.Items.Count + 1,
                        typedWebpageElementLocatingInstructionToListView
                    )
                );
                RefreshListView();
                listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedIndex = -1;
                listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.ScrollIntoView(
                    listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.Items[
                        listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.Items.Count - 1
                    ]
                );
                comboBox_webpageElementLocatingMethod.SelectedItem = "";
                checkBox_webpageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
                textBox_webpageElementLocatingMethodArgument.Text = "";
                comboBox_webpageElementLocatingInterval.SelectedItem = "";
            }
        }
        private void MoveSelectedWebpageElementLocatingInstructionInListView(ListViewItemMovingDirection movingDirection)
        {
            if (listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedItems.Count == 1)
            {
                WebpageElementLocatingInstructionListViewItem selectedWebpageElementLocatingInstructionListViewItem =
                    (WebpageElementLocatingInstructionListViewItem)listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedItems[0];
                int indexOfSelectedWebpageElementLocatingInstructionListViewItem =
                    locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn.IndexOf(
                        selectedWebpageElementLocatingInstructionListViewItem
                    );
                switch (movingDirection)
                {
                    case ListViewItemMovingDirection.Up:
                        if (indexOfSelectedWebpageElementLocatingInstructionListViewItem > 0)
                        {
                            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                indexOfSelectedWebpageElementLocatingInstructionListViewItem
                            ].WebpageElementLocatingInstructionIndex--;
                            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                indexOfSelectedWebpageElementLocatingInstructionListViewItem - 1
                            ].WebpageElementLocatingInstructionIndex++;
                            WebpageElementLocatingInstructionListViewItem tempWebpageElementLocatingInstructionListViewItem =
                                locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                    indexOfSelectedWebpageElementLocatingInstructionListViewItem
                                ];
                            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                indexOfSelectedWebpageElementLocatingInstructionListViewItem
                            ] =
                                locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                    indexOfSelectedWebpageElementLocatingInstructionListViewItem - 1
                                ];
                            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                indexOfSelectedWebpageElementLocatingInstructionListViewItem - 1
                            ] =
                                tempWebpageElementLocatingInstructionListViewItem;
                        }
                        break;
                    case ListViewItemMovingDirection.Down:
                        if (indexOfSelectedWebpageElementLocatingInstructionListViewItem < locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn.Count - 1)
                        {
                            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                indexOfSelectedWebpageElementLocatingInstructionListViewItem
                            ].WebpageElementLocatingInstructionIndex++;
                            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                indexOfSelectedWebpageElementLocatingInstructionListViewItem + 1
                            ].WebpageElementLocatingInstructionIndex--;
                            WebpageElementLocatingInstructionListViewItem tempWebpageElementLocatingInstructionListViewItem =
                                locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                    indexOfSelectedWebpageElementLocatingInstructionListViewItem
                                ];
                            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                indexOfSelectedWebpageElementLocatingInstructionListViewItem
                            ] =
                                locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                    indexOfSelectedWebpageElementLocatingInstructionListViewItem + 1
                                ];
                            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[
                                indexOfSelectedWebpageElementLocatingInstructionListViewItem + 1
                            ] =
                                tempWebpageElementLocatingInstructionListViewItem;
                        }
                        break;
                }
                RefreshListView();
                listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.ScrollIntoView(
                    listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedItems[0]
                );
            }
        }
        private void RemoveSelectedWebpageElementLocatingInstructionsFromListView()
        {
            if (listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedItems.Count > 0)
            {
                foreach (WebpageElementLocatingInstructionListViewItem selectedWebpageElementLocatingInstructionListViewItem in
                         listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.SelectedItems)
                {
                    locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn.Remove(
                        selectedWebpageElementLocatingInstructionListViewItem
                    );
                }
                for (int i = 0; i < locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn.Count; i++)
                    locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn[i].WebpageElementLocatingInstructionIndex = i + 1;
                RefreshListView();
            }
        }
        private void RemoveAllWebpageElementLocatingInstructionsFromListView()
        {
            locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn.Clear();
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
            textBox_webpageUrl.Text = "";
            if (program.IsUpdateCheckConfigured)
            {
                checkBox_configureUpdateCheck.IsChecked = true;
                textBox_webpageUrl.Text = program.WebpageUrl;
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
                    case Program._VersionSearchMethod.SearchGloballyWithinTheWebpage:
                        radioButton_searchGloballyWithinTheWebpage.IsChecked = true;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyFromTextWithinTheWebpage:
                        radioButton_searchGloballyWithinTheWebpage.IsChecked = true;
                        checkBox_searchFromTextWithinTheWebpage.IsChecked = true;
                        textBox_searchFromTextWithinTheWebpageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyUntilTextWithinTheWebpage:
                        radioButton_searchGloballyWithinTheWebpage.IsChecked = true;
                        checkBox_searchUntilTextWithinTheWebpage.IsChecked = true;
                        textBox_searchUntilTextWithinTheWebpageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyFromTextUntilTextWithinTheWebpage:
                        radioButton_searchGloballyWithinTheWebpage.IsChecked = true;
                        checkBox_searchFromTextWithinTheWebpage.IsChecked = true;
                        checkBox_searchUntilTextWithinTheWebpage.IsChecked = true;
                        textBox_searchFromTextWithinTheWebpageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        textBox_searchUntilTextWithinTheWebpageParameter.Text =
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
                comboBox_webpagePostLoadDelay.SelectedItem =
                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                        program.WebpagePostLoadDelay
                    ).Replace(" Ms", "ms");
                if (program.LocatingInstructionsOfWebpageElementsToSimulateAClickOn.Count > 0)
                {
                    checkBox_simulateWebpageElementClicks.IsChecked = true;
                    for (int i = 0; i < program.LocatingInstructionsOfWebpageElementsToSimulateAClickOn.Count; i++)
                    {
                        locatingInstructionListViewItemsOfWebpageElementsToSimulateAClickOn.Add(
                            new WebpageElementLocatingInstructionListViewItem(
                                i + 1,
                                program.LocatingInstructionsOfWebpageElementsToSimulateAClickOn[i]
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
            string name = textBox_name.Text.Trim();
            string installedVersion = textBox_installedVersion.Text.Trim();
            string latestVersion = "";
            Program._InstallationScope installationScope = GetInstallationScope();
            string webpageUrl = textBox_webpageUrl.Text.Trim();
            string versionSearchMethodArgument1;
            string versionSearchMethodArgument2;
            Program._VersionSearchMethod versionSearchMethod = GetVersionSearchMethod(
                out versionSearchMethodArgument1,
                out versionSearchMethodArgument2
            );
            Program._VersionSearchBehavior versionSearchBehavior = GetVersionSearchBehavior();
            Program._WebpagePostLoadDelay webpagePostLoadDelay = GetWebpagePostLoadDelay();
            List<WebpageElementLocatingInstruction> locatingInstructionsOfWebpageElementsToSimulateAClickOn =
                GetLocatingInstructionsOfWebpageElementsToSimulateAClickOn();
            Program._UpdateCheckConfigurationStatus updateCheckConfigurationStatus =
                Program._UpdateCheckConfigurationStatus.Unknown;
            Program._UpdateCheckConfigurationError updateCheckConfigurationError =
                Program._UpdateCheckConfigurationError.None;
            string skippedVersion = "";
            if (programToEdit != null)
            {
                if (checkBox_configureUpdateCheck.IsChecked == programToEdit.IsUpdateCheckConfigured &&
                    webpageUrl.Equals(programToEdit.WebpageUrl) &&
                    versionSearchMethod == programToEdit.VersionSearchMethod &&
                    versionSearchMethodArgument1.Equals(programToEdit.VersionSearchMethodArgument1) &&
                    versionSearchMethodArgument2.Equals(programToEdit.VersionSearchMethodArgument2) &&
                    checkBox_treatAStandaloneNumberAsAVersion.IsChecked == programToEdit.TreatAStandaloneNumberAsAVersion &&
                    versionSearchBehavior == programToEdit.VersionSearchBehavior &&
                    webpagePostLoadDelay == programToEdit.WebpagePostLoadDelay &&
                    locatingInstructionsOfWebpageElementsToSimulateAClickOn.SequenceEqual(
                        programToEdit.LocatingInstructionsOfWebpageElementsToSimulateAClickOn
                    ))
                {
                    latestVersion = programToEdit.LatestVersion;
                    updateCheckConfigurationStatus = programToEdit.UpdateCheckConfigurationStatus;
                    updateCheckConfigurationError = programToEdit.UpdateCheckConfigurationError;
                }
                if (!programToEdit.SkippedVersion.Equals("") &&
                    (installedVersion.Equals("") ||
                     VersionUtilities.IsVersionNewer(
                         programToEdit.SkippedVersion,
                         installedVersion
                     )))
                {
                    skippedVersion = programToEdit.SkippedVersion;
                }
            }
            return new Program(
                name,
                installedVersion,
                latestVersion,
                installationScope,
                checkBox_detectAutomatically.IsChecked.Value,
                checkBox_configureUpdateCheck.IsChecked.Value,
                webpageUrl,
                versionSearchMethod,
                versionSearchMethodArgument1,
                versionSearchMethodArgument2,
                checkBox_treatAStandaloneNumberAsAVersion.IsChecked.Value,
                versionSearchBehavior,
                webpagePostLoadDelay,
                locatingInstructionsOfWebpageElementsToSimulateAClickOn,
                updateCheckConfigurationStatus,
                updateCheckConfigurationError,
                skippedVersion,
                (programToEdit != null ? programToEdit.IsHidden : false),
                false
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
                                    StringUtilities.GetTextAsASentence(
                                        EnumUtilities.GetHumanReadableStringFromEnumItem(
                                            updateCheckConfigurationError
                                        )
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
                    657 :
                    (programToEdit.UpdateCheckConfigurationStatus != Program._UpdateCheckConfigurationStatus.Valid ?
                        687 :
                        717)) * App.WindowsRenderingScale
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
            string name = null;
            if (checkProgramProperties)
            {
                name = textBox_name.Text.Trim();
                if (name.Equals(""))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_NAME;
                    return false;
                }
                string installedVersion = textBox_installedVersion.Text.Trim();
                if (!installedVersion.Equals("") &&
                    !VersionUtilities.IsVersion(
                        installedVersion,
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
                    textBox_webpageUrl.Text.Trim().Equals(""))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_WEBPAGE_URL;
                    return false;
                }
                if (checkBox_configureUpdateCheck.IsChecked == true &&
                    ((radioButton_searchWithinTheHtmlElementWithId.IsChecked == true &&
                      textBox_searchWithinTheHtmlElementWithIdParameter.Text.Trim().Equals("")) ||
                     (radioButton_searchWithinTheHtmlElementsThatMatchXPath.IsChecked == true &&
                      textBox_searchWithinTheHtmlElementsThatMatchXPathParameter.Text.Trim().Equals("")) ||
                     (checkBox_searchFromTextWithinTheWebpage.IsChecked == true &&
                      textBox_searchFromTextWithinTheWebpageParameter.Text.Trim().Equals("")) ||
                     (checkBox_searchUntilTextWithinTheWebpage.IsChecked == true &&
                      textBox_searchUntilTextWithinTheWebpageParameter.Text.Trim().Equals(""))))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_METHOD_OF_VERSION_SEARCH;
                    return false;
                }
                if (checkBox_configureUpdateCheck.IsChecked == true &&
                    (checkBox_simulateWebpageElementClicks.IsChecked == true &&
                     listView_locatingInstructionsOfWebpageElementsToSimulateAClickOn.Items.Count == 0))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_WEBPAGE_ELEMENTS_TO_SIMULATE_A_CLICK_ON;
                    return false;
                }
            }
            if (checkProgramProperties)
            {
                if ((programToEdit == null &&
                     programsAlreadyInDatabase.ContainsKey(name)) ||
                    (programToEdit != null &&
                     !name.Equals(programToEdit.Name) &&
                     programsAlreadyInDatabase.ContainsKey(name)))
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
