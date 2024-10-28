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




using System.Text;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using Scrupdate.Classes.Objects;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;


namespace Scrupdate.UiElements.Windows
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window, INotifyPropertyChanged
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string DIRECTORY_NAME__DOCS = "docs";
        private const string FILE_NAME__SCRUPDATE_USER_MANUAL_PDF = "Scrupdate User Manual.pdf";
        private const string FILE_NAME__XML_PATH_LANGUAGE_PDF = "The XML Path Language (XPath).pdf";
        private const string ERROR_DIALOG_TITLE__ERROR = "Error";
        private const string ERROR_DIALOG_MESSAGE__UNABLE_TO_OPEN_THE_FILE = "Unable to Open the File!";
        private const string ERROR_DIALOG_MESSAGE__UNABLE_TO_OPEN_THE_HYPER_LINK = "Unable to Open the Hyper-Link!";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum HelpChapterMenuTab
        {
            None,
            Introduction,
            GettingStarted,
            UserInterface,
            Tips,
            Notes,
            References
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly string scrupdateUserManualPdfFilePath =
            (new StringBuilder(
                256 + 1 + DIRECTORY_NAME__DOCS.Length + 1 + FILE_NAME__SCRUPDATE_USER_MANUAL_PDF.Length
            ))
                .Append(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .Append('\\')
                .Append(DIRECTORY_NAME__DOCS)
                .Append('\\')
                .Append(FILE_NAME__SCRUPDATE_USER_MANUAL_PDF)
                .ToString();
        public static readonly string xmlPathLanguagePdfFilePath =
            (new StringBuilder(
                256 + 1 + DIRECTORY_NAME__DOCS.Length + 1 + FILE_NAME__XML_PATH_LANGUAGE_PDF.Length
            ))
                .Append(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .Append('\\')
                .Append(DIRECTORY_NAME__DOCS)
                .Append('\\')
                .Append(FILE_NAME__XML_PATH_LANGUAGE_PDF)
                .ToString();
        public static readonly DependencyProperty SelectedHelpChapterMenuTabProperty = DependencyProperty.Register(
            nameof(SelectedHelpChapterMenuTab),
            typeof(HelpChapterMenuTab),
            typeof(HelpWindow),
            new PropertyMetadata(HelpChapterMenuTab.None)
        );
        private CancellableThread panelBackgroundFlashingCancellableThread;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSizeOfWindow { get; private set; }
        public HelpChapterMenuTab SelectedHelpChapterMenuTab
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (HelpChapterMenuTab)GetValue(SelectedHelpChapterMenuTabProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(SelectedHelpChapterMenuTabProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(SelectedHelpChapterMenuTab))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public HelpWindow()
        {
            InitializeComponent();
            BaseSizeOfWindow = new Size(Width, Height);
            WindowUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSizeOfWindow,
                App.WindowsRenderingScale
            );
            SelectedHelpChapterMenuTab = HelpChapterMenuTab.Introduction;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnWindowClosingEvent(object sender, CancelEventArgs e)
        {
            panelBackgroundFlashingCancellableThread?.RequestCancellation();
            Owner?.Activate();
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_introductionHelpChapter)
            {
                SelectedHelpChapterMenuTab = HelpChapterMenuTab.Introduction;
                scrollViewer_helpContent.ScrollToTop();
            }
            else if (senderButton == button_gettingStartedHelpChapter)
            {
                SelectedHelpChapterMenuTab = HelpChapterMenuTab.GettingStarted;
                scrollViewer_helpContent.ScrollToTop();
            }
            else if (senderButton == button_userInterfaceHelpChapter)
            {
                SelectedHelpChapterMenuTab = HelpChapterMenuTab.UserInterface;
                scrollViewer_helpContent.ScrollToTop();
            }
            else if (senderButton == button_tipsHelpChapter)
            {
                SelectedHelpChapterMenuTab = HelpChapterMenuTab.Tips;
                scrollViewer_helpContent.ScrollToTop();
            }
            else if (senderButton == button_notesHelpChapter)
            {
                SelectedHelpChapterMenuTab = HelpChapterMenuTab.Notes;
                scrollViewer_helpContent.ScrollToTop();
            }
            else if (senderButton == button_referencesHelpChapter)
            {
                SelectedHelpChapterMenuTab = HelpChapterMenuTab.References;
                scrollViewer_helpContent.ScrollToTop();
            }
        }
        private void OnHyperlinkClickEvent(object sender, RoutedEventArgs e)
        {
            Hyperlink senderHyperlink = (Hyperlink)sender;
            if (senderHyperlink == hyperlink_goToNote1 ||
                senderHyperlink == hyperlink_goToNote2 ||
                senderHyperlink == hyperlink_goToNote3 ||
                senderHyperlink == hyperlink_goToNote4 ||
                senderHyperlink == hyperlink_goToNote5 ||
                senderHyperlink == hyperlink_goToNote6 ||
                senderHyperlink == hyperlink_goToNote7 ||
                senderHyperlink == hyperlink_goToNote8 ||
                senderHyperlink == hyperlink_goToReference1 ||
                senderHyperlink == hyperlink_goBackFromNote1 ||
                senderHyperlink == hyperlink_goBackFromNote2 ||
                senderHyperlink == hyperlink_goBackFromNote3 ||
                senderHyperlink == hyperlink_goBackFromNote4 ||
                senderHyperlink == hyperlink_goBackFromNote5 ||
                senderHyperlink == hyperlink_goBackFromNote6 ||
                senderHyperlink == hyperlink_goBackFromNote7 ||
                senderHyperlink == hyperlink_goBackFromNote8 ||
                senderHyperlink == hyperlink_goBackFromReference1)
            {
                if (senderHyperlink == hyperlink_goToNote1 ||
                    senderHyperlink == hyperlink_goToNote2 ||
                    senderHyperlink == hyperlink_goToNote3 ||
                    senderHyperlink == hyperlink_goToNote4 ||
                    senderHyperlink == hyperlink_goToNote5 ||
                    senderHyperlink == hyperlink_goToNote6 ||
                    senderHyperlink == hyperlink_goToNote7 ||
                    senderHyperlink == hyperlink_goToNote8 ||
                    senderHyperlink == hyperlink_goToReference1)
                {
                    HelpChapterMenuTab helpChapterMenuTabToSelect;
                    Panel panelToBringIntoViewAndFlash;
                    if (senderHyperlink == hyperlink_goToNote1)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Notes;
                        panelToBringIntoViewAndFlash = grid_note1;
                    }
                    else if (senderHyperlink == hyperlink_goToNote2)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Notes;
                        panelToBringIntoViewAndFlash = grid_note2;
                    }
                    else if (senderHyperlink == hyperlink_goToNote3)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Notes;
                        panelToBringIntoViewAndFlash = grid_note3;
                    }
                    else if (senderHyperlink == hyperlink_goToNote4)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Notes;
                        panelToBringIntoViewAndFlash = grid_note4;
                    }
                    else if (senderHyperlink == hyperlink_goToNote5)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Notes;
                        panelToBringIntoViewAndFlash = grid_note5;
                    }
                    else if (senderHyperlink == hyperlink_goToNote6)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Notes;
                        panelToBringIntoViewAndFlash = grid_note6;
                    }
                    else if (senderHyperlink == hyperlink_goToNote7)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Notes;
                        panelToBringIntoViewAndFlash = grid_note7;
                    }
                    else if (senderHyperlink == hyperlink_goToNote8)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Notes;
                        panelToBringIntoViewAndFlash = grid_note8;
                    }
                    else
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.References;
                        panelToBringIntoViewAndFlash = grid_reference1;
                    }
                    SelectedHelpChapterMenuTab = helpChapterMenuTabToSelect;
                    panelToBringIntoViewAndFlash.BringIntoView();
                    FlashBackgroundOfPanel(
                        panelToBringIntoViewAndFlash,
                        (SolidColorBrush)Application.Current.FindResource(
                            App.RESOURCE_KEY__PALE_ORANGE_SOLID_COLOR_BRUSH
                        ),
                        300,
                        2
                    );
                }
                else
                {
                    HelpChapterMenuTab helpChapterMenuTabToSelect;
                    Hyperlink hyperlinkToBringIntoView;
                    if (senderHyperlink == hyperlink_goBackFromNote1)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Introduction;
                        hyperlinkToBringIntoView = hyperlink_goToNote1;
                    }
                    else if (senderHyperlink == hyperlink_goBackFromNote2)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.Introduction;
                        hyperlinkToBringIntoView = hyperlink_goToNote2;
                    }
                    else if (senderHyperlink == hyperlink_goBackFromNote3)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.GettingStarted;
                        hyperlinkToBringIntoView = hyperlink_goToNote3;
                    }
                    else if (senderHyperlink == hyperlink_goBackFromNote4)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.GettingStarted;
                        hyperlinkToBringIntoView = hyperlink_goToNote4;
                    }
                    else if (senderHyperlink == hyperlink_goBackFromNote5)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.GettingStarted;
                        hyperlinkToBringIntoView = hyperlink_goToNote5;
                    }
                    else if (senderHyperlink == hyperlink_goBackFromNote6)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.GettingStarted;
                        hyperlinkToBringIntoView = hyperlink_goToNote6;
                    }
                    else if (senderHyperlink == hyperlink_goBackFromNote7)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.UserInterface;
                        hyperlinkToBringIntoView = hyperlink_goToNote7;
                    }
                    else if (senderHyperlink == hyperlink_goBackFromNote8)
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.UserInterface;
                        hyperlinkToBringIntoView = hyperlink_goToNote8;
                    }
                    else
                    {
                        helpChapterMenuTabToSelect = HelpChapterMenuTab.UserInterface;
                        hyperlinkToBringIntoView = hyperlink_goToReference1;
                    }
                    SelectedHelpChapterMenuTab = helpChapterMenuTabToSelect;
                    hyperlinkToBringIntoView.BringIntoView();
                }
            }
            else if (senderHyperlink == hyperlink_openXmlPathLanguagePdfFile ||
                     senderHyperlink == hyperlink_viewAsPdf)
            {
                string pdfFilePath =
                    (senderHyperlink == hyperlink_openXmlPathLanguagePdfFile ?
                        xmlPathLanguagePdfFilePath :
                        scrupdateUserManualPdfFilePath);
                try
                {
                    ProcessUtilities.RunFile(
                        pdfFilePath,
                        null,
                        true,
                        false,
                        false,
                        -1,
                        false,
                        false,
                        out _
                    );
                    if (senderHyperlink == hyperlink_viewAsPdf)
                        Close();
                }
                catch
                {
                    DialogUtilities.ShowErrorDialog(
                        ERROR_DIALOG_TITLE__ERROR,
                        ERROR_DIALOG_MESSAGE__UNABLE_TO_OPEN_THE_FILE,
                        this
                    );
                }
            }
        }
        private void OnHyperlinkRequestNavigateEvent(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink senderHyperlink = (Hyperlink)sender;
            if (senderHyperlink == hyperlink_googleChromeBrowserDownloadPage ||
                senderHyperlink == hyperlink_chromeDriverDownloadPage)
            {
                if (!ProcessUtilities.OpenUrlInDefaultWebBrowser(e.Uri.ToString()))
                {
                    DialogUtilities.ShowErrorDialog(
                        ERROR_DIALOG_TITLE__ERROR,
                        ERROR_DIALOG_MESSAGE__UNABLE_TO_OPEN_THE_HYPER_LINK,
                        this
                    );
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void FlashBackgroundOfPanel(Panel panel,
                                            Brush flashColor,
                                            int singleFlashDurationInMilliseconds,
                                            int flashesCount)
        {
            if (panelBackgroundFlashingCancellableThread == null)
            {
                Brush currentBackgroundColorOfPanel = panel.Background;
                int singleFlashHalfDurationInMilliseconds = singleFlashDurationInMilliseconds / 2;
                panelBackgroundFlashingCancellableThread = new CancellableThread(
                    cancellationToken =>
                        {
                            for (int i = 0; i < flashesCount; i++)
                            {
                                cancellationToken.WaitHandle.WaitOne(
                                    singleFlashHalfDurationInMilliseconds
                                );
                                ThreadingUtilities.RunOnAnotherThread(
                                    Dispatcher,
                                    () => panel.Background = flashColor
                                );
                                cancellationToken.WaitHandle.WaitOne(
                                    singleFlashHalfDurationInMilliseconds
                                );
                                ThreadingUtilities.RunOnAnotherThread(
                                    Dispatcher,
                                    () => panel.Background = currentBackgroundColorOfPanel
                                );
                            }
                            panelBackgroundFlashingCancellableThread = null;
                        }
                );
                panelBackgroundFlashingCancellableThread.Start();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
