﻿// Copyright © 2021-2025 Matan Brightbert
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


namespace Scrupdate.Classes.Utilities
{
    public static class StringUtilities
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class StringDoesNotContainOnlyLettersAndDigitsException : Exception
        {
            private const string EXCEPTION_MESSAGE = "String does not contain only letters and digits!";
            public StringDoesNotContainOnlyLettersAndDigitsException() : base(EXCEPTION_MESSAGE) { }
        }
        public class StringIsNotPascalCasedException : Exception
        {
            private const string EXCEPTION_MESSAGE = "String is not Pascal cased!";
            public StringIsNotPascalCasedException() : base(EXCEPTION_MESSAGE) { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool IsOnlyDigits(string stringToCheck)
        {
            if (stringToCheck == null)
                throw new ArgumentNullException(nameof(stringToCheck));
            try
            {
                if (stringToCheck.Length == 0)
                    return false;
                bool stringContainsOnlyDigits = true;
                for (int i = 0; i < stringToCheck.Length; i++)
                {
                    if (!char.IsDigit(stringToCheck[i]))
                    {
                        stringContainsOnlyDigits = false;
                        break;
                    }
                }
                return stringContainsOnlyDigits;
            }
            catch
            {
                return false;
            }
        }
        public static string GetSpaceSeparatedWordsStringFromPascalCasedWordsString(string pascalCasedWordsString)
        {
            if (pascalCasedWordsString == null)
                throw new ArgumentNullException(nameof(pascalCasedWordsString));
            if (pascalCasedWordsString.Length > 0 &&
                !Array.TrueForAll(
                    pascalCasedWordsString.ToCharArray(),
                    c => char.IsLetterOrDigit(c)
                ))
            {
                throw new StringDoesNotContainOnlyLettersAndDigitsException();
            }
            if (pascalCasedWordsString.Length > 0 &&
                !(char.IsUpper(pascalCasedWordsString[0]) || char.IsDigit(pascalCasedWordsString[0])))
            {
                throw new StringIsNotPascalCasedException();
            }
            for (int i = 1; i < pascalCasedWordsString.Length; i++)
                if (char.IsLower(pascalCasedWordsString[i]) && char.IsDigit(pascalCasedWordsString[i - 1]))
                    throw new StringIsNotPascalCasedException();
            try
            {
                StringBuilder spaceSeparatedWordsString = new StringBuilder(
                    Math.Max(0, pascalCasedWordsString.Length * 2 - 1)
                );
                if (pascalCasedWordsString.Length > 0)
                {
                    spaceSeparatedWordsString.Append(pascalCasedWordsString[0]);
                    for (int i = 1; i < pascalCasedWordsString.Length; i++)
                    {
                        if (char.IsUpper(pascalCasedWordsString[i]) ||
                            ((char.IsDigit(pascalCasedWordsString[i]) && !char.IsDigit(pascalCasedWordsString[i - 1])) ||
                             (!char.IsDigit(pascalCasedWordsString[i]) && char.IsDigit(pascalCasedWordsString[i - 1]))))
                        {
                            spaceSeparatedWordsString.Append(' ');
                        }
                        spaceSeparatedWordsString.Append(pascalCasedWordsString[i]);
                    }
                }
                return spaceSeparatedWordsString.ToString();
            }
            catch
            {
                return null;
            }
        }
        public static string GetTextAsASentence(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            StringBuilder textAsASentence = new StringBuilder(text.ToLower());
            textAsASentence[0] = Convert.ToChar(
                Convert.ToString(textAsASentence[0]).ToUpper()
            );
            return textAsASentence.ToString();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
