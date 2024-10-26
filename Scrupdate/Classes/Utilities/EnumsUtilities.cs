﻿using System;
using System.Text;


namespace Scrupdate.Classes.Utilities
{
    public static class EnumsUtilities
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
            return StringsUtilities.GetSpaceSeparatedWordsStringFromPascalCasedWordsString(
                (enumItemString[0] == '_' ?
                    enumItemString.Substring(1) :
                    enumItemString)
            );
        }
        public static TEnum GetEnumItemFromHumanReadableString<TEnum>(string enumItemHumanReadableString)
        {
            string normalizedEnumItemHumanReadableString = enumItemHumanReadableString.Replace(" ", "");
            StringBuilder enumItemString = new StringBuilder();
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
