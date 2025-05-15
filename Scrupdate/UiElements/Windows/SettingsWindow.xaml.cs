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
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Scrupdate.Classes.Objects;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;


namespace Scrupdate.UiElements.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string ERROR_DIALOG_TITLE__ERROR = "Error";
        private const string ERROR_DIALOG_MESSAGE__NO_DAYS_WERE_SELECTED = "No Days Were Selected!\r\n[In the 'General' tab under the 'Enable Scheduled Check for Program Updates' setting]";
        private const string ERROR_DIALOG_MESSAGE__AN_ERROR_HAS_OCCURRED_WHILE_INSTALLING_CHROMEDRIVER_OR_THE_SELECTED_EXECUTABLE_FILE_IS_NOT_A_VALID_CHROMEDRIVER = "An Error Has Occurred While Installing ChromeDriver\r\nor the Selected Executable File Is Not a Valid ChromeDriver!";
        private const string ERROR_DIALOG_MESSAGE__AN_ERROR_HAS_OCCURRED_WHILE_UNINSTALLING_CHROMEDRIVER = "An Error Has Occurred While Uninstalling ChromeDriver!";
        private const string ERROR_DIALOG_MESSAGE__NO_CHROMEDRIVER_USER_AGENT_STRING_WAS_SPECIFIED = "No ChromeDriver User-Agent String Was Specified!\r\n[In the 'ChromeDriver' tab under the 'ChromeDriver User-Agent String' Field]";
        private const string QUESTION_DIALOG_MESSAGE__DISABLE_SCANNING_FOR_INSTALLED_PROGRAMS = "Disable Scanning for Installed Programs?\r\n\r\n•  All existing programs in the list will be converted to manually-added programs.\r\n•  It cannot be undone automatically.";
        private const string QUESTION_DIALOG_MESSAGE__RESET_SETTINGS_TO_THEIR_DEFAULT_VALUES = "Reset Settings to Their Default Values?\r\n\r\n•  The program database and the installed ChromeDriver will not be affected.";
        private const string QUESTION_DIALOG_MESSAGE__RESET_ALL_SETTINGS_AND_DATA = "Reset All Settings and Data?\r\n\r\n•  This includes: the settings, the program database, and the installed ChromeDriver.\r\n•  It cannot be undone.\r\n•  Scrupdate will be closed.";
        private const string QUESTION_DIALOG_MESSAGE__UNINSTALL_CHROMEDRIVER = "Uninstall ChromeDriver?";
        private const string CHROMEDRIVER_INSTALLATION_STATUS_MESSAGE__UNKNOWN = "Unknown";
        private const string CHROMEDRIVER_INSTALLATION_STATUS_MESSAGE__NONE = "None";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum SettingsCategoryMenuTab
        {
            None,
            General,
            Appearance,
            ChromeDriver
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty SelectedSettingsCategoryMenuTabProperty = DependencyProperty.Register(
            nameof(SelectedSettingsCategoryMenuTab),
            typeof(SettingsCategoryMenuTab),
            typeof(SettingsWindow),
            new PropertyMetadata(SettingsCategoryMenuTab.None)
        );
        private Settings currentSettings;
        private Settings updatedSettings;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSize { get; private set; }
        public SettingsCategoryMenuTab SelectedSettingsCategoryMenuTab
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (SettingsCategoryMenuTab)GetValue(SelectedSettingsCategoryMenuTabProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(SelectedSettingsCategoryMenuTabProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(SelectedSettingsCategoryMenuTab))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public SettingsWindow(Settings currentSettings, bool programDatabaseIsOpen)
        {
            this.currentSettings = currentSettings;
            updatedSettings = null;
            InitializeComponent();
            BaseSize = new Size(Width, Height);
            WindowUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSize,
                App.WindowsRenderingScale
            );
            if (currentSettings.General.EnableScanningForInstalledPrograms && !programDatabaseIsOpen)
                checkBox_enableScanningForInstalledPrograms.IsEnabled = false;
            for (int i = 0; i < 24; i++)
            {
                StringBuilder programUpdatesScheduledCheckHourString = new StringBuilder(5);
                if (i < 10)
                    programUpdatesScheduledCheckHourString.Append('0');
                programUpdatesScheduledCheckHourString
                    .Append(i)
                    .Append(':')
                    .Append("00");
                comboBox_programUpdatesScheduledCheckHour.Items.Add(
                    programUpdatesScheduledCheckHourString.ToString()
                );
            }
            comboBox_windowsScalingFactor.Items.Add("No Scaling");
            comboBox_windowsScalingFactor.Items.Add("Auto");
            Rect displayWorkArea = SystemParameters.WorkArea;
            for (double i = 1.25D; true; i += 0.25D)
            {
                if (WindowUtilities.BASE_WINDOW_WIDTH_FOR_WINDOWS_SCALING * i + WindowUtilities.WINDOWS_MARGIN * 2.0D > displayWorkArea.Width ||
                    WindowUtilities.BASE_WINDOW_HEIGHT_FOR_WINDOWS_SCALING * i + WindowUtilities.WINDOWS_MARGIN * 2.0D > displayWorkArea.Height)
                {
                    break;
                }
                comboBox_windowsScalingFactor.Items.Add(i.ToString("0.00"));
            }
            for (int i = 2; i <= 4; i++)
            {
                comboBox_minimumVersionSegments.Items.Add(i.ToString());
                comboBox_maximumVersionSegments.Items.Add(i.ToString());
            }
            foreach (Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout chromeDriverPageLoadTimeout in
                     Enum.GetValues(typeof(Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout)))
            {
                comboBox_chromeDriverPageLoadTimeout.Items.Add(
                    EnumUtilities.GetHumanReadableStringFromEnumItem(
                        chromeDriverPageLoadTimeout
                    )
                );
            }
            ApplySettingsToUiControlsValues(currentSettings);
            SelectedSettingsCategoryMenuTab = SettingsCategoryMenuTab.General;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnClosingEvent(object sender, CancelEventArgs e)
        {
            Owner?.Activate();
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_generalSettingsCategory)
            {
                SelectedSettingsCategoryMenuTab = SettingsCategoryMenuTab.General;
                scrollViewer_settingsControls.ScrollToTop();
            }
            else if (senderButton == button_appearanceSettingsCategory)
            {
                SelectedSettingsCategoryMenuTab = SettingsCategoryMenuTab.Appearance;
                scrollViewer_settingsControls.ScrollToTop();
            }
            else if (senderButton == button_chromeDriverSettingsCategory)
            {
                bool unableToAccessInstalledChromeDriverExecutableFile;
                string installedChromeDriverInformation =
                    ChromeDriverUtilities.GetInstalledChromeDriverInformation(
                        out unableToAccessInstalledChromeDriverExecutableFile
                    );
                if (installedChromeDriverInformation == null)
                {
                    if (unableToAccessInstalledChromeDriverExecutableFile)
                    {
                        ChangeChromeDriverInstallationStatusMessage(
                            CHROMEDRIVER_INSTALLATION_STATUS_MESSAGE__UNKNOWN,
                            (SolidColorBrush)Application.Current.FindResource(
                                App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH
                            )
                        );
                    }
                    else
                    {
                        ChangeChromeDriverInstallationStatusMessage(
                            CHROMEDRIVER_INSTALLATION_STATUS_MESSAGE__NONE,
                            (SolidColorBrush)Application.Current.FindResource(
                                App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH
                            )
                        );
                    }
                }
                else
                {
                    ChangeChromeDriverInstallationStatusMessage(
                        installedChromeDriverInformation,
                        (SolidColorBrush)Application.Current.FindResource(
                            App.RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH
                        )
                    );
                }
                SelectedSettingsCategoryMenuTab = SettingsCategoryMenuTab.ChromeDriver;
                scrollViewer_settingsControls.ScrollToTop();
            }
            else if (senderButton == button_resetToDefaultSettings)
            {
                if (DialogUtilities.ShowQuestionDialog(
                        "",
                        QUESTION_DIALOG_MESSAGE__RESET_SETTINGS_TO_THEIR_DEFAULT_VALUES,
                        this
                    ) == true)
                {
                    updatedSettings = new Settings(
                        currentSettings.Cached,
                        new Settings.GeneralSettings(),
                        new Settings.AppearanceSettings(),
                        new Settings.ChromeDriverSettings()
                    );
                    DialogResult = true;
                    Close();
                }
            }
            else if (senderButton == button_resetAll)
            {
                if (DialogUtilities.ShowQuestionDialog(
                        "",
                        QUESTION_DIALOG_MESSAGE__RESET_ALL_SETTINGS_AND_DATA,
                        this
                    ) == true)
                {
                    updatedSettings = null;
                    DialogResult = true;
                    Close();
                }
            }
            else if (senderButton == button_InstallChromeDriver)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Executable Files (.exe)|*.exe";
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == true)
                {
                    if (!InstallChromeDriver(openFileDialog.FileName))
                    {
                        DialogUtilities.ShowErrorDialog(
                            ERROR_DIALOG_TITLE__ERROR,
                            ERROR_DIALOG_MESSAGE__AN_ERROR_HAS_OCCURRED_WHILE_INSTALLING_CHROMEDRIVER_OR_THE_SELECTED_EXECUTABLE_FILE_IS_NOT_A_VALID_CHROMEDRIVER,
                            this
                        );
                    }
                }
            }
            else if (senderButton == button_UninstallChromeDriver)
            {
                if (DialogUtilities.ShowQuestionDialog(
                        "",
                        QUESTION_DIALOG_MESSAGE__UNINSTALL_CHROMEDRIVER,
                        this
                    ) == true)
                {
                    if (!UninstallChromeDriver())
                    {
                        DialogUtilities.ShowErrorDialog(
                            ERROR_DIALOG_TITLE__ERROR,
                            ERROR_DIALOG_MESSAGE__AN_ERROR_HAS_OCCURRED_WHILE_UNINSTALLING_CHROMEDRIVER,
                            this
                        );
                    }
                }
            }
            else if (senderButton == button_save)
            {
                string errorDialogMessage = null;
                if (checkBox_enableScheduledCheckForProgramUpdates.IsChecked == true &&
                    !(checkBox_programUpdatesScheduledCheckDaySunday.IsChecked == true ||
                      checkBox_programUpdatesScheduledCheckDayMonday.IsChecked == true ||
                      checkBox_programUpdatesScheduledCheckDayTuesday.IsChecked == true ||
                      checkBox_programUpdatesScheduledCheckDayWednesday.IsChecked == true ||
                      checkBox_programUpdatesScheduledCheckDayThursday.IsChecked == true ||
                      checkBox_programUpdatesScheduledCheckDayFriday.IsChecked == true ||
                      checkBox_programUpdatesScheduledCheckDaySaturday.IsChecked == true))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_DAYS_WERE_SELECTED;
                }
                else if (radioButton_useCustomChromeDriverUserAgentString.IsChecked == true &&
                         textBox_customChromeDriverUserAgentString.Text.Trim().Equals(""))
                {
                    errorDialogMessage = ERROR_DIALOG_MESSAGE__NO_CHROMEDRIVER_USER_AGENT_STRING_WAS_SPECIFIED;
                }
                if (errorDialogMessage != null)
                {
                    DialogUtilities.ShowErrorDialog(
                        ERROR_DIALOG_TITLE__ERROR,
                        errorDialogMessage,
                        this
                    );
                    return;
                }
                updatedSettings = GetSettingsFromUiControlsValues();
                DialogResult = true;
                Close();
            }
            else if (senderButton == button_cancel)
                Close();
        }
        private void OnCheckBoxUncheckedEvent(object sender, RoutedEventArgs e)
        {
            CustomCheckBox senderCheckBox = (CustomCheckBox)sender;
            if (senderCheckBox == checkBox_enableScanningForInstalledPrograms)
            {
                if (!currentSettings.General.EnableScanningForInstalledPrograms ||
                    DialogUtilities.ShowQuestionDialog(
                        "",
                        QUESTION_DIALOG_MESSAGE__DISABLE_SCANNING_FOR_INSTALLED_PROGRAMS,
                        this
                    ) == true)
                {
                    checkBox_scanForInstalledProgramsAutomaticallyOnStart.IsChecked = false;
                }
                else
                    checkBox_enableScanningForInstalledPrograms.IsChecked = true;
            }
            else if (senderCheckBox == checkBox_enableScheduledCheckForProgramUpdates)
            {
                checkBox_programUpdatesScheduledCheckDaySunday.IsChecked = false;
                checkBox_programUpdatesScheduledCheckDayMonday.IsChecked = false;
                checkBox_programUpdatesScheduledCheckDayTuesday.IsChecked = false;
                checkBox_programUpdatesScheduledCheckDayWednesday.IsChecked = false;
                checkBox_programUpdatesScheduledCheckDayThursday.IsChecked = false;
                checkBox_programUpdatesScheduledCheckDayFriday.IsChecked = false;
                checkBox_programUpdatesScheduledCheckDaySaturday.IsChecked = false;
                comboBox_programUpdatesScheduledCheckHour.SelectedIndex = 0;
                checkBox_includeHiddenProgramsInProgramUpdatesScheduledCheckResults.IsChecked = false;
            }
        }
        private void OnComboBoxSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            CustomComboBox senderComboBox = (CustomComboBox)sender;
            if (senderComboBox == comboBox_maximumVersionSegments)
            {
                int previousSelectionOfMinimumVersionSegments = Convert.ToInt32(
                    (string)comboBox_minimumVersionSegments.SelectedItem
                );
                int currentSelectionOfMaximumVersionSegments = Convert.ToInt32(
                    (string)comboBox_maximumVersionSegments.SelectedItem
                );
                comboBox_minimumVersionSegments.Items.Clear();
                for (int i = 2; i <= currentSelectionOfMaximumVersionSegments; i++)
                    comboBox_minimumVersionSegments.Items.Add(i.ToString());
                comboBox_minimumVersionSegments.SelectedItem = Convert.ToString(
                    (previousSelectionOfMinimumVersionSegments > currentSelectionOfMaximumVersionSegments ?
                        currentSelectionOfMaximumVersionSegments :
                        previousSelectionOfMinimumVersionSegments)
                );
            }
        }
        private void OnRadioButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomRadioButton senderRadioButton = (CustomRadioButton)sender;
            if (senderRadioButton == radioButton_useDefaultChromeDriverUserAgentString)
                textBox_customChromeDriverUserAgentString.Text = "";
        }
        private void OnTextBoxTextChangedEvent(object sender, TextChangedEventArgs e)
        {
            CustomTextBox senderTextBox = (CustomTextBox)sender;
            if (senderTextBox == textBox_customChromeDriverUserAgentString)
            {
                if (Array.TrueForAll(
                        textBox_customChromeDriverUserAgentString.Text.ToCharArray(),
                        c => char.IsWhiteSpace(c)
                    ))
                {
                    textBox_customChromeDriverUserAgentString.Text = "";
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool InstallChromeDriver(string chromeDriverExecutableFilePath)
        {
            string chromeDriverInformation;
            if (ChromeDriverUtilities.InstallChromeDriver(
                    chromeDriverExecutableFilePath,
                    out chromeDriverInformation
                ))
            {
                ChangeChromeDriverInstallationStatusMessage(
                    chromeDriverInformation,
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__GREEN_SOLID_COLOR_BRUSH
                    )
                );
                return true;
            }
            return false;
        }
        private bool UninstallChromeDriver()
        {
            if (ChromeDriverUtilities.UninstallChromeDriver())
            {
                ChangeChromeDriverInstallationStatusMessage(
                    CHROMEDRIVER_INSTALLATION_STATUS_MESSAGE__NONE,
                    (SolidColorBrush)Application.Current.FindResource(
                        App.RESOURCE_KEY__RED_SOLID_COLOR_BRUSH
                    )
                );
                return true;
            }
            return false;
        }
        private void ChangeChromeDriverInstallationStatusMessage(string chromeDriverInstallationStatusMessage,
                                                                 Brush chromeDriverInstallationStatusMessageColor)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        label_chromeDriverInstallationStatusMessage.Content =
                            chromeDriverInstallationStatusMessage;
                        label_chromeDriverInstallationStatusMessage.Foreground =
                            chromeDriverInstallationStatusMessageColor;
                    }
            );
        }
        private void ApplySettingsToUiControlsValues(Settings settings)
        {
            checkBox_enableScanningForInstalledPrograms.IsChecked =
                settings.General.EnableScanningForInstalledPrograms;
            checkBox_scanForInstalledProgramsAutomaticallyOnStart.IsChecked =
                settings.General.ScanForInstalledProgramsAutomaticallyOnStart;
            checkBox_rememberLastProgramListOptions.IsChecked =
                settings.General.RememberLastProgramListOptions;
            checkBox_enableScheduledCheckForProgramUpdates.IsChecked =
                settings.General.EnableScheduledCheckForProgramUpdates;
            Settings.GeneralSettings.WeekDays scheduleDays =
                settings.General.ProgramUpdatesScheduledCheckDays;
            checkBox_programUpdatesScheduledCheckDaySunday.IsChecked =
                scheduleDays.HasFlag(Settings.GeneralSettings.WeekDays.Sunday);
            checkBox_programUpdatesScheduledCheckDayMonday.IsChecked =
                scheduleDays.HasFlag(Settings.GeneralSettings.WeekDays.Monday);
            checkBox_programUpdatesScheduledCheckDayTuesday.IsChecked =
                scheduleDays.HasFlag(Settings.GeneralSettings.WeekDays.Tuesday);
            checkBox_programUpdatesScheduledCheckDayWednesday.IsChecked =
                scheduleDays.HasFlag(Settings.GeneralSettings.WeekDays.Wednesday);
            checkBox_programUpdatesScheduledCheckDayThursday.IsChecked =
                scheduleDays.HasFlag(Settings.GeneralSettings.WeekDays.Thursday);
            checkBox_programUpdatesScheduledCheckDayFriday.IsChecked =
                scheduleDays.HasFlag(Settings.GeneralSettings.WeekDays.Friday);
            checkBox_programUpdatesScheduledCheckDaySaturday.IsChecked =
                scheduleDays.HasFlag(Settings.GeneralSettings.WeekDays.Saturday);
            comboBox_programUpdatesScheduledCheckHour.SelectedIndex =
                settings.General.ProgramUpdatesScheduledCheckHour;
            checkBox_includeHiddenProgramsInProgramUpdatesScheduledCheckResults.IsChecked =
                settings.General.IncludeHiddenProgramsInProgramUpdatesScheduledCheckResults;
            if (settings.Appearance.WindowsScalingFactor == 0.0D)
                comboBox_windowsScalingFactor.SelectedIndex = 1;
            else if (settings.Appearance.WindowsScalingFactor == 1.0D)
                comboBox_windowsScalingFactor.SelectedIndex = 0;
            else
            {
                comboBox_windowsScalingFactor.SelectedIndex =
                    2 + (int)((settings.Appearance.WindowsScalingFactor - 1.25D) / 0.25D);
            }
            comboBox_minimumVersionSegments.SelectedItem = Convert.ToString(
                settings.Appearance.MinimumVersionSegments
            );
            comboBox_maximumVersionSegments.SelectedItem = Convert.ToString(
                settings.Appearance.MaximumVersionSegments
            );
            checkBox_removeTrailingZeroSegmentsOfVersions.IsChecked =
                settings.Appearance.RemoveTrailingZeroSegmentsOfVersions;
            comboBox_chromeDriverPageLoadTimeout.SelectedItem =
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    settings.ChromeDriver.PageLoadTimeout
                );
            if (!settings.ChromeDriver.UseCustomUserAgentString)
                radioButton_useDefaultChromeDriverUserAgentString.IsChecked = true;
            else
            {
                radioButton_useCustomChromeDriverUserAgentString.IsChecked = true;
                textBox_customChromeDriverUserAgentString.Text = settings.ChromeDriver.CustomUserAgentString;
            }
        }
        private Settings GetSettingsFromUiControlsValues()
        {
            Settings.GeneralSettings.WeekDays scheduleDays = Settings.GeneralSettings.WeekDays.None;
            scheduleDays |=
                ((bool)checkBox_programUpdatesScheduledCheckDaySunday.IsChecked ?
                    (Settings.GeneralSettings.WeekDays)1 :
                    0);
            scheduleDays |=
                ((bool)checkBox_programUpdatesScheduledCheckDayMonday.IsChecked ?
                    (Settings.GeneralSettings.WeekDays)(1 << 1) :
                    0);
            scheduleDays |=
                ((bool)checkBox_programUpdatesScheduledCheckDayTuesday.IsChecked ?
                    (Settings.GeneralSettings.WeekDays)(1 << 2) :
                    0);
            scheduleDays |=
                ((bool)checkBox_programUpdatesScheduledCheckDayWednesday.IsChecked ?
                    (Settings.GeneralSettings.WeekDays)(1 << 3) :
                    0);
            scheduleDays |=
                ((bool)checkBox_programUpdatesScheduledCheckDayThursday.IsChecked ?
                    (Settings.GeneralSettings.WeekDays)(1 << 4) :
                    0);
            scheduleDays |=
                ((bool)checkBox_programUpdatesScheduledCheckDayFriday.IsChecked ?
                    (Settings.GeneralSettings.WeekDays)(1 << 5) :
                    0);
            scheduleDays |=
                ((bool)checkBox_programUpdatesScheduledCheckDaySaturday.IsChecked ?
                    (Settings.GeneralSettings.WeekDays)(1 << 6) :
                    0);
            Settings.GeneralSettings generalSettings = new Settings.GeneralSettings(
                (bool)checkBox_enableScanningForInstalledPrograms.IsChecked,
                (bool)checkBox_scanForInstalledProgramsAutomaticallyOnStart.IsChecked,
                (bool)checkBox_rememberLastProgramListOptions.IsChecked,
                (bool)checkBox_enableScheduledCheckForProgramUpdates.IsChecked,
                scheduleDays,
                comboBox_programUpdatesScheduledCheckHour.SelectedIndex,
                (bool)checkBox_includeHiddenProgramsInProgramUpdatesScheduledCheckResults.IsChecked
            );
            Settings.AppearanceSettings appearanceSettings = new Settings.AppearanceSettings(
                (comboBox_windowsScalingFactor.SelectedIndex == 0 ?
                    1.0D :
                    (comboBox_windowsScalingFactor.SelectedIndex == 1 ?
                        0.0D :
                        1.25D + ((comboBox_windowsScalingFactor.SelectedIndex - 2) * 0.25D))),
                Convert.ToInt32((string)comboBox_minimumVersionSegments.SelectedItem),
                Convert.ToInt32((string)comboBox_maximumVersionSegments.SelectedItem),
                (bool)checkBox_removeTrailingZeroSegmentsOfVersions.IsChecked
            );
            Settings.ChromeDriverSettings chromeDriverSettings = new Settings.ChromeDriverSettings(
                EnumUtilities.GetEnumItemFromHumanReadableString<Settings.ChromeDriverSettings.ChromeDriverPageLoadTimeout>(
                    (string)comboBox_chromeDriverPageLoadTimeout.SelectedItem
                ),
                (radioButton_useCustomChromeDriverUserAgentString.IsChecked == true),
                textBox_customChromeDriverUserAgentString.Text.Trim()
            );
            return new Settings(
                currentSettings.Cached,
                generalSettings,
                appearanceSettings,
                chromeDriverSettings
            );
        }
        public void RefreshAvailableWindowsScalingFactorSelections()
        {
            int currentWindowsScalingFactorComboBoxSelectionIndex = comboBox_windowsScalingFactor.SelectedIndex;
            comboBox_windowsScalingFactor.Items.Clear();
            comboBox_windowsScalingFactor.Items.Add("No Scaling");
            comboBox_windowsScalingFactor.Items.Add("Auto");
            Rect displayWorkArea = SystemParameters.WorkArea;
            for (double i = 1.25D; true; i += 0.25D)
            {
                if (WindowUtilities.BASE_WINDOW_WIDTH_FOR_WINDOWS_SCALING * i + WindowUtilities.WINDOWS_MARGIN * 2.0D > displayWorkArea.Width ||
                    WindowUtilities.BASE_WINDOW_HEIGHT_FOR_WINDOWS_SCALING * i + WindowUtilities.WINDOWS_MARGIN * 2.0D > displayWorkArea.Height)
                {
                    break;
                }
                comboBox_windowsScalingFactor.Items.Add(i.ToString("0.00"));
            }
            if (currentWindowsScalingFactorComboBoxSelectionIndex < comboBox_windowsScalingFactor.Items.Count)
                comboBox_windowsScalingFactor.SelectedIndex = currentWindowsScalingFactorComboBoxSelectionIndex;
            else if (comboBox_windowsScalingFactor.Items.Count == 2)
                comboBox_windowsScalingFactor.SelectedIndex = 0;
            else
                comboBox_windowsScalingFactor.SelectedIndex = comboBox_windowsScalingFactor.Items.Count - 1;
        }
        public Settings GetUpdatedSettings()
        {
            return updatedSettings;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
