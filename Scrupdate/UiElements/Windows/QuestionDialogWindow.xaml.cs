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
using System.Media;
using System.ComponentModel;
using System.Windows;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;


namespace Scrupdate.UiElements.Windows
{
    /// <summary>
    /// Interaction logic for QuestionDialogWindow.xaml
    /// </summary>
    public partial class QuestionDialogWindow : Window
    {
        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSizeOfWindow { get; private set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public QuestionDialogWindow(string dialogTitle, string dialogMessage)
        {
            InitializeComponent();
            BaseSizeOfWindow = new Size(Width, Height);
            WindowUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSizeOfWindow,
                App.WindowsRenderingScale
            );
            Title = dialogTitle;
            label_dialogMessage.Content = dialogMessage;
            CalculateWindowDynamicSizeAndResizeWindow();
            button_yes.Focus();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnWindowLoadedEvent(object sender, RoutedEventArgs e)
        {
            SystemSounds.Question.Play();
        }
        private void OnWindowClosingEvent(object sender, CancelEventArgs e)
        {
            Owner?.Activate();
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_yes)
            {
                DialogResult = true;
                Close();
            }
            else if (senderButton == button_no)
                Close();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void CalculateWindowDynamicSizeAndResizeWindow()
        {
            label_dialogMessage.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size calculatedWindowSize = new Size(
                Math.Floor(
                    (Math.Round(label_dialogMessage.DesiredSize.Width) + 122.0D) * App.WindowsRenderingScale
                ),
                Math.Floor(
                    (Math.Round(label_dialogMessage.DesiredSize.Height) + 142.0D) * App.WindowsRenderingScale
                )
            );
            calculatedWindowSize.Width +=
                SystemParameters.WindowNonClientFrameThickness.Left +
                SystemParameters.WindowNonClientFrameThickness.Right +
                SystemParameters.WindowResizeBorderThickness.Left +
                SystemParameters.WindowResizeBorderThickness.Right;
            calculatedWindowSize.Height +=
                SystemParameters.WindowNonClientFrameThickness.Top +
                SystemParameters.WindowNonClientFrameThickness.Bottom +
                SystemParameters.WindowResizeBorderThickness.Top +
                SystemParameters.WindowResizeBorderThickness.Bottom;
            MinWidth = calculatedWindowSize.Width;
            Width = calculatedWindowSize.Width;
            MaxWidth = calculatedWindowSize.Width;
            MinHeight = calculatedWindowSize.Height;
            Height = calculatedWindowSize.Height;
            MaxHeight = calculatedWindowSize.Height;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
