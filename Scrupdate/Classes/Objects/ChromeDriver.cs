// Copyright © 2021-2025 Matan Brightbert
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
        public class ElementWasNotFoundWithinTheWebpageException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Element was not found within the webpage!";
            public ElementWasNotFoundWithinTheWebpageException() : base(EXCEPTION_MESSAGE) { }
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
        public void Open(bool openInHeadlessMode, bool useIncognitoMode)
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
                if (useIncognitoMode)
                    chromeOptions.AddArgument("--incognito");
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
        public void NavigateToAWebpage(string webpageUrl)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            chromeDriver.Navigate().GoToUrl(webpageUrl);
        }
        public void ClickOnAnElementWithinTheWebpage(WebpageElementLocatingInstruction webpageElementLocatingInstruction)
        {
            ClickOnAnElementWithinTheWebpage(
                webpageElementLocatingInstruction,
                null
            );
        }
        public void ClickOnAnElementWithinTheWebpage(WebpageElementLocatingInstruction webpageElementLocatingInstruction,
                                                     CancellationToken? cancellationToken)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (!googleChromeBrowserIsOpen)
                throw new GoogleChromeBrowserIsNotOpenException();
            IWebElement webpageElement = null;
            try
            {
                switch (webpageElementLocatingInstruction.LocatingMethod)
                {
                    case WebpageElementLocatingInstruction._LocatingMethod.ByHtmlElementId:
                        webpageElement = chromeDriver.FindElement(
                            By.Id(webpageElementLocatingInstruction.MethodArgument)
                        );
                        break;
                    case WebpageElementLocatingInstruction._LocatingMethod.ByXPath:
                        webpageElement = chromeDriver.FindElement(
                            By.XPath(webpageElementLocatingInstruction.MethodArgument)
                        );
                        break;
                    case WebpageElementLocatingInstruction._LocatingMethod.ByInnerText:
                        StringBuilder xPath = new StringBuilder(
                            22 + webpageElementLocatingInstruction.MethodArgument.Length + 3
                        );
                        xPath
                            .Append("//*[contains(text(), '")
                            .Append(webpageElementLocatingInstruction.MethodArgument)
                            .Append("')]");
                        webpageElement = chromeDriver.FindElement(By.XPath(xPath.ToString()));
                        if (webpageElementLocatingInstruction.MatchExactText &&
                            !webpageElementLocatingInstruction.MethodArgument.Equals(
                                webpageElement.GetDomProperty("innerText").Trim()
                            ))
                        {
                            webpageElement = null;
                        }
                        break;
                }
            }
            catch
            {
                throw new ElementWasNotFoundWithinTheWebpageException();
            }
            if (webpageElement == null)
                throw new ElementWasNotFoundWithinTheWebpageException();
            try
            {
                webpageElement.Click();
            }
            catch
            {
                throw new FailedToPerformAClickOnTheElementException();
            }
            int webpageElementLocatingInterval = 0;
            switch (webpageElementLocatingInstruction.LocatingInterval)
            {
                case WebpageElementLocatingInstruction._LocatingInterval._1Ms:
                    webpageElementLocatingInterval = 1;
                    break;
                case WebpageElementLocatingInstruction._LocatingInterval._10Ms:
                    webpageElementLocatingInterval = 10;
                    break;
                case WebpageElementLocatingInstruction._LocatingInterval._100Ms:
                    webpageElementLocatingInterval = 100;
                    break;
                case WebpageElementLocatingInstruction._LocatingInterval._250Ms:
                    webpageElementLocatingInterval = 250;
                    break;
                case WebpageElementLocatingInstruction._LocatingInterval._500Ms:
                    webpageElementLocatingInterval = 500;
                    break;
                case WebpageElementLocatingInstruction._LocatingInterval._1000Ms:
                    webpageElementLocatingInterval = 1000;
                    break;
            }
            cancellationToken.Value.WaitHandle.WaitOne(
                webpageElementLocatingInterval
            );
        }
        public string GetAllTextWithinTheWebpage()
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
