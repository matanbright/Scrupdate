// Copyright © 2021-2025 Matan Brightbert
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




using Scrupdate.UiElements.Controls;


namespace Scrupdate.Classes.Objects
{
    public class ProgramListViewColumnState
    {
        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string Tag { get; set; }
        public double Width { get; set; }
        public CustomGridViewColumnHeader.SortingOrder SortingOrder { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ProgramListViewColumnState() : this("") { }
        public ProgramListViewColumnState(string tag) :
            this(
                tag,
                0.0,
                CustomGridViewColumnHeader.SortingOrder.None
            )
        { }
        public ProgramListViewColumnState(string tag,
                                          double width,
                                          CustomGridViewColumnHeader.SortingOrder sortingOrder)
        {
            Tag = tag;
            Width = width;
            SortingOrder = sortingOrder;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
