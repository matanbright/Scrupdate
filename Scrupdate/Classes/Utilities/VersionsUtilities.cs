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

namespace Scrupdate.Classes.Utilities
{
    public static class VersionsUtilities
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class InvalidVersionStringException : Exception
        {
            // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            private const string EXCEPTION_MESSAGE = "Invalid version string!";
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            public InvalidVersionStringException() : base(EXCEPTION_MESSAGE) { }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const int MINIMUM_VERSION_SEGMENTS = 2;
        public const int MAXIMUM_VERSION_SEGMENTS = 4;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum VersionValidation
        {
            None,
            ValidateVersionSegmentsCountButTreatAStandaloneNumberAsAVersion,
            ValidateVersionSegmentsCount
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool IsVersion(string stringToCheck, VersionValidation versionValidation)
        {
            if (stringToCheck == null)
                throw new ArgumentNullException();
            if (!(versionValidation >= VersionValidation.None && versionValidation <= VersionValidation.ValidateVersionSegmentsCount))
                throw new ArgumentOutOfRangeException();
            try
            {
                if (stringToCheck.Equals(""))
                    return false;
                if (versionValidation == VersionValidation.ValidateVersionSegmentsCountButTreatAStandaloneNumberAsAVersion && StringsUtilities.IsOnlyDigits(stringToCheck))
                    return int.TryParse(stringToCheck, out _);
                if (!(char.IsDigit(stringToCheck[0]) && char.IsDigit(stringToCheck[stringToCheck.Length - 1])))
                    return false;
                for (int i = 0; i < stringToCheck.Length; i++)
                {
                    if (!((stringToCheck[i] >= '0' && stringToCheck[i] <= '9') || stringToCheck[i] == '.'))
                        return false;
                    if (i > 0 && (stringToCheck[i] == '.' && stringToCheck[i - 1] == '.'))
                        return false;
                }
                if (versionValidation != VersionValidation.None)
                {
                    int dotsCount = 0;
                    for (int j = 0; j < stringToCheck.Length; j++)
                        if (stringToCheck[j] == '.')
                            dotsCount++;
                    if (!(dotsCount >= MINIMUM_VERSION_SEGMENTS - 1 && dotsCount <= MAXIMUM_VERSION_SEGMENTS - 1))
                        return false;
                }
                string[] splittedVersionString = stringToCheck.Split(new char[] { '.' });
                for (int i = 0; i < splittedVersionString.Length; i++)
                    if (!int.TryParse(splittedVersionString[i], out _))
                        return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsVersionNewer(string newVersionString, string oldVersionString)
        {
            return IsVersionNewer(newVersionString, oldVersionString, false);
        }
        public static bool IsVersionNewer(string newVersionString, string oldVersionString, bool treatAStandaloneNumberAsAVersion)
        {
            if (newVersionString == null || oldVersionString == null)
                throw new ArgumentNullException();
            if (!IsVersion(newVersionString, (!treatAStandaloneNumberAsAVersion ? VersionValidation.ValidateVersionSegmentsCount : VersionValidation.ValidateVersionSegmentsCountButTreatAStandaloneNumberAsAVersion)) || !IsVersion(oldVersionString, (!treatAStandaloneNumberAsAVersion ? VersionValidation.ValidateVersionSegmentsCount : VersionValidation.ValidateVersionSegmentsCountButTreatAStandaloneNumberAsAVersion)))
                throw new InvalidVersionStringException();
            try
            {
                int newVersionDotsCount = 0, oldVersionDotsCount = 0;
                for (int i = 0; i < newVersionString.Length; i++)
                    if (newVersionString[i] == '.')
                        newVersionDotsCount++;
                for (int i = 0; i < oldVersionString.Length; i++)
                    if (oldVersionString[i] == '.')
                        oldVersionDotsCount++;
                StringBuilder tempNewVersionString = new StringBuilder(newVersionString);
                StringBuilder tempOldVersionString = new StringBuilder(oldVersionString);
                if (newVersionDotsCount == 0)
                {
                    newVersionDotsCount++;
                    tempNewVersionString.Append(".0");
                }
                if (oldVersionDotsCount == 0)
                {
                    oldVersionDotsCount++;
                    tempOldVersionString.Append(".0");
                }
                if (newVersionDotsCount > oldVersionDotsCount)
                    for (int i = 0; i < newVersionDotsCount - oldVersionDotsCount; i++)
                        tempOldVersionString.Append(".0");
                else if (oldVersionDotsCount > newVersionDotsCount)
                    for (int i = 0; i < oldVersionDotsCount - newVersionDotsCount; i++)
                        tempNewVersionString.Append(".0");
                return (Version.Parse(tempNewVersionString.ToString()).CompareTo(Version.Parse(tempOldVersionString.ToString())) > 0);
            }
            catch
            {
                return false;
            }
        }
        public static string GetTheFirstFoundVersionFromString(string stringContainingTheVersion)
        {
            return GetTheFirstFoundVersionFromString(stringContainingTheVersion, false, false);
        }
        public static string GetTheFirstFoundVersionFromString(string stringContainingTheVersion, bool treatAStandaloneNumberAsAVersion, bool reversedSearch)
        {
            if (stringContainingTheVersion == null)
                throw new ArgumentNullException();
            try
            {
                string[] words = stringContainingTheVersion.Split(new char[] { ' ' });
                for (int i = 0; i < words.Length; i++)
                {
                    char[] tempWordCharArray = words[(!reversedSearch ? i : words.Length - 1 - i)].ToCharArray();
                    for (int j = 0; j < tempWordCharArray.Length; j++)
                        if (!((tempWordCharArray[j] >= '0' && tempWordCharArray[j] <= '9') || tempWordCharArray[j] == '.'))
                            tempWordCharArray[j] = ' ';
                    string[] versionStrings = (new string(tempWordCharArray)).Split(new char[] { ' ' });
                    foreach (string versionString in versionStrings)
                    {
                        string versionStringWithoutEdgeDots = versionString;
                        if (versionStringWithoutEdgeDots.Length > 0 && versionStringWithoutEdgeDots[0] == '.')
                            versionStringWithoutEdgeDots = versionStringWithoutEdgeDots.Substring(1);
                        if (versionStringWithoutEdgeDots.Length > 0 && versionStringWithoutEdgeDots[versionStringWithoutEdgeDots.Length - 1] == '.')
                            versionStringWithoutEdgeDots = versionStringWithoutEdgeDots.Substring(0, versionStringWithoutEdgeDots.Length - 1);
                        if (IsVersion(versionStringWithoutEdgeDots, (!treatAStandaloneNumberAsAVersion ? VersionValidation.ValidateVersionSegmentsCount : VersionValidation.ValidateVersionSegmentsCountButTreatAStandaloneNumberAsAVersion)))
                            return versionStringWithoutEdgeDots;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public static string GetStringWithoutTheFirstFoundVersion(string originalString, out string versionString)
        {
            return GetStringWithoutTheFirstFoundVersion(originalString, false, false, false, out versionString);
        }
        public static string GetStringWithoutTheFirstFoundVersion(string originalString, bool treatAStandaloneNumberAsAVersion, bool reversedSearch, bool removeVersionWholeWordAndLeftOverSeparators, out string versionString)
        {
            versionString = null;
            if (originalString == null)
                throw new ArgumentNullException();
            try
            {
                versionString = GetTheFirstFoundVersionFromString(originalString, treatAStandaloneNumberAsAVersion, reversedSearch);
                if (versionString == null)
                    return new string(originalString);
                StringBuilder stringWithoutVersion = new StringBuilder();
                string[] words = originalString.Split(new char[] { ' ' });
                bool versionWasFound = false;
                for (int i = 0; i < words.Length; i++)
                {
                    if (versionWasFound || !words[i].Contains(versionString))
                        stringWithoutVersion.Append(words[i]).Append(' ');
                    else
                    {
                        versionWasFound = true;
                        if (!removeVersionWholeWordAndLeftOverSeparators)
                            stringWithoutVersion.Append(words[i].Substring(0, words[i].IndexOf(versionString))).Append(words[i].Substring(words[i].IndexOf(versionString) + versionString.Length)).Append(' ');
                        else
                        {
                            if ((i == words.Length - 1 && i > 0) && (words[i - 1].Length == 1 && !char.IsLetterOrDigit(words[i - 1][0])))
                                stringWithoutVersion[stringWithoutVersion.Length - 2] = ' ';
                        }
                    }
                }
                return stringWithoutVersion.ToString().Trim();
            }
            catch
            {
                return null;
            }
        }
        public static string GetTheLatestVersionFromString(string stringContainingTheVersion)
        {
            return GetTheLatestVersionFromString(stringContainingTheVersion, false);
        }
        public static string GetTheLatestVersionFromString(string stringContainingTheVersion, bool treatAStandaloneNumberAsAVersion)
        {
            if (stringContainingTheVersion == null)
                throw new ArgumentNullException();
            try
            {
                List<string> versionsFromString = new List<string>();
                string tempStringContainingTheVersion = stringContainingTheVersion;
                while (true)
                {
                    string currentFoundVersionString;
                    tempStringContainingTheVersion = GetStringWithoutTheFirstFoundVersion(tempStringContainingTheVersion, treatAStandaloneNumberAsAVersion, false, false, out currentFoundVersionString);
                    if (currentFoundVersionString == null)
                        break;
                    versionsFromString.Add(currentFoundVersionString);
                }
                if (versionsFromString.Count == 0)
                    return null;
                string latestVersion = versionsFromString[0];
                for (int i = 1; i < versionsFromString.Count; i++)
                {
                    if (IsVersionNewer(versionsFromString[i], latestVersion, treatAStandaloneNumberAsAVersion))
                        latestVersion = versionsFromString[i];
                }
                return latestVersion;
            }
            catch
            {
                return null;
            }
        }
        public static string TrimVersion(string versionStringToTrim, int minimumVersionSegments, int maximumVersionSegments)
        {
            return TrimVersion(versionStringToTrim, minimumVersionSegments, maximumVersionSegments, false);
        }
        public static string TrimVersion(string versionStringToTrim, int minimumVersionSegments, int maximumVersionSegments, bool removeTrailingZeroSegmentsOfVersion)
        {
            if (versionStringToTrim == null)
                throw new ArgumentNullException();
            if (!(versionStringToTrim.Equals("") || IsVersion(versionStringToTrim, VersionValidation.None)))
                throw new InvalidVersionStringException();
            if ((minimumVersionSegments < MINIMUM_VERSION_SEGMENTS || minimumVersionSegments > MAXIMUM_VERSION_SEGMENTS) || (maximumVersionSegments < MINIMUM_VERSION_SEGMENTS || maximumVersionSegments > MAXIMUM_VERSION_SEGMENTS))
                throw new ArgumentOutOfRangeException();
            if (minimumVersionSegments > maximumVersionSegments)
                throw new ArgumentException();
            try
            {
                StringBuilder trimmedVersionString = new StringBuilder();
                string[] splittedVersionStringToTrim = versionStringToTrim.Split(new char[] { '.' });
                if (splittedVersionStringToTrim.Length <= minimumVersionSegments)
                {
                    if (versionStringToTrim.Equals(""))
                        trimmedVersionString.Append('0');
                    else
                    {
                        for (int i = 0; i < splittedVersionStringToTrim.Length; i++)
                        {
                            if (i != 0)
                                trimmedVersionString.Append('.');
                            trimmedVersionString.Append(Convert.ToInt32(splittedVersionStringToTrim[i]));
                        }
                    }
                    for (int i = 0; i < minimumVersionSegments - splittedVersionStringToTrim.Length; i++)
                        trimmedVersionString.Append(".0");
                }
                else
                {
                    int trimmedVersionNumberSegmentsCount = Math.Min(splittedVersionStringToTrim.Length, maximumVersionSegments);
                    if (removeTrailingZeroSegmentsOfVersion)
                    {
                        int versionTrailingZeroSegmentsCount = 0;
                        for (int i = trimmedVersionNumberSegmentsCount - 1; i >= 0; i--)
                        {
                            if (Convert.ToInt32(splittedVersionStringToTrim[i]) == 0 && i >= minimumVersionSegments)
                                versionTrailingZeroSegmentsCount++;
                            else
                                break;
                        }
                        trimmedVersionNumberSegmentsCount -= versionTrailingZeroSegmentsCount;
                    }
                    for (int i = 0; i < trimmedVersionNumberSegmentsCount; i++)
                    {
                        if (i != 0)
                            trimmedVersionString.Append('.');
                        trimmedVersionString.Append(Convert.ToInt32(splittedVersionStringToTrim[i]));
                    }
                }
                return trimmedVersionString.ToString();
            }
            catch
            {
                return null;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
