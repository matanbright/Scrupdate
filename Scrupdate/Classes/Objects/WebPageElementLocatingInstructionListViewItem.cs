﻿// Copyright © 2021-2024 Matan Brightbert
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
    public class WebPageElementLocatingInstructionListViewItem
    {
        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int WebPageElementLocatingInstructionIndex { get; set; }
        public WebPageElementLocatingInstruction UnderlyingWebPageElementLocatingInstruction { get; set; }
        public string WebPageElementLocatingMethodToDisplay { get; set; }
        public string WebPageElementLocatingIntervalToDisplay { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public WebPageElementLocatingInstructionListViewItem(int webPageElementLocatingInstructionIndex,
                                                             WebPageElementLocatingInstruction underlyingWebPageElementLocatingInstruction) :
            this(
                webPageElementLocatingInstructionIndex,
                underlyingWebPageElementLocatingInstruction,
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    underlyingWebPageElementLocatingInstruction.LocatingMethod
                ).Replace("Id", "ID").Replace("Html", "HTML").Replace("X Path", "XPath"),
                EnumUtilities.GetHumanReadableStringFromEnumItem(
                    underlyingWebPageElementLocatingInstruction.LocatingInterval
                ).Replace(" Ms", "ms")
            )
        { }
        public WebPageElementLocatingInstructionListViewItem(int webPageElementLocatingInstructionIndex,
                                                             WebPageElementLocatingInstruction underlyingWebPageElementLocatingInstruction,
                                                             string webPageElementLocatingMethodToDisplay,
                                                             string webPageElementLocatingIntervalToDisplay)
        {
            WebPageElementLocatingInstructionIndex = webPageElementLocatingInstructionIndex;
            UnderlyingWebPageElementLocatingInstruction = underlyingWebPageElementLocatingInstruction;
            WebPageElementLocatingMethodToDisplay = webPageElementLocatingMethodToDisplay;
            WebPageElementLocatingIntervalToDisplay = webPageElementLocatingIntervalToDisplay;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
