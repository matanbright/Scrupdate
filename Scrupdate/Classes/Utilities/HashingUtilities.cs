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
using System.Security.Cryptography;


namespace Scrupdate.Classes.Utilities
{
    public static class HashingUtilities
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string MD5_HASH_FILE_EXTENSION = ".md5";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static readonly MD5 md5HashGenerator = MD5.Create();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GetMD5Hash(string stringToHash)
        {
            if (stringToHash == null)
                throw new ArgumentNullException(nameof(stringToHash));
            try
            {
                return GetMD5Hash(Encoding.Default.GetBytes(stringToHash));
            }
            catch
            {
                return null;
            }
        }
        public static string GetMD5Hash(byte[] bytesArrayToHash)
        {
            if (bytesArrayToHash == null)
                throw new ArgumentNullException(nameof(bytesArrayToHash));
            try
            {
                byte[] hashedStringBytes = md5HashGenerator.ComputeHash(bytesArrayToHash);
                StringBuilder hashedString = new StringBuilder(hashedStringBytes.Length * 2);
                for (int i = 0; i < hashedStringBytes.Length; i++)
                    hashedString.Append(hashedStringBytes[i].ToString("x2"));
                return hashedString.ToString();
            }
            catch
            {
                return null;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
