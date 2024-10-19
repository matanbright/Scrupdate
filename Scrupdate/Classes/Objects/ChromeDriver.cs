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
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;


namespace Scrupdate.Classes.Objects
{
    public class ChromeDriver : IDisposable
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class UnableToOpenGoogleChromeBrowserException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE = "Unable to open Google Chrome™ browser! Please ensure that it is installed and that its version is compatible with the installed ChromeDriver.";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public UnableToOpenGoogleChromeBrowserException() : base(EXCEPTION_MESSAGE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class GoogleChromeBrowserIsNotOpenException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE = "Google Chrome™ browser is not open!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public GoogleChromeBrowserIsNotOpenException() : base(EXCEPTION_MESSAGE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class ElementWasNotFoundWithinTheWebPageException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE = "Element was not found within the web page!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public ElementWasNotFoundWithinTheWebPageException() : base(EXCEPTION_MESSAGE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class FailedToPerformAClickOnTheElementException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE = "Failed to perform a click on the element!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public FailedToPerformAClickOnTheElementException() : base(EXCEPTION_MESSAGE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public class HtmlElementsWereNotFoundException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE = "HTML element(s) were not found!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public HtmlElementsWereNotFoundException() : base(EXCEPTION_MESSAGE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private volatile bool disposed;
        private StringBuilder tempStringBuilder;
        private string chromeDriverDirectoryPath;
        private string chromeDriverUserAgent;
        private int chromeDriverPageLoadTimeoutInMilliseconds;
        private ChromeDriverService chromeDriverService;
        private OpenQA.Selenium.Chrome.ChromeDriver chromeDriver;
        private bool googleChromeBrowserIsOpen;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ChromeDriver() : this(null, null, 0) { }
        public ChromeDriver(string chromeDriverDirectoryPath, string chromeDriverUserAgent, int chromeDriverPageLoadTimeoutInMilliseconds)
        {
            disposed = false;
            tempStringBuilder = new StringBuilder();
            this.chromeDriverDirectoryPath = chromeDriverDirectoryPath;
            this.chromeDriverUserAgent = chromeDriverUserAgent;
            this.chromeDriverPageLoadTimeoutInMilliseconds = chromeDriverPageLoadTimeoutInMilliseconds;
            chromeDriver = null;
            googleChromeBrowserIsOpen = false;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Destructors /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ~ChromeDriver() => Dispose();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Open(bool openInHeadlessMode)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
            {
                ChromeOptions chromeOptions = new ChromeOptions();
                if (chromeDriverUserAgent != null && !chromeDriverUserAgent.Equals(""))
                {
                    tempStringBuilder.Clear().Append("--user-agent=").Append(chromeDriverUserAgent);
                    chromeOptions.AddArgument(tempStringBuilder.ToString());
                }
                if (openInHeadlessMode)
                {
                    chromeOptions.AddArgument("--headless");
                    chromeOptions.AddArgument("--disable-gpu");
                    chromeOptions.AddArgument("--start-maximized");
                    chromeOptions.AddArgument("--window-size=1920,1080");
                    chromeOptions.AddArgument("--disable-extensions");
                    chromeOptions.AddArgument("--mute-audio");
                    chromeOptions.AddArgument("--blink-settings=imagesEnabled=false");
                    chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
                    chromeOptions.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
                }
                ChromeDriverService chromeDriverService = null;
                try
                {
                    chromeDriverService = ChromeDriverService.CreateDefaultService(chromeDriverDirectoryPath);
                    chromeDriverService.HideCommandPromptWindow = true;
                    chromeDriver = new OpenQA.Selenium.Chrome.ChromeDriver(chromeDriverService, chromeOptions);
                    if (chromeDriverPageLoadTimeoutInMilliseconds > 0)
                        chromeDriver.Manage().Timeouts().PageLoad = new TimeSpan(0, 0, 0, 0, chromeDriverPageLoadTimeoutInMilliseconds);
                }
                catch
                {
                    chromeDriver?.Quit();
                    chromeDriverService?.Dispose();
                    chromeDriverService = null;
                    throw new UnableToOpenGoogleChromeBrowserException();
                }
                googleChromeBrowserIsOpen = true;
            }
        }
        public void Quit()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (googleChromeBrowserIsOpen)
            {
                chromeDriver.Quit();
                chromeDriver = null;
                googleChromeBrowserIsOpen = false;
            }
        }
        public void NavigateToAWebPage(string webPageUrl)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            chromeDriver.Navigate().GoToUrl(webPageUrl);
        }
        public void ClickOnAnElementWithinTheWebpage(WebPageElementLocatingInstruction webPageElementLocatingInstruction)
        {
            ClickOnAnElementWithinTheWebpage(webPageElementLocatingInstruction, null);
        }
        public void ClickOnAnElementWithinTheWebpage(WebPageElementLocatingInstruction webPageElementLocatingInstruction, CancellationToken? cancellationToken)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IWebElement webPageElement = null;
            try
            {
                switch (webPageElementLocatingInstruction.Method)
                {
                    case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionMethod.ByHtmlElementId:
                        webPageElement = chromeDriver.FindElement(By.Id(webPageElementLocatingInstruction.MethodArgument));
                        break;
                    case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionMethod.ByXPath:
                        webPageElement = chromeDriver.FindElement(By.XPath(webPageElementLocatingInstruction.MethodArgument));
                        break;
                    case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionMethod.ByInnerText:
                        tempStringBuilder.Clear().Append("//*[contains(text(), '").Append(webPageElementLocatingInstruction.MethodArgument).Append("')]");
                        webPageElement = chromeDriver.FindElement(By.XPath(tempStringBuilder.ToString()));
                        if (webPageElementLocatingInstruction.MatchExactText && !webPageElementLocatingInstruction.MethodArgument.Equals(webPageElement.GetAttribute("innerText").Trim()))
                            webPageElement = null;
                        break;
                }
            }
            catch
            {
                throw new ElementWasNotFoundWithinTheWebPageException();
            }
            if (webPageElement == null)
                throw new ElementWasNotFoundWithinTheWebPageException();
            try
            {
                webPageElement.Click();
            }
            catch
            {
                throw new FailedToPerformAClickOnTheElementException();
            }
            int webPageElementLocatingInstructionDuration = 0;
            switch (webPageElementLocatingInstruction.Duration)
            {
                case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionDuration._1Ms:
                    webPageElementLocatingInstructionDuration = 1;
                    break;
                case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionDuration._10Ms:
                    webPageElementLocatingInstructionDuration = 10;
                    break;
                case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionDuration._100Ms:
                    webPageElementLocatingInstructionDuration = 100;
                    break;
                case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionDuration._250Ms:
                    webPageElementLocatingInstructionDuration = 250;
                    break;
                case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionDuration._500Ms:
                    webPageElementLocatingInstructionDuration = 500;
                    break;
                case WebPageElementLocatingInstruction.WebPageElementLocatingInstructionDuration._1000Ms:
                    webPageElementLocatingInstructionDuration = 1000;
                    break;
            }
            cancellationToken.Value.WaitHandle.WaitOne(webPageElementLocatingInstructionDuration);
        }
        public string GetAllTextWithinWebPage()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IWebElement htmlRootElement = chromeDriver.FindElement(By.XPath("/*"));
            return htmlRootElement.GetAttribute("innerText");
        }
        public string GetTextInsideHtmlElementById(string htmlElementId)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IWebElement htmlElement = null;
            try
            {
                htmlElement = chromeDriver.FindElement(By.Id(htmlElementId));
            }
            catch
            {
                throw new HtmlElementsWereNotFoundException();
            }
            if (htmlElement == null)
                throw new HtmlElementsWereNotFoundException();
            return htmlElement.GetAttribute("innerText");
        }
        public string[] GetTextsInsideHtmlElementsByXPath(string htmlElementsXPath)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IReadOnlyCollection<IWebElement> htmlElements = null;
            try
            {
                htmlElements = chromeDriver.FindElements(By.XPath(htmlElementsXPath));
            }
            catch
            {
                throw new HtmlElementsWereNotFoundException();
            }
            if (htmlElements == null || htmlElements.Count == 0)
                throw new HtmlElementsWereNotFoundException();
            List<string> htmlElementsTexts = new List<string>(htmlElements.Count);
            for (IEnumerator<IWebElement> i = htmlElements.GetEnumerator(); i.MoveNext();)
            {
                IWebElement htmlElement = i.Current;
                htmlElementsTexts.Add(htmlElement.GetAttribute("innerText"));
            }
            return htmlElementsTexts.ToArray();
        }
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                chromeDriver?.Quit();
                chromeDriver = null;
                googleChromeBrowserIsOpen = false;
                chromeDriverService?.Dispose();
                chromeDriverService = null;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
