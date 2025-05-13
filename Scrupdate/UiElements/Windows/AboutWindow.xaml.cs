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




using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Scrupdate.Classes.Utilities;


namespace Scrupdate.UiElements.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string FILE_NAME__SCRUPDATE_LICENSE_TXT = "LICENSE.txt";
        private const string FILE_NAME__SELENIUM_WEBDRIVER_LICENSE_TXT = "LICENSE.txt";
        private const string ERROR_DIALOG_TITLE__ERROR = "Error";
        private const string ERROR_DIALOG_MESSAGE__UNABLE_TO_OPEN_THE_FILE = "Unable to Open the File!";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly string scrupdateLicenseTextFilePath =
            (new StringBuilder(
                256 + 1 + FILE_NAME__SCRUPDATE_LICENSE_TXT.Length
            ))
                .Append(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .Append('\\')
                .Append(FILE_NAME__SCRUPDATE_LICENSE_TXT)
                .ToString();
        public static readonly string seleniumWebDriverLicenseTextFilePath =
            (new StringBuilder(
                256 + 1 + FILE_NAME__SELENIUM_WEBDRIVER_LICENSE_TXT.Length
            ))
                .Append(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .Append('\\')
                .Append(FILE_NAME__SELENIUM_WEBDRIVER_LICENSE_TXT)
                .ToString();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSize { get; private set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public AboutWindow()
        {
            InitializeComponent();
            BaseSize = new Size(Width, Height);
            WindowUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSize,
                App.WindowsRenderingScale
            );
            label_appVersion.Content = ((string)label_appVersion.Content).Replace(
                "{*}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(3)
            );
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnClosingEvent(object sender, CancelEventArgs e)
        {
            Owner?.Activate();
        }
        private void OnHyperlinkClickEvent(object sender, RoutedEventArgs e)
        {
            Hyperlink senderHyperlink = (Hyperlink)sender;
            if (senderHyperlink == hyperlink_viewScrupdateLicense ||
                senderHyperlink == hyperlink_viewSeleniumWebDriverLicense)
            {
                string licenseTextFilePath =
                    (senderHyperlink == hyperlink_viewScrupdateLicense ?
                        scrupdateLicenseTextFilePath :
                        seleniumWebDriverLicenseTextFilePath);
                try
                {
                    ProcessUtilities.RunFile(
                        licenseTextFilePath,
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
                        ERROR_DIALOG_MESSAGE__UNABLE_TO_OPEN_THE_FILE,
                        this
                    );
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
