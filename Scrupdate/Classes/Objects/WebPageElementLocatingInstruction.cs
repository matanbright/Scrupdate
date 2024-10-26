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




using System.Text;


namespace Scrupdate.Classes.Objects
{
    public class WebPageElementLocatingInstruction
    {
        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum _LocatingMethod
        {
            Unspecified,
            ByHtmlElementId,
            ByXPath,
            ByInnerText
        }
        public enum _LocatingDuration
        {
            _Unspecified,
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
        public _LocatingDuration LocatingDuration { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public WebPageElementLocatingInstruction() :
            this(
                _LocatingMethod.Unspecified,
                "",
                false,
                _LocatingDuration._Unspecified
            )
        { }
        public WebPageElementLocatingInstruction(_LocatingMethod locatingMethod,
                                                 string methodArgument,
                                                 bool matchExactText,
                                                 _LocatingDuration locatingDuration)
        {
            LocatingMethod = locatingMethod;
            MethodArgument = methodArgument;
            MatchExactText = matchExactText;
            LocatingDuration = locatingDuration;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            WebPageElementLocatingInstruction otherWebPageElementLocatingInstruction =
                (WebPageElementLocatingInstruction)obj;
            return (LocatingMethod.Equals(otherWebPageElementLocatingInstruction.LocatingMethod) &&
                    MethodArgument.Equals(otherWebPageElementLocatingInstruction.MethodArgument) &&
                    MatchExactText == otherWebPageElementLocatingInstruction.MatchExactText &&
                    LocatingDuration == otherWebPageElementLocatingInstruction.LocatingDuration);
        }
        public override int GetHashCode()
        {
            StringBuilder objectHashingString = new StringBuilder();
            objectHashingString
                .Append(LocatingMethod.GetHashCode())
                .Append(MethodArgument.GetHashCode())
                .Append(MatchExactText.GetHashCode())
                .Append(LocatingDuration.GetHashCode());
            return objectHashingString.ToString().GetHashCode();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
