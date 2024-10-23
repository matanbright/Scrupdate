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




using System.Windows;
using System.Windows.Media;


namespace Scrupdate.Classes.Objects
{
    public class ProgramListViewItem
    {
        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Program UnderlyingProgram { get; set; }
        public string ProgramInstalledVersionToDisplay { get; set; }
        public string ProgramLatestVersionToDisplay { get; set; }
        public Brush Foreground { get; set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ProgramListViewItem(Program underlyingProgram) :
            this(
                underlyingProgram,
                underlyingProgram.InstalledVersion,
                underlyingProgram.LatestVersion
            )
        { }
        public ProgramListViewItem(Program underlyingProgram,
                                   string programInstalledVersionToDisplay,
                                   string programLatestVersionToDisplay) :
            this(
                underlyingProgram,
                programInstalledVersionToDisplay,
                programLatestVersionToDisplay,
                (SolidColorBrush)Application.Current.FindResource(App.RESOURCE_KEY__BLACK_SOLID_COLOR_BRUSH)
            )
        { }
        public ProgramListViewItem(Program underlyingProgram,
                                   string programInstalledVersionToDisplay,
                                   string programLatestVersionToDisplay,
                                   Brush foreground)
        {
            UnderlyingProgram = underlyingProgram;
            ProgramInstalledVersionToDisplay = programInstalledVersionToDisplay;
            ProgramLatestVersionToDisplay = programLatestVersionToDisplay;
            Foreground = foreground;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
