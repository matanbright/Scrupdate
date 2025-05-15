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




using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Scrupdate.Classes.Objects
{
    public class Program
    {
        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum _InstallationScope
        {
            None,
            Everyone,
            User
        }
        public enum _VersionSearchMethod
        {
            Unknown,
            SearchWithinTheHtmlElementWithId,
            SearchWithinTheHtmlElementsThatMatchXPath,
            SearchGloballyWithinTheWebPage,
            SearchGloballyFromTextWithinTheWebPage,
            SearchGloballyUntilTextWithinTheWebPage,
            SearchGloballyFromTextUntilTextWithinTheWebPage
        }
        public enum _VersionSearchBehavior
        {
            Unknown,
            GetTheFirstVersionThatIsFound,
            GetTheFirstVersionThatIsFoundFromTheEnd,
            GetTheLatestVersionFromAllTheVersionsThatAreFound
        }
        public enum _WebPagePostLoadDelay
        {
            None,
            _100Ms,
            _250Ms,
            _500Ms,
            _1000Ms,
            _2000Ms,
            _3000Ms,
            _4000Ms,
            _5000Ms
        }
        public enum _UpdateCheckConfigurationStatus
        {
            Unknown,
            Invalid,
            Valid
        }
        public enum _UpdateCheckConfigurationError
        {
            None,
            GeneralFailure,
            WebPageDidNotRespond,
            HtmlElementWasNotFound,
            TextWasNotFoundWithinTheWebPage,
            NoVersionWasFound
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string Name { get; set; }
        public string InstalledVersion { get; set; }
        public string LatestVersion { get; set; }
        public _InstallationScope InstallationScope { get; set; }
        public bool IsAutomaticallyAdded { get; set; }
        public bool IsUpdateCheckConfigured { get; set; }
        public string WebPageUrl { get; set; }
        public _VersionSearchMethod VersionSearchMethod { get; set; }
        public string VersionSearchMethodArgument1 { get; set; }
        public string VersionSearchMethodArgument2 { get; set; }
        public bool TreatAStandaloneNumberAsAVersion { get; set; }
        public _VersionSearchBehavior VersionSearchBehavior { get; set; }
        public _WebPagePostLoadDelay WebPagePostLoadDelay { get; set; }
        public List<WebPageElementLocatingInstruction> LocatingInstructionsOfWebPageElementsToSimulateAClickOn { get; set; }
        public _UpdateCheckConfigurationStatus UpdateCheckConfigurationStatus { get; set; }
        public _UpdateCheckConfigurationError UpdateCheckConfigurationError { get; set; }
        public string SkippedVersion { get; set; }
        public bool IsHidden { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Program() :
            this(
                "",
                "",
                "",
                _InstallationScope.None,
                false
            )
        { }
        public Program(string name,
                       string installedVersion,
                       string latestVersion,
                       _InstallationScope installationScope,
                       bool isAutomaticallyAdded) :
            this(
                name,
                installedVersion,
                latestVersion,
                installationScope,
                isAutomaticallyAdded,
                false,
                "",
                _VersionSearchMethod.Unknown,
                "",
                "",
                false,
                _VersionSearchBehavior.Unknown,
                _WebPagePostLoadDelay.None,
                new List<WebPageElementLocatingInstruction>(),
                _UpdateCheckConfigurationStatus.Unknown,
                _UpdateCheckConfigurationError.None,
                "",
                false
            )
        { }
        public Program(string name,
                       string installedVersion,
                       string latestVersion,
                       _InstallationScope installationScope,
                       bool isAutomaticallyAdded,
                       bool isUpdateCheckConfigured,
                       string webPageUrl,
                       _VersionSearchMethod versionSearchMethod,
                       string versionSearchMethodArgument1,
                       string versionSearchMethodArgument2,
                       bool treatAStandaloneNumberAsAVersion,
                       _VersionSearchBehavior versionSearchBehavior,
                       _WebPagePostLoadDelay webPagePostLoadDelay,
                       List<WebPageElementLocatingInstruction> locatingInstructionsOfWebPageElementsToSimulateAClickOn,
                       _UpdateCheckConfigurationStatus updateCheckConfigurationStatus,
                       _UpdateCheckConfigurationError updateCheckConfigurationError,
                       string skippedVersion,
                       bool isHidden)
        {
            Name = name;
            InstalledVersion = installedVersion;
            LatestVersion = latestVersion;
            InstallationScope = installationScope;
            IsAutomaticallyAdded = isAutomaticallyAdded;
            IsUpdateCheckConfigured = isUpdateCheckConfigured;
            WebPageUrl = webPageUrl;
            VersionSearchMethod = versionSearchMethod;
            VersionSearchMethodArgument1 = versionSearchMethodArgument1;
            VersionSearchMethodArgument2 = versionSearchMethodArgument2;
            TreatAStandaloneNumberAsAVersion = treatAStandaloneNumberAsAVersion;
            VersionSearchBehavior = versionSearchBehavior;
            WebPagePostLoadDelay = webPagePostLoadDelay;
            LocatingInstructionsOfWebPageElementsToSimulateAClickOn =
                locatingInstructionsOfWebPageElementsToSimulateAClickOn;
            UpdateCheckConfigurationStatus = updateCheckConfigurationStatus;
            UpdateCheckConfigurationError = updateCheckConfigurationError;
            SkippedVersion = skippedVersion;
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
                    IsAutomaticallyAdded == otherProgram.IsAutomaticallyAdded &&
                    IsUpdateCheckConfigured == otherProgram.IsUpdateCheckConfigured &&
                    WebPageUrl.Equals(otherProgram.WebPageUrl) &&
                    VersionSearchMethod == otherProgram.VersionSearchMethod &&
                    VersionSearchMethodArgument1.Equals(otherProgram.VersionSearchMethodArgument1) &&
                    VersionSearchMethodArgument2.Equals(otherProgram.VersionSearchMethodArgument2) &&
                    TreatAStandaloneNumberAsAVersion == otherProgram.TreatAStandaloneNumberAsAVersion &&
                    VersionSearchBehavior == otherProgram.VersionSearchBehavior &&
                    WebPagePostLoadDelay == otherProgram.WebPagePostLoadDelay &&
                    LocatingInstructionsOfWebPageElementsToSimulateAClickOn.SequenceEqual(
                        otherProgram.LocatingInstructionsOfWebPageElementsToSimulateAClickOn
                    ) &&
                    UpdateCheckConfigurationStatus == otherProgram.UpdateCheckConfigurationStatus &&
                    UpdateCheckConfigurationError == otherProgram.UpdateCheckConfigurationError &&
                    SkippedVersion.Equals(otherProgram.SkippedVersion) &&
                    IsHidden == otherProgram.IsHidden);
        }
        public override int GetHashCode()
        {
            StringBuilder objectHashString = new StringBuilder(
                10 * 13 + 10 * LocatingInstructionsOfWebPageElementsToSimulateAClickOn.Count + 10 * 4
            );
            objectHashString
                .Append(Name.GetHashCode())
                .Append(InstalledVersion.GetHashCode())
                .Append(LatestVersion.GetHashCode())
                .Append(InstallationScope.GetHashCode())
                .Append(IsAutomaticallyAdded.GetHashCode())
                .Append(IsUpdateCheckConfigured.GetHashCode())
                .Append(WebPageUrl.GetHashCode())
                .Append(VersionSearchMethod.GetHashCode())
                .Append(VersionSearchMethodArgument1.GetHashCode())
                .Append(VersionSearchMethodArgument2.GetHashCode())
                .Append(TreatAStandaloneNumberAsAVersion.GetHashCode())
                .Append(VersionSearchBehavior.GetHashCode())
                .Append(WebPagePostLoadDelay.GetHashCode());
            foreach (WebPageElementLocatingInstruction locatingInstructionOfWebPageElementToSimulateAClickOn in
                     LocatingInstructionsOfWebPageElementsToSimulateAClickOn)
            {
                objectHashString.Append(
                    locatingInstructionOfWebPageElementToSimulateAClickOn.GetHashCode()
                );
            }
            objectHashString
                .Append(UpdateCheckConfigurationStatus.GetHashCode())
                .Append(UpdateCheckConfigurationError.GetHashCode())
                .Append(SkippedVersion.GetHashCode())
                .Append(IsHidden.GetHashCode());
            return objectHashString.ToString().GetHashCode();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
