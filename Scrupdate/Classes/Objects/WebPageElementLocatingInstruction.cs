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
        public enum WebPageElementLocatingInstructionMethod
        {
            Unspecified,
            ByHtmlElementId,
            ByXPath,
            ByInnerText
        }
        public enum WebPageElementLocatingInstructionDuration
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
        public WebPageElementLocatingInstructionMethod Method { get; set; }
        public string MethodArgument { get; set; }
        public bool MatchExactText { get; set; }
        public WebPageElementLocatingInstructionDuration Duration { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public WebPageElementLocatingInstruction()
        {
            Method = WebPageElementLocatingInstructionMethod.Unspecified;
            MethodArgument = "";
            MatchExactText = false;
            Duration = WebPageElementLocatingInstructionDuration._Unspecified;
        }
        public WebPageElementLocatingInstruction(WebPageElementLocatingInstructionMethod method, string methodArgument, bool matchExactText, WebPageElementLocatingInstructionDuration duration)
        {
            Method = method;
            MethodArgument = methodArgument;
            MatchExactText = matchExactText;
            Duration = duration;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            WebPageElementLocatingInstruction otherWebPageElementLocatingInstruction = (WebPageElementLocatingInstruction)obj;
            return (Method.Equals(otherWebPageElementLocatingInstruction.Method) &&
                MethodArgument.Equals(otherWebPageElementLocatingInstruction.MethodArgument) &&
                MatchExactText == otherWebPageElementLocatingInstruction.MatchExactText &&
                Duration == otherWebPageElementLocatingInstruction.Duration);
        }
        public override int GetHashCode()
        {
            StringBuilder objectHashingString = new StringBuilder();
            objectHashingString.Append(Method.GetHashCode());
            objectHashingString.Append(MethodArgument.GetHashCode());
            objectHashingString.Append(MatchExactText.GetHashCode());
            objectHashingString.Append(Duration.GetHashCode());
            return objectHashingString.ToString().GetHashCode();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
