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




using System.IO;
using Microsoft.Win32;


namespace Scrupdate.Classes.Utilities
{
    public static class GoogleChromeBrowserUtilities
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string GOOGLE_CHROME_BROWSER_EXECUTABLE_FILE_NAME = "chrome.exe";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GetPathOfInstalledGoogleChromeBrowserExecutableFile()
        {
            try
            {
                RegistryKey installedGoogleChromeBrowserAppPathRegistryKey = Registry.LocalMachine?
                    .OpenSubKey("SOFTWARE")?
                    .OpenSubKey("Microsoft")?
                    .OpenSubKey("Windows")?
                    .OpenSubKey("CurrentVersion")?
                    .OpenSubKey("App Paths")?
                    .OpenSubKey(GOOGLE_CHROME_BROWSER_EXECUTABLE_FILE_NAME);
                if (installedGoogleChromeBrowserAppPathRegistryKey == null)
                    return null;
                return (string)installedGoogleChromeBrowserAppPathRegistryKey.GetValue("");
            }
            catch
            {
                return null;
            }
        }
        public static string GetChecksumOfInstalledGoogleChromeBrowserExecutableFile()
        {
            try
            {
                string googleChromeBrowserExecutableFilePath =
                    GetPathOfInstalledGoogleChromeBrowserExecutableFile();
                if (googleChromeBrowserExecutableFilePath == null)
                    return null;
                return HashingUtilities.GetMD5Hash(
                    File.ReadAllBytes(googleChromeBrowserExecutableFilePath)
                );
            }
            catch
            {
                return null;
            }
        }
        public static bool IsGoogleChromeBrowserInstalled()
        {
            try
            {
                string googleChromeBrowserExecutableFilePath =
                    GetPathOfInstalledGoogleChromeBrowserExecutableFile();
                if (googleChromeBrowserExecutableFilePath == null)
                    return false;
                return File.Exists(googleChromeBrowserExecutableFilePath);
            }
            catch
            {
                return false;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
