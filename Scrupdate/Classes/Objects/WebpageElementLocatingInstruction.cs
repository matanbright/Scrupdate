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




using System.Text;


namespace Scrupdate.Classes.Objects
{
    public class WebpageElementLocatingInstruction
    {
        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum _LocatingMethod
        {
            Unspecified,
            ByHtmlElementId,
            ByXPath,
            ByInnerText
        }
        public enum _LocatingInterval
        {
            Unspecified,
            _1Ms,
            _10Ms,
            _100Ms,
            _250Ms,
            _500Ms,
            _1000Ms
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public _LocatingMethod LocatingMethod { get; set; }
        public string MethodArgument { get; set; }
        public bool MatchExactText { get; set; }
        public _LocatingInterval LocatingInterval { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public WebpageElementLocatingInstruction() :
            this(
                _LocatingMethod.Unspecified,
                "",
                false,
                _LocatingInterval.Unspecified
            )
        { }
        public WebpageElementLocatingInstruction(_LocatingMethod locatingMethod,
                                                 string methodArgument,
                                                 bool matchExactText,
                                                 _LocatingInterval locatingInterval)
        {
            LocatingMethod = locatingMethod;
            MethodArgument = methodArgument;
            MatchExactText = matchExactText;
            LocatingInterval = locatingInterval;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            WebpageElementLocatingInstruction otherWebpageElementLocatingInstruction =
                (WebpageElementLocatingInstruction)obj;
            return (LocatingMethod.Equals(otherWebpageElementLocatingInstruction.LocatingMethod) &&
                    MethodArgument.Equals(otherWebpageElementLocatingInstruction.MethodArgument) &&
                    MatchExactText == otherWebpageElementLocatingInstruction.MatchExactText &&
                    LocatingInterval == otherWebpageElementLocatingInstruction.LocatingInterval);
        }
        public override int GetHashCode()
        {
            StringBuilder objectHashString = new StringBuilder(10 * 4);
            objectHashString
                .Append(LocatingMethod.GetHashCode())
                .Append(MethodArgument.GetHashCode())
                .Append(MatchExactText.GetHashCode())
                .Append(LocatingInterval.GetHashCode());
            return objectHashString.ToString().GetHashCode();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
