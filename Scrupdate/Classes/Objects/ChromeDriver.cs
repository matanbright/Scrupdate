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
            private const string EXCEPTION_MESSAGE = "Unable to open Google Chrome™ browser! Please ensure that it is installed and that its version is compatible with the installed ChromeDriver.";
            public UnableToOpenGoogleChromeBrowserException() : base(EXCEPTION_MESSAGE) { }
        }
        public class GoogleChromeBrowserIsNotOpenException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Google Chrome™ browser is not open!";
            public GoogleChromeBrowserIsNotOpenException() : base(EXCEPTION_MESSAGE) { }
        }
        public class ElementWasNotFoundWithinTheWebPageException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Element was not found within the web page!";
            public ElementWasNotFoundWithinTheWebPageException() : base(EXCEPTION_MESSAGE) { }
        }
        public class FailedToPerformAClickOnTheElementException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Failed to perform a click on the element!";
            public FailedToPerformAClickOnTheElementException() : base(EXCEPTION_MESSAGE) { }
        }
        public class HtmlElementsWereNotFoundException : Exception
        {
            private const string EXCEPTION_MESSAGE = "HTML element(s) were not found!";
            public HtmlElementsWereNotFoundException() : base(EXCEPTION_MESSAGE) { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private volatile bool disposed;
        private string chromeDriverDirectoryPath;
        private string chromeDriverUserAgent;
        private int chromeDriverPageLoadTimeoutInMilliseconds;
        private ChromeDriverService chromeDriverService;
        private OpenQA.Selenium.Chrome.ChromeDriver chromeDriver;
        private bool googleChromeBrowserIsOpen;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ChromeDriver() : this(null, null, 0) { }
        public ChromeDriver(string chromeDriverDirectoryPath,
                            string chromeDriverUserAgent,
                            int chromeDriverPageLoadTimeoutInMilliseconds)
        {
            disposed = false;
            this.chromeDriverDirectoryPath = chromeDriverDirectoryPath;
            this.chromeDriverUserAgent = chromeDriverUserAgent;
            this.chromeDriverPageLoadTimeoutInMilliseconds = chromeDriverPageLoadTimeoutInMilliseconds;
            chromeDriverService = null;
            chromeDriver = null;
            googleChromeBrowserIsOpen = false;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Destructors /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ~ChromeDriver() => Dispose(false);
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
                    StringBuilder chromeUserAgentArgument = new StringBuilder(
                        13 + chromeDriverUserAgent.Length
                    );
                    chromeUserAgentArgument
                        .Append("--user-agent=")
                        .Append(chromeDriverUserAgent);
                    chromeOptions.AddArgument(chromeUserAgentArgument.ToString());
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
                try
                {
                    chromeDriverService = ChromeDriverService.CreateDefaultService(
                        chromeDriverDirectoryPath
                    );
                    chromeDriverService.HideCommandPromptWindow = true;
                    chromeDriver = new OpenQA.Selenium.Chrome.ChromeDriver(
                        chromeDriverService,
                        chromeOptions
                    );
                    if (chromeDriverPageLoadTimeoutInMilliseconds > 0)
                    {
                        chromeDriver.Manage().Timeouts().PageLoad = new TimeSpan(
                            0,
                            0,
                            0,
                            0,
                            chromeDriverPageLoadTimeoutInMilliseconds
                        );
                    }
                }
                catch
                {
                    if (chromeDriver != null)
                    {
                        chromeDriver.Quit();
                        chromeDriver = null;
                    }
                    if (chromeDriverService != null)
                    {
                        chromeDriverService.Dispose();
                        chromeDriverService = null;
                    }
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
            ClickOnAnElementWithinTheWebpage(
                webPageElementLocatingInstruction,
                null
            );
        }
        public void ClickOnAnElementWithinTheWebpage(WebPageElementLocatingInstruction webPageElementLocatingInstruction,
                                                     CancellationToken? cancellationToken)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IWebElement webPageElement = null;
            try
            {
                switch (webPageElementLocatingInstruction.LocatingMethod)
                {
                    case WebPageElementLocatingInstruction._LocatingMethod.ByHtmlElementId:
                        webPageElement = chromeDriver.FindElement(
                            By.Id(webPageElementLocatingInstruction.MethodArgument)
                        );
                        break;
                    case WebPageElementLocatingInstruction._LocatingMethod.ByXPath:
                        webPageElement = chromeDriver.FindElement(
                            By.XPath(webPageElementLocatingInstruction.MethodArgument)
                        );
                        break;
                    case WebPageElementLocatingInstruction._LocatingMethod.ByInnerText:
                        StringBuilder xPath = new StringBuilder(
                            22 + webPageElementLocatingInstruction.MethodArgument.Length + 3
                        );
                        xPath
                            .Append("//*[contains(text(), '")
                            .Append(webPageElementLocatingInstruction.MethodArgument)
                            .Append("')]");
                        webPageElement = chromeDriver.FindElement(By.XPath(xPath.ToString()));
                        if (webPageElementLocatingInstruction.MatchExactText &&
                            !webPageElementLocatingInstruction.MethodArgument.Equals(
                                webPageElement.GetDomProperty("innerText").Trim()
                            ))
                        {
                            webPageElement = null;
                        }
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
            int webPageElementLocatingInterval = 0;
            switch (webPageElementLocatingInstruction.LocatingInterval)
            {
                case WebPageElementLocatingInstruction._LocatingInterval._1Ms:
                    webPageElementLocatingInterval = 1;
                    break;
                case WebPageElementLocatingInstruction._LocatingInterval._10Ms:
                    webPageElementLocatingInterval = 10;
                    break;
                case WebPageElementLocatingInstruction._LocatingInterval._100Ms:
                    webPageElementLocatingInterval = 100;
                    break;
                case WebPageElementLocatingInstruction._LocatingInterval._250Ms:
                    webPageElementLocatingInterval = 250;
                    break;
                case WebPageElementLocatingInstruction._LocatingInterval._500Ms:
                    webPageElementLocatingInterval = 500;
                    break;
                case WebPageElementLocatingInstruction._LocatingInterval._1000Ms:
                    webPageElementLocatingInterval = 1000;
                    break;
            }
            cancellationToken.Value.WaitHandle.WaitOne(
                webPageElementLocatingInterval
            );
        }
        public string GetAllTextWithinTheWebPage()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IWebElement htmlRootElement = chromeDriver.FindElement(By.XPath("/*"));
            return htmlRootElement.GetDomProperty("innerText");
        }
        public string GetTextInsideHtmlElementById(string htmlElementId)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IWebElement htmlElement;
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
            return htmlElement.GetDomProperty("innerText");
        }
        public string[] GetTextsInsideHtmlElementsByXPath(string htmlElementsXPath)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IReadOnlyCollection<IWebElement> htmlElements;
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
                htmlElementsTexts.Add(htmlElement.GetDomProperty("innerText"));
            }
            return htmlElementsTexts.ToArray();
        }
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (chromeDriver != null)
                {
                    chromeDriver.Quit();
                    chromeDriver = null;
                }
                googleChromeBrowserIsOpen = false;
                if (chromeDriverService != null)
                {
                    chromeDriverService.Dispose();
                    chromeDriverService = null;
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
