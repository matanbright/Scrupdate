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
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
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
        private const string WINDOW_TITLE__PROGRAM_ADDING = "Scrupdate - Program Adding";
        private const string WINDOW_TITLE__PROGRAM_EDITING = "Scrupdate - Program Editing [{*}]";
        private const string LAST_PROGRAM_UPDATE_CHECK_STATUS_MESSAGE__LAST_CHECK_WAS_SUCCEEDED = "Last Check Was Succeeded";
        private const string LAST_PROGRAM_UPDATE_CHECK_STATUS_MESSAGE__LAST_CHECK_WAS_FAILED = "Last Check Was Failed (Reason: {*})";
        private const string ERROR_DIALOG_TITLE__ERROR = "Error";
        private const string ERROR_DIALOG_MESSAGE__NO_PROGRAM_NAME = "The 'Program Name' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__INVALID_INSTALLED_VERSION = "The 'Installed Version' Field's Value Is Invalid!";
        private const string ERROR_DIALOG_MESSAGE__NO_WEB_PAGE_URL = "The 'Web Page URL' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__NO_METHOD_OF_VERSION_SEARCH = "The 'Version Search Method' Field Cannot Be Empty!";
        private const string ERROR_DIALOG_MESSAGE__NO_WEB_PAGE_ELEMENTS_WERE_SELECTED_FOR_SIMULATING_A_CLICK = "No Web Page Elements Were Selected for Simulating a Click!";
        private const string ERROR_DIALOG_MESSAGE__PROGRAM_NAME_ALREADY_EXISTS = "A Program with That Name Already Exists!";
        private const string QUESTION_DIALOG_MESSAGE__CONVERT_THE_PROGRAM_TO_A_MANUALLY_ADDED_PROGRAM = "Convert the Program to a Manually-Added Program?\r\n\r\n•  You will need to update the program's information manually,\r\n    every time you install a new version of the program or remove the program.\r\n•  It cannot be undone automatically.";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum WindowVariation
        {
            Unknown,
            ProgramAddingWindow,
            ProgramEditingWindow
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
        public Size BaseSizeOfWindow { get; private set; }
        public WindowVariation CurrentWindowVariation
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (WindowVariation)GetValue(CurrentWindowVariationProperty)
                );
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(
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
                return ThreadsUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Program._UpdateCheckConfigurationStatus)GetValue(LastProgramUpdateCheckConfigurationStatusProperty)
                );
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(
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
            BaseSizeOfWindow = new Size(Width, Height);
            WindowsUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSizeOfWindow,
                App.WindowsRenderingScale
            );
            foreach (Program._InstallationScope installationScope in
                     Enum.GetValues(typeof(Program._InstallationScope)))
            {
                comboBox_installedFor.Items.Add(
                    EnumsUtilities.GetHumanReadableStringFromEnumItem(
                        installationScope
                    )
                );
            }
            comboBox_installedFor.SelectedItem =
                EnumsUtilities.GetHumanReadableStringFromEnumItem(
                    Program._InstallationScope.Everyone
                );
            foreach (Program._WebPagePostLoadDelay webPagePostLoadDelay in
                     Enum.GetValues(typeof(Program._WebPagePostLoadDelay)))
            {
                comboBox_webPagePostLoadDelay.Items.Add(
                    EnumsUtilities.GetHumanReadableStringFromEnumItem(
                        webPagePostLoadDelay
                    ).Replace(" Ms", "ms")
                );
            }
            comboBox_webPagePostLoadDelay.SelectedItem =
                EnumsUtilities.GetHumanReadableStringFromEnumItem(
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
                        EnumsUtilities.GetHumanReadableStringFromEnumItem(
                            locatingMethod
                        ).Replace("Id", "ID").Replace("Html", "HTML").Replace("X Path", "XPath")
                    );
                }
            }
            comboBox_webPageElementLocatingMethod.SelectedItem = "";
            foreach (WebPageElementLocatingInstruction._LocatingDuration locatingDuration in
                     Enum.GetValues(typeof(WebPageElementLocatingInstruction._LocatingDuration)))
            {
                if (locatingDuration == WebPageElementLocatingInstruction._LocatingDuration.Unspecified)
                    comboBox_webPageElementLocatingDuration.Items.Add("");
                else
                {
                    comboBox_webPageElementLocatingDuration.Items.Add(
                        EnumsUtilities.GetHumanReadableStringFromEnumItem(
                            locatingDuration
                        ).Replace(" Ms", "ms")
                    );
                }
            }
            comboBox_webPageElementLocatingDuration.SelectedItem = "";
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
        private void OnWindowClosingEvent(object sender, CancelEventArgs e)
        {
            ThreadsUtilities.RunOnAnotherThread(
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
                MoveDownSelectedWebPageElementLocatingInstructionsInListView();
            else if (senderButton == button_moveUpWebPageElement)
                MoveUpSelectedWebPageElementLocatingInstructionsInListView();
            else if (senderButton == button_addOrSave)
            {
                string errorDialogMessage = null;
                if (textBox_programName.Text.Trim().Equals(""))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_PROGRAM_NAME;
                }
                else if (!textBox_installedVersion.Text.Trim().Equals("") &&
                         !VersionsUtilities.IsVersion(
                             textBox_installedVersion.Text.Trim(),
                             VersionsUtilities.VersionValidation.ValidateVersionSegmentsCountButTreatAStandaloneNumberAsAVersion
                         ))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__INVALID_INSTALLED_VERSION;
                }
                else if (checkBox_configureProgramUpdateCheck.IsChecked == true &&
                         textBox_webPageUrl.Text.Trim().Equals(""))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_WEB_PAGE_URL;
                }
                else if (checkBox_configureProgramUpdateCheck.IsChecked == true &&
                         ((radioButton_searchInTheContentOfHtmlElementWithId.IsChecked == true &&
                           textBox_searchInTheContentOfHtmlElementWithIdParameter.Text.Trim().Equals("")) ||
                          (radioButton_searchInTheContentOfHtmlElementsMatchingXPath.IsChecked == true &&
                           textBox_searchInTheContentOfHtmlElementsMatchingXPathParameter.Text.Trim().Equals("")) ||
                          (checkBox_searchFromTextWithinWebPage.IsChecked == true &&
                           textBox_searchFromTextWithinWebPageParameter.Text.Trim().Equals("")) ||
                          (checkBox_searchUntilTextWithinWebPage.IsChecked == true &&
                           textBox_searchUntilTextWithinWebPageParameter.Text.Trim().Equals(""))))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_METHOD_OF_VERSION_SEARCH;
                }
                else if (checkBox_simulateWebPageElementClicks.IsChecked == true &&
                         listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items.Count == 0)
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_WEB_PAGE_ELEMENTS_WERE_SELECTED_FOR_SIMULATING_A_CLICK;
                }
                else if ((programToEdit == null &&
                          programsAlreadyInDatabase.ContainsKey(textBox_programName.Text.Trim())) ||
                         (programToEdit != null &&
                          !textBox_programName.Text.Trim().Equals(programToEdit.Name) &&
                          programsAlreadyInDatabase.ContainsKey(textBox_programName.Text.Trim())))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__PROGRAM_NAME_ALREADY_EXISTS;
                }
                if (errorDialogMessage != null)
                {
                    DialogsUtilities.ShowErrorDialog(
                        ERROR_DIALOG_TITLE__ERROR,
                        errorDialogMessage,
                        this
                    );
                    return;
                }
                if (!textBox_installedVersion.Text.Trim().Equals(""))
                {
                    textBox_installedVersion.Text = VersionsUtilities.NormalizeAndTrimVersion(
                        textBox_installedVersion.Text.Trim(),
                        VersionsUtilities.MINIMUM_VERSION_SEGMENTS,
                        VersionsUtilities.MAXIMUM_VERSION_SEGMENTS
                    );
                }
                Program._InstallationScope installationScope = GetSelectedInstallationScope();
                string versionSearchMethodArgument1;
                string versionSearchMethodArgument2;
                Program._VersionSearchMethod versionSearchMethod = GetSelectedVersionSearchMethod(
                    out versionSearchMethodArgument1,
                    out versionSearchMethodArgument2
                );
                Program._VersionSearchBehavior versionSearchBehavior = GetSelectedVersionSearchBehavior();
                Program._WebPagePostLoadDelay webPagePostLoadDelay = GetSelectedWebPagePostLoadDelay();
                List<WebPageElementLocatingInstruction> locatingInstructionsOfWebPageElementsToSimulateAClickOn =
                    GetSelectedLocatingInstructionsOfWebPageElementsToSimulateAClickOn();
                if (programToEdit != null &&
                    (checkBox_detectAutomatically.IsChecked == programToEdit.IsAutomaticallyAdded &&
                     textBox_programName.Text.Trim().Equals(programToEdit.Name) &&
                     textBox_installedVersion.Text.Trim().Equals(programToEdit.InstalledVersion) &&
                     installationScope == programToEdit.InstallationScope &&
                     checkBox_configureProgramUpdateCheck.IsChecked == programToEdit.IsUpdateCheckConfigured &&
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
                        if (DialogsUtilities.ShowQuestionDialog(
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
                    textBox_programName.Text = programToEdit.Name;
                    textBox_installedVersion.Text = programToEdit.InstalledVersion;
                    comboBox_installedFor.SelectedItem =
                        EnumsUtilities.GetHumanReadableStringFromEnumItem(
                            programToEdit.InstallationScope
                        );
                }
            }
            else if (senderCheckBox == checkBox_configureProgramUpdateCheck)
            {
                if (checkBox_configureProgramUpdateCheck.IsChecked == false)
                {
                    textBox_webPageUrl.Text = "";
                    textBox_searchInTheContentOfHtmlElementWithIdParameter.Text = "";
                    textBox_searchInTheContentOfHtmlElementsMatchingXPathParameter.Text = "";
                    textBox_searchFromTextWithinWebPageParameter.Text = "";
                    textBox_searchUntilTextWithinWebPageParameter.Text = "";
                    checkBox_searchFromTextWithinWebPage.IsChecked = false;
                    checkBox_searchUntilTextWithinWebPage.IsChecked = false;
                    radioButton_searchInTheContentOfHtmlElementWithId.IsChecked = true;
                    checkBox_treatAStandaloneNumberAsAVersion.IsChecked = false;
                    radioButton_getTheFirstVersionThatIsFound.IsChecked = true;
                    expander_advancedOptions.IsExpanded = false;
                    comboBox_webPagePostLoadDelay.SelectedItem =
                        EnumsUtilities.GetHumanReadableStringFromEnumItem(
                            Program._WebPagePostLoadDelay.None
                        ).Replace(" Ms", "ms");
                    RemoveAllWebPageElementLocatingInstructionsFromListView();
                    comboBox_webPageElementLocatingMethod.SelectedItem = "";
                    checkBox_webPageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
                    textBox_webPageElementLocatingMethodArgument.Text = "";
                    comboBox_webPageElementLocatingDuration.SelectedItem = "";
                    checkBox_simulateWebPageElementClicks.IsChecked = false;
                }
            }
            else if (senderCheckBox == checkBox_searchFromTextWithinWebPage)
            {
                if (checkBox_searchFromTextWithinWebPage.IsChecked == false)
                    textBox_searchFromTextWithinWebPageParameter.Text = "";
            }
            else if (senderCheckBox == checkBox_searchUntilTextWithinWebPage)
            {
                if (checkBox_searchUntilTextWithinWebPage.IsChecked == false)
                    textBox_searchUntilTextWithinWebPageParameter.Text = "";
            }
            else if (senderCheckBox == checkBox_simulateWebPageElementClicks)
            {
                if (checkBox_simulateWebPageElementClicks.IsChecked == false)
                {
                    RemoveAllWebPageElementLocatingInstructionsFromListView();
                    comboBox_webPageElementLocatingMethod.SelectedItem = "";
                    checkBox_webPageElementLocatingMethodArgumentMatchExactText.IsChecked = false;
                    textBox_webPageElementLocatingMethodArgument.Text = "";
                    comboBox_webPageElementLocatingDuration.SelectedItem = "";
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
            if (senderRadioButton == radioButton_searchInTheContentOfHtmlElementWithId ||
                senderRadioButton == radioButton_searchInTheContentOfHtmlElementsMatchingXPath ||
                senderRadioButton == radioButton_searchGloballyInTheWebPage)
            {
                if (senderRadioButton != radioButton_searchInTheContentOfHtmlElementWithId)
                    textBox_searchInTheContentOfHtmlElementWithIdParameter.Text = "";
                if (senderRadioButton != radioButton_searchInTheContentOfHtmlElementsMatchingXPath)
                    textBox_searchInTheContentOfHtmlElementsMatchingXPathParameter.Text = "";
                if (senderRadioButton != radioButton_searchGloballyInTheWebPage)
                {
                    textBox_searchFromTextWithinWebPageParameter.Text = "";
                    textBox_searchUntilTextWithinWebPageParameter.Text = "";
                    checkBox_searchFromTextWithinWebPage.IsChecked = false;
                    checkBox_searchUntilTextWithinWebPage.IsChecked = false;
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
        private void OnGridViewColumnsCollectionCollectionChangedEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            GridViewColumnCollection senderGridViewColumnCollection = (GridViewColumnCollection)sender;
            senderGridViewColumnCollection.CollectionChanged -= OnGridViewColumnsCollectionCollectionChangedEvent;
            senderGridViewColumnCollection.Remove(gridViewColumn_webPageElementLocatingInstructionIndex);
            senderGridViewColumnCollection.Insert(0, gridViewColumn_webPageElementLocatingInstructionIndex);
            senderGridViewColumnCollection.CollectionChanged += OnGridViewColumnsCollectionCollectionChangedEvent;
        }
        private void OnMenuItemClickEvent(object sender, RoutedEventArgs e)
        {
            MenuItem senderMenuItem = (MenuItem)sender;
            if (senderMenuItem.Header.Equals("Copy Locating Method Argument"))
            {
                WebPageElementLocatingInstructionListViewItem locatingInstructionListViewItemOfWebPageElementLocatingInstructionToEdit =
                    (WebPageElementLocatingInstructionListViewItem)listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems[0];
                Clipboard.SetText(
                    locatingInstructionListViewItemOfWebPageElementLocatingInstructionToEdit.UnderlyingWebPageElementLocatingInstruction.MethodArgument
                );
                Clipboard.Flush();
            }
            else if (senderMenuItem.Header.Equals("Move Up"))
                MoveUpSelectedWebPageElementLocatingInstructionsInListView();
            else if (senderMenuItem.Header.Equals("Move Down"))
                MoveDownSelectedWebPageElementLocatingInstructionsInListView();
            else if (senderMenuItem.Header.Equals("Remove") ||
                     senderMenuItem.Header.Equals("Remove Selected"))
            {
                RemoveSelectedWebPageElementLocatingInstructionsFromListView();
            }
        }
        private void OnComboBoxKeyDownEvent(object sender, KeyEventArgs e)
        {
            CustomComboBox senderComboBox = (CustomComboBox)sender;
            if (senderComboBox == comboBox_webPageElementLocatingMethod ||
                senderComboBox == comboBox_webPageElementLocatingDuration)
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
            if (senderTextBox == textBox_programName ||
                senderTextBox == textBox_installedVersion ||
                senderTextBox == textBox_webPageUrl ||
                senderTextBox == textBox_searchInTheContentOfHtmlElementWithIdParameter ||
                senderTextBox == textBox_searchInTheContentOfHtmlElementsMatchingXPathParameter ||
                senderTextBox == textBox_searchFromTextWithinWebPageParameter ||
                senderTextBox == textBox_searchUntilTextWithinWebPageParameter ||
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
        private Program._InstallationScope GetSelectedInstallationScope()
        {
            return EnumsUtilities.GetEnumItemFromHumanReadableString<Program._InstallationScope>(
                (string)comboBox_installedFor.SelectedItem
            );
        }
        private Program._VersionSearchMethod GetSelectedVersionSearchMethod(out string versionSearchMethodArgument1,
                                                                            out string versionSearchMethodArgument2)
        {
            versionSearchMethodArgument1 = null;
            versionSearchMethodArgument2 = null;
            Program._VersionSearchMethod versionSearchMethod =
                Program._VersionSearchMethod.Unknown;
            if (checkBox_configureProgramUpdateCheck.IsChecked == true)
            {
                if (radioButton_searchInTheContentOfHtmlElementWithId.IsChecked == true)
                {
                    versionSearchMethod =
                        Program._VersionSearchMethod.SearchInTheContentOfHtmlElementWithId;
                    versionSearchMethodArgument1 =
                        textBox_searchInTheContentOfHtmlElementWithIdParameter.Text.Trim();
                    versionSearchMethodArgument2 = "";
                }
                else if (radioButton_searchInTheContentOfHtmlElementsMatchingXPath.IsChecked == true)
                {
                    versionSearchMethod =
                        Program._VersionSearchMethod.SearchInTheContentOfHtmlElementsMatchingXPath;
                    versionSearchMethodArgument1 =
                        textBox_searchInTheContentOfHtmlElementsMatchingXPathParameter.Text.Trim();
                    versionSearchMethodArgument2 = "";
                }
                else if (radioButton_searchGloballyInTheWebPage.IsChecked == true)
                {
                    if (checkBox_searchFromTextWithinWebPage.IsChecked == true &&
                        checkBox_searchUntilTextWithinWebPage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyFromTextUntilTextWithinWebPage;
                        versionSearchMethodArgument1 =
                            textBox_searchFromTextWithinWebPageParameter.Text.Trim();
                        versionSearchMethodArgument2 =
                            textBox_searchUntilTextWithinWebPageParameter.Text.Trim();
                    }
                    else if (checkBox_searchFromTextWithinWebPage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyFromTextWithinWebPage;
                        versionSearchMethodArgument1 =
                            textBox_searchFromTextWithinWebPageParameter.Text.Trim();
                        versionSearchMethodArgument2 = "";
                    }
                    else if (checkBox_searchUntilTextWithinWebPage.IsChecked == true)
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyUntilTextWithinWebPage;
                        versionSearchMethodArgument1 =
                            textBox_searchUntilTextWithinWebPageParameter.Text.Trim();
                        versionSearchMethodArgument2 = "";
                    }
                    else
                    {
                        versionSearchMethod =
                            Program._VersionSearchMethod.SearchGloballyInTheWebPage;
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
        private Program._VersionSearchBehavior GetSelectedVersionSearchBehavior()
        {
            Program._VersionSearchBehavior versionSearchBehavior =
                Program._VersionSearchBehavior.Unknown;
            if (checkBox_configureProgramUpdateCheck.IsChecked == true)
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
        private Program._WebPagePostLoadDelay GetSelectedWebPagePostLoadDelay()
        {
            Program._WebPagePostLoadDelay webPagePostLoadDelay =
                Program._WebPagePostLoadDelay.None;
            if (checkBox_configureProgramUpdateCheck.IsChecked == true)
            {
                webPagePostLoadDelay =
                    EnumsUtilities.GetEnumItemFromHumanReadableString<Program._WebPagePostLoadDelay>(
                        ((string)comboBox_webPagePostLoadDelay.SelectedItem)
                            .Replace("ms", "Ms")
                    );
            }
            return webPagePostLoadDelay;
        }
        private List<WebPageElementLocatingInstruction> GetSelectedLocatingInstructionsOfWebPageElementsToSimulateAClickOn()
        {
            List<WebPageElementLocatingInstruction> selectedLocatingInstructionsOfWebPageElementsToSimulateAClickOn =
                new List<WebPageElementLocatingInstruction>();
            foreach (WebPageElementLocatingInstructionListViewItem locatingInstructionOfWebPageElementToSimulateAClickOn in
                     listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.Items)
            {
                selectedLocatingInstructionsOfWebPageElementsToSimulateAClickOn.Add(
                    locatingInstructionOfWebPageElementToSimulateAClickOn.UnderlyingWebPageElementLocatingInstruction
                );
            }
            return selectedLocatingInstructionsOfWebPageElementsToSimulateAClickOn;
        }
        private void RefreshListView()
        {
            ThreadsUtilities.RunOnAnotherThread(
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
                 !comboBox_webPageElementLocatingDuration.SelectedItem.Equals("")))
            {
                WebPageElementLocatingInstruction._LocatingMethod selectedWebPageElementLocatingMethod =
                    EnumsUtilities.GetEnumItemFromHumanReadableString<WebPageElementLocatingInstruction._LocatingMethod>(
                        ((string)comboBox_webPageElementLocatingMethod.SelectedItem)
                            .Replace("ID", "Id")
                            .Replace("HTML", "Html")
                    );
                WebPageElementLocatingInstruction._LocatingDuration selectedWebPageElementLocatingDuration =
                    EnumsUtilities.GetEnumItemFromHumanReadableString<WebPageElementLocatingInstruction._LocatingDuration>(
                        ((string)comboBox_webPageElementLocatingDuration.SelectedItem)
                            .Replace("ms", "Ms")
                    );
                WebPageElementLocatingInstruction typedWebPageElementLocatingInstructionToListView =
                    new WebPageElementLocatingInstruction(
                        selectedWebPageElementLocatingMethod,
                        textBox_webPageElementLocatingMethodArgument.Text.Trim(),
                        (bool)checkBox_webPageElementLocatingMethodArgumentMatchExactText.IsChecked,
                        selectedWebPageElementLocatingDuration
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
                comboBox_webPageElementLocatingDuration.SelectedItem = "";
            }
        }
        private void MoveDownSelectedWebPageElementLocatingInstructionsInListView()
        {
            if (listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems.Count == 1)
            {
                WebPageElementLocatingInstructionListViewItem selectedWebPageElementLocatingInstructionListViewItem =
                    (WebPageElementLocatingInstructionListViewItem)listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems[0];
                int indexOfWebPageElementListViewItem =
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.IndexOf(
                        selectedWebPageElementLocatingInstructionListViewItem
                    );
                if (indexOfWebPageElementListViewItem < locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.Count - 1)
                {
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                        indexOfWebPageElementListViewItem
                    ].WebPageElementLocatingInstructionIndex++;
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                        indexOfWebPageElementListViewItem + 1
                    ].WebPageElementLocatingInstructionIndex--;
                    WebPageElementLocatingInstructionListViewItem tempWebPageElementLocatingInstructionListViewItem =
                        locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                            indexOfWebPageElementListViewItem
                        ];
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                        indexOfWebPageElementListViewItem
                    ] =
                        locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                            indexOfWebPageElementListViewItem + 1
                        ];
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                        indexOfWebPageElementListViewItem + 1
                    ] =
                        tempWebPageElementLocatingInstructionListViewItem;
                }
                RefreshListView();
                listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.ScrollIntoView(
                    listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems[0]
                );
            }
        }
        private void MoveUpSelectedWebPageElementLocatingInstructionsInListView()
        {
            if (listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems.Count == 1)
            {
                WebPageElementLocatingInstructionListViewItem selectedWebPageElementLocatingInstructionListViewItem =
                    (WebPageElementLocatingInstructionListViewItem)listView_locatingInstructionsOfWebPageElementsToSimulateAClickOn.SelectedItems[0];
                int indexOfWebPageElementListViewItem =
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn.IndexOf(
                        selectedWebPageElementLocatingInstructionListViewItem
                    );
                if (indexOfWebPageElementListViewItem > 0)
                {
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                        indexOfWebPageElementListViewItem
                    ].WebPageElementLocatingInstructionIndex--;
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                        indexOfWebPageElementListViewItem - 1
                    ].WebPageElementLocatingInstructionIndex++;
                    WebPageElementLocatingInstructionListViewItem tempWebPageElementLocatingInstructionListViewItem =
                        locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                            indexOfWebPageElementListViewItem
                        ];
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                        indexOfWebPageElementListViewItem
                    ] =
                        locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                            indexOfWebPageElementListViewItem - 1
                        ];
                    locatingInstructionListViewItemsOfWebPageElementsToSimulateAClickOn[
                        indexOfWebPageElementListViewItem - 1
                    ] =
                        tempWebPageElementLocatingInstructionListViewItem;
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
            checkBox_detectAutomatically.IsEnabled = program.IsAutomaticallyAdded;
            checkBox_detectAutomatically.IsChecked = program.IsAutomaticallyAdded;
            textBox_programName.Text = program.Name;
            textBox_installedVersion.Text = program.InstalledVersion;
            label_latestVersion.Content = program.LatestVersion;
            label_latestVersion.Foreground = (SolidColorBrush)Application.Current.FindResource(
                App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH
            );
            if (program.InstallationScope != Program._InstallationScope.None)
            {
                if (!(program.InstalledVersion.Equals("") ||
                      program.LatestVersion.Equals("")) &&
                    VersionsUtilities.IsVersionNewer(
                        program.LatestVersion,
                        program.InstalledVersion
                    ))
                {
                    label_latestVersion.Foreground = (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__DARK_GREEN_SOLID_COLOR_BRUSH
                    );
                }
            }
            comboBox_installedFor.SelectedItem =
                EnumsUtilities.GetHumanReadableStringFromEnumItem(
                    program.InstallationScope
                );
            checkBox_configureProgramUpdateCheck.IsChecked = false;
            textBox_webPageUrl.Text = "";
            if (program.IsUpdateCheckConfigured)
            {
                checkBox_configureProgramUpdateCheck.IsChecked = true;
                textBox_webPageUrl.Text = program.WebPageUrl;
                switch (program.VersionSearchMethod)
                {
                    case Program._VersionSearchMethod.SearchInTheContentOfHtmlElementWithId:
                        radioButton_searchInTheContentOfHtmlElementWithId.IsChecked = true;
                        textBox_searchInTheContentOfHtmlElementWithIdParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchInTheContentOfHtmlElementsMatchingXPath:
                        radioButton_searchInTheContentOfHtmlElementsMatchingXPath.IsChecked = true;
                        textBox_searchInTheContentOfHtmlElementsMatchingXPathParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyInTheWebPage:
                        radioButton_searchGloballyInTheWebPage.IsChecked = true;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyFromTextWithinWebPage:
                        radioButton_searchGloballyInTheWebPage.IsChecked = true;
                        checkBox_searchFromTextWithinWebPage.IsChecked = true;
                        textBox_searchFromTextWithinWebPageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyUntilTextWithinWebPage:
                        radioButton_searchGloballyInTheWebPage.IsChecked = true;
                        checkBox_searchUntilTextWithinWebPage.IsChecked = true;
                        textBox_searchUntilTextWithinWebPageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        break;
                    case Program._VersionSearchMethod.SearchGloballyFromTextUntilTextWithinWebPage:
                        radioButton_searchGloballyInTheWebPage.IsChecked = true;
                        checkBox_searchFromTextWithinWebPage.IsChecked = true;
                        checkBox_searchUntilTextWithinWebPage.IsChecked = true;
                        textBox_searchFromTextWithinWebPageParameter.Text =
                            program.VersionSearchMethodArgument1;
                        textBox_searchUntilTextWithinWebPageParameter.Text =
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
                    EnumsUtilities.GetHumanReadableStringFromEnumItem(
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
            string programLatestVersion = "";
            Program._InstallationScope installationScope = GetSelectedInstallationScope();
            string versionSearchMethodArgument1;
            string versionSearchMethodArgument2;
            Program._VersionSearchMethod versionSearchMethod = GetSelectedVersionSearchMethod(
                out versionSearchMethodArgument1,
                out versionSearchMethodArgument2
            );
            Program._VersionSearchBehavior versionSearchBehavior = GetSelectedVersionSearchBehavior();
            Program._WebPagePostLoadDelay webPagePostLoadDelay = GetSelectedWebPagePostLoadDelay();
            List<WebPageElementLocatingInstruction> locatingInstructionsOfWebPageElementsToSimulateAClickOn =
                GetSelectedLocatingInstructionsOfWebPageElementsToSimulateAClickOn();
            Program._UpdateCheckConfigurationStatus updateCheckConfigurationStatus =
                Program._UpdateCheckConfigurationStatus.Unknown;
            Program._UpdateCheckConfigurationError updateCheckConfigurationError =
                Program._UpdateCheckConfigurationError.None;
            if (programToEdit != null &&
                (checkBox_configureProgramUpdateCheck.IsChecked == programToEdit.IsUpdateCheckConfigured &&
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
                programLatestVersion = programToEdit.LatestVersion;
                updateCheckConfigurationStatus = programToEdit.UpdateCheckConfigurationStatus;
                updateCheckConfigurationError = programToEdit.UpdateCheckConfigurationError;
            }
            return new Program(
                textBox_programName.Text.Trim(),
                textBox_installedVersion.Text.Trim(),
                programLatestVersion,
                installationScope,
                (bool)checkBox_configureProgramUpdateCheck.IsChecked,
                textBox_webPageUrl.Text.Trim(),
                versionSearchMethod,
                versionSearchMethodArgument1,
                versionSearchMethodArgument2,
                (bool)checkBox_treatAStandaloneNumberAsAVersion.IsChecked,
                versionSearchBehavior,
                webPagePostLoadDelay,
                locatingInstructionsOfWebPageElementsToSimulateAClickOn,
                (bool)checkBox_detectAutomatically.IsChecked,
                updateCheckConfigurationStatus,
                updateCheckConfigurationError,
                (programToEdit != null ? programToEdit.IsHidden : false)
            );
        }
        private void ChangeLastProgramUpdateCheckConfigurationStatusMessageAndIconAccordingToError(Program._UpdateCheckConfigurationError? updateCheckConfigurationError)
        {
            ThreadsUtilities.RunOnAnotherThread(
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
                                    EnumsUtilities.GetHumanReadableStringFromEnumItem(
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
        public Program GetNewOrUpdatedProgram()
        {
            return newOrUpdatedProgram;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
