﻿// Copyright © 2021 Matan Brightbert
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




using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrupdate.Classes.Objects
{
    public class Program
    {
        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum ProgramInstallationScope
        {
            None,
            Everyone,
            User
        }
        public enum ProgramVersionSearchMethod
        {
            Unknown,
            SearchInTheContentOfHtmlElementWithId,
            SearchInTheContentOfHtmlElementsMatchingXPath,
            SearchGloballyInTheWebPage,
            SearchGloballyFromTextWithinWebPage,
            SearchGloballyUntilTextWithinWebPage,
            SearchGloballyFromTextUntilTextWithinWebPage
        }
        public enum ProgramVersionSearchBehavior
        {
            Unknown,
            GetTheFirstVersionThatIsFound,
            GetTheFirstVersionThatIsFoundFromTheEnd,
            GetTheLatestVersionFromAllTheVersionsThatAreFound
        }
        public enum ProgramWebPagePostLoadDelay
        {
            _None,
            _100Ms,
            _250Ms,
            _500Ms,
            _1000Ms,
            _2000Ms,
            _3000Ms,
            _4000Ms,
            _5000Ms
        }
        public enum ProgramUpdateCheckConfigurationStatus
        {
            Unknown,
            Invalid,
            Valid
        }
        public enum ProgramUpdateCheckConfigurationError
        {
            None,
            GeneralFailure,
            WebPageDidNotRespond,
            HtmlElementWasNotFound,
            TextWasNotFoundWithinWebPage,
            NoVersionWasFound
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string Name { get; set; }
        public string InstalledVersion { get; set; }
        public string LatestVersion { get; set; }
        public ProgramInstallationScope InstallationScope { get; set; }
        public bool IsUpdateCheckConfigured { get; set; }
        public string WebPageUrl { get; set; }
        public ProgramVersionSearchMethod VersionSearchMethod { get; set; }
        public string VersionSearchMethodArgument1 { get; set; }
        public string VersionSearchMethodArgument2 { get; set; }
        public bool TreatAStandaloneNumberAsAVersion { get; set; }
        public ProgramVersionSearchBehavior VersionSearchBehavior { get; set; }
        public ProgramWebPagePostLoadDelay WebPagePostLoadDelay { get; set; }
        public List<WebPageElementLocatingInstruction> WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn { get; set; }
        public bool IsAutomaticallyAdded { get; set; }
        public ProgramUpdateCheckConfigurationStatus UpdateCheckConfigurationStatus { get; set; }
        public ProgramUpdateCheckConfigurationError UpdateCheckConfigurationError { get; set; }
        public bool IsHidden { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Program()
        {
            Name = "";
            InstalledVersion = "";
            LatestVersion = "";
            InstallationScope = ProgramInstallationScope.None;
            IsUpdateCheckConfigured = false;
            WebPageUrl = "";
            VersionSearchMethod = ProgramVersionSearchMethod.Unknown;
            VersionSearchMethodArgument1 = "";
            VersionSearchMethodArgument2 = "";
            TreatAStandaloneNumberAsAVersion = false;
            VersionSearchBehavior = ProgramVersionSearchBehavior.Unknown;
            WebPagePostLoadDelay = ProgramWebPagePostLoadDelay._None;
            WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn = new List<WebPageElementLocatingInstruction>();
            IsAutomaticallyAdded = false;
            UpdateCheckConfigurationStatus = ProgramUpdateCheckConfigurationStatus.Unknown;
            UpdateCheckConfigurationError = ProgramUpdateCheckConfigurationError.None;
            IsHidden = false;
        }
        public Program(string name, string installedVersion, string latestVersion, ProgramInstallationScope installationScope, bool isUpdateCheckConfigured, string webPageUrl, ProgramVersionSearchMethod versionSearchMethod, string versionSearchMethodArgument1, string versionSearchMethodArgument2, bool treatAStandaloneNumberAsAVersion, ProgramVersionSearchBehavior versionSearchBehavior, ProgramWebPagePostLoadDelay webPagePostLoadDelay, List<WebPageElementLocatingInstruction> webPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn, bool isAutomaticallyAdded, ProgramUpdateCheckConfigurationStatus updateCheckConfigurationStatus, ProgramUpdateCheckConfigurationError updateCheckConfigurationError, bool isHidden)
        {
            Name = name;
            InstalledVersion = installedVersion;
            LatestVersion = latestVersion;
            InstallationScope = installationScope;
            IsUpdateCheckConfigured = isUpdateCheckConfigured;
            WebPageUrl = webPageUrl;
            VersionSearchMethod = versionSearchMethod;
            VersionSearchMethodArgument1 = versionSearchMethodArgument1;
            VersionSearchMethodArgument2 = versionSearchMethodArgument2;
            TreatAStandaloneNumberAsAVersion = treatAStandaloneNumberAsAVersion;
            VersionSearchBehavior = versionSearchBehavior;
            WebPagePostLoadDelay = webPagePostLoadDelay;
            WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn = webPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn;
            IsAutomaticallyAdded = isAutomaticallyAdded;
            UpdateCheckConfigurationStatus = updateCheckConfigurationStatus;
            UpdateCheckConfigurationError = updateCheckConfigurationError;
            IsHidden = isHidden;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            Program otherProgram = (Program)obj;
            return (Name.Equals(otherProgram.Name) &&
                InstalledVersion.Equals(otherProgram.InstalledVersion) &&
                LatestVersion.Equals(otherProgram.LatestVersion) &&
                InstallationScope == otherProgram.InstallationScope &&
                IsUpdateCheckConfigured == otherProgram.IsUpdateCheckConfigured &&
                WebPageUrl.Equals(otherProgram.WebPageUrl) &&
                VersionSearchMethod == otherProgram.VersionSearchMethod &&
                VersionSearchMethodArgument1.Equals(otherProgram.VersionSearchMethodArgument1) &&
                VersionSearchMethodArgument2.Equals(otherProgram.VersionSearchMethodArgument2) &&
                TreatAStandaloneNumberAsAVersion == otherProgram.TreatAStandaloneNumberAsAVersion &&
                VersionSearchBehavior == otherProgram.VersionSearchBehavior &&
                WebPagePostLoadDelay == otherProgram.WebPagePostLoadDelay &&
                WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn.SequenceEqual(otherProgram.WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn) &&
                IsAutomaticallyAdded == otherProgram.IsAutomaticallyAdded &&
                UpdateCheckConfigurationStatus == otherProgram.UpdateCheckConfigurationStatus &&
                UpdateCheckConfigurationError == otherProgram.UpdateCheckConfigurationError &&
                IsHidden == otherProgram.IsHidden);
        }
        public override int GetHashCode()
        {
            StringBuilder objectHashingString = new StringBuilder();
            objectHashingString.Append(Name.GetHashCode());
            objectHashingString.Append(InstalledVersion.GetHashCode());
            objectHashingString.Append(LatestVersion.GetHashCode());
            objectHashingString.Append(InstallationScope.GetHashCode());
            objectHashingString.Append(IsUpdateCheckConfigured.GetHashCode());
            objectHashingString.Append(WebPageUrl.GetHashCode());
            objectHashingString.Append(VersionSearchMethod.GetHashCode());
            objectHashingString.Append(VersionSearchMethodArgument1.GetHashCode());
            objectHashingString.Append(VersionSearchMethodArgument2.GetHashCode());
            objectHashingString.Append(TreatAStandaloneNumberAsAVersion.GetHashCode());
            objectHashingString.Append(VersionSearchBehavior.GetHashCode());
            objectHashingString.Append(WebPagePostLoadDelay.GetHashCode());
            foreach (WebPageElementLocatingInstruction webPageElementLocatingInstructionOfWebPageElementToSimulateAClickOn in WebPageElementLocatingInstructionsOfWebPageElementsToSimulateAClickOn)
                objectHashingString.Append(webPageElementLocatingInstructionOfWebPageElementToSimulateAClickOn.GetHashCode());
            objectHashingString.Append(IsAutomaticallyAdded.GetHashCode());
            objectHashingString.Append(UpdateCheckConfigurationStatus.GetHashCode());
            objectHashingString.Append(UpdateCheckConfigurationError.GetHashCode());
            objectHashingString.Append(IsHidden.GetHashCode());
            return objectHashingString.ToString().GetHashCode();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
