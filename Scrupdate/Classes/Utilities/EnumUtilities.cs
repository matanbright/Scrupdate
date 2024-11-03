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
using System.Text;


namespace Scrupdate.Classes.Utilities
{
    public static class EnumUtilities
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class UnableToGetEnumItemFromHumanReadableStringException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Unable to Get Enum Item from Human Readable String!";
            public UnableToGetEnumItemFromHumanReadableStringException() : base(EXCEPTION_MESSAGE) { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GetHumanReadableStringFromEnumItem(Enum enumItem)
        {
            string enumItemString = enumItem.ToString();
            return StringUtilities.GetSpaceSeparatedWordsStringFromPascalCasedWordsString(
                (enumItemString[0] == '_' ?
                    enumItemString.Substring(1) :
                    enumItemString)
            );
        }
        public static TEnum GetEnumItemFromHumanReadableString<TEnum>(string enumItemHumanReadableString)
        {
            string normalizedEnumItemHumanReadableString = enumItemHumanReadableString.Replace(" ", "");
            StringBuilder enumItemString = new StringBuilder(
                1 + normalizedEnumItemHumanReadableString.Length
            );
            if (char.IsDigit(normalizedEnumItemHumanReadableString[0]))
                enumItemString.Append('_');
            enumItemString.Append(normalizedEnumItemHumanReadableString);
            try
            {
                return (TEnum)Enum.Parse(
                    typeof(TEnum),
                    enumItemString.ToString()
                );
            }
            catch
            {
                throw new UnableToGetEnumItemFromHumanReadableStringException();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
