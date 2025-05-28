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




using Scrupdate.Classes.Utilities;


namespace Scrupdate.Classes.Objects
{
    public class WebpageElementLocatingInstructionListViewItem
    {
        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int WebpageElementLocatingInstructionIndex { get; set; }
        public WebpageElementLocatingInstruction UnderlyingWebpageElementLocatingInstruction { get; set; }
        public string WebpageElementLocatingMethodToDisplay { get; set; }
        public string WebpageElementLocatingIntervalToDisplay { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public WebpageElementLocatingInstructionListViewItem(int webpageElementLocatingInstructionIndex,
                                                             WebpageElementLocatingInstruction underlyingWebpageElementLocatingInstruction) :
            this(
                webpageElementLocatingInstructionIndex,
                underlyingWebpageElementLocatingInstruction,
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    underlyingWebpageElementLocatingInstruction.LocatingMethod
                ).Replace("Id", "ID").Replace("Html", "HTML").Replace("X Path", "XPath"),
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    underlyingWebpageElementLocatingInstruction.LocatingInterval
                ).Replace(" Ms", "ms")
            )
        { }
        public WebpageElementLocatingInstructionListViewItem(int webpageElementLocatingInstructionIndex,
                                                             WebpageElementLocatingInstruction underlyingWebpageElementLocatingInstruction,
                                                             string webpageElementLocatingMethodToDisplay,
                                                             string webpageElementLocatingIntervalToDisplay)
        {
            WebpageElementLocatingInstructionIndex = webpageElementLocatingInstructionIndex;
            UnderlyingWebpageElementLocatingInstruction = underlyingWebpageElementLocatingInstruction;
            WebpageElementLocatingMethodToDisplay = webpageElementLocatingMethodToDisplay;
            WebpageElementLocatingIntervalToDisplay = webpageElementLocatingIntervalToDisplay;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
