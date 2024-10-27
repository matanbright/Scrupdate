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
using System.Text;
using System.IO;
using OpenQA.Selenium.Chrome;


namespace Scrupdate.Classes.Utilities
{
    public static class ChromeDriverUtilities
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string DIRECTORY_NAME__CHROMEDRIVER = "ChromeDriver";
        private const string FILE_NAME__CHROMEDRIVER_EXE = "chromedriver.exe";
        private const string CHROMEDRIVER_NAME = "ChromeDriver";
        private const int CHROMEDRIVER_VERSION_RETRIEVAL_TIMEOUT_IN_MILLISECONDS = 3000;
        private const int CHROMEDRIVER_VERSION_RETRIEVAL_MAXIMUM_COUNT_OF_WORDS_TO_CHECK = 100;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly string chromeDriverDirectoryPath = (new StringBuilder())
            .Append(ApplicationUtilities.dataDirectoryPath)
            .Append('\\')
            .Append(DIRECTORY_NAME__CHROMEDRIVER)
            .ToString();
        public static readonly string chromeDriverExecutableFilePath = (new StringBuilder())
            .Append(chromeDriverDirectoryPath)
            .Append('\\')
            .Append(FILE_NAME__CHROMEDRIVER_EXE)
            .ToString();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool InstallChromeDriver(string pathOfChromeDriverExecutableFileToInstall,
                                               out string chromeDriverInformation)
        {
            chromeDriverInformation = null;
            if (pathOfChromeDriverExecutableFileToInstall == null)
                throw new ArgumentNullException(nameof(pathOfChromeDriverExecutableFileToInstall));
            try
            {
                chromeDriverInformation = GetChromeDriverInformationFromAFile(
                    pathOfChromeDriverExecutableFileToInstall,
                    out _
                );
                if (chromeDriverInformation == null)
                    return false;
                if (!UninstallChromeDriver())
                    return false;
                if (!Directory.Exists(chromeDriverDirectoryPath))
                    Directory.CreateDirectory(chromeDriverDirectoryPath);
                File.Copy(
                    pathOfChromeDriverExecutableFileToInstall,
                    chromeDriverExecutableFilePath
                );
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool UninstallChromeDriver()
        {
            try
            {
                if (Directory.Exists(chromeDriverDirectoryPath))
                {
                    if (File.Exists(chromeDriverExecutableFilePath))
                        File.Delete(chromeDriverExecutableFilePath);
                    if (Directory.Exists(chromeDriverDirectoryPath))
                    {
                        if (Directory.GetFiles(chromeDriverDirectoryPath).Length == 0 &&
                            Directory.GetDirectories(chromeDriverDirectoryPath).Length == 0)
                        {
                            Directory.Delete(chromeDriverDirectoryPath);
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string GetInstalledChromeDriverInformation(out bool unableToAccessInstalledChromeDriverExecutableFile)
        {
            return GetChromeDriverInformationFromAFile(
                chromeDriverExecutableFilePath,
                out unableToAccessInstalledChromeDriverExecutableFile
            );
        }
        private static string GetChromeDriverInformationFromAFile(string filePath,
                                                                  out bool unableToAccessFile)
        {
            unableToAccessFile = false;
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            try
            {
                if (!File.Exists(filePath))
                    return null;
                string result;
                if (ProcessesUtilities.RunFile(
                        filePath,
                        "--version",
                        false,
                        true,
                        true,
                        CHROMEDRIVER_VERSION_RETRIEVAL_TIMEOUT_IN_MILLISECONDS,
                        true,
                        true,
                        out result
                    ) != 0)
                {
                    unableToAccessFile = true;
                    return null;
                }
                if (result == null)
                    return null;
                string[] splittedResult = result.Split(
                    new char[] { ' ' },
                    CHROMEDRIVER_VERSION_RETRIEVAL_MAXIMUM_COUNT_OF_WORDS_TO_CHECK
                );
                bool fileIsChromeDriver = false;
                string chromeDriverVersion = null;
                for (int i = 0; i < splittedResult.Length; i++)
                {
                    if (splittedResult[i].Contains(
                            CHROMEDRIVER_NAME,
                            StringComparison.CurrentCultureIgnoreCase
                        ))
                    {
                        fileIsChromeDriver = true;
                        chromeDriverVersion = VersionsUtilities.GetTheFirstFoundVersionFromString(result);
                        break;
                    }
                }
                if (!fileIsChromeDriver || chromeDriverVersion == null)
                    return null;
                StringBuilder chromeDriverInformation = new StringBuilder();
                chromeDriverInformation
                    .Append(CHROMEDRIVER_NAME)
                    .Append(' ')
                    .Append(chromeDriverVersion);
                return chromeDriverInformation.ToString();
            }
            catch
            {
                return null;
            }
        }
        public static string GetDefaultChromeDriverUserAgentString()
        {
            try
            {
                ChromeOptions tempChromeOptions = new ChromeOptions();
                tempChromeOptions.AddArgument("--headless");
                ChromeDriverService tempChromeDriverService = null;
                ChromeDriver tempChromeDriver = null;
                try
                {
                    tempChromeDriverService = ChromeDriverService.CreateDefaultService(
                        Path.GetDirectoryName(chromeDriverExecutableFilePath),
                        Path.GetFileName(chromeDriverExecutableFilePath)
                    );
                    tempChromeDriverService.HideCommandPromptWindow = true;
                    tempChromeDriver = new ChromeDriver(tempChromeDriverService, tempChromeOptions);
                    string defaultChromeDriverUserAgentString =
                        (string)tempChromeDriver.ExecuteScript("return navigator.userAgent;");
                    tempChromeDriver.Quit();
                    tempChromeDriver = null;
                    tempChromeDriverService.Dispose();
                    tempChromeDriverService = null;
                    return defaultChromeDriverUserAgentString.Replace("HeadlessChrome", "Chrome");
                }
                catch
                {
                    if (tempChromeDriver != null)
                    {
                        tempChromeDriver.Quit();
                        tempChromeDriver = null;
                    }
                    if (tempChromeDriverService != null)
                    {
                        tempChromeDriverService.Dispose();
                        tempChromeDriverService = null;
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
