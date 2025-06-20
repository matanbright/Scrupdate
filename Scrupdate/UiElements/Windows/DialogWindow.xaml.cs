﻿// Copyright © 2021-2025 Matan Brightbert
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
using System.ComponentModel;
using System.Media;
using System.Windows;
using System.Windows.Input;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;


namespace Scrupdate.UiElements.Windows
{
    public partial class DialogWindow : Window, INotifyPropertyChanged
    {
        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum DialogType
        {
            Unknown,
            Information,
            Warning,
            Error,
            Question
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty CurrentDialogTypeProperty = DependencyProperty.Register(
            nameof(CurrentDialogType),
            typeof(DialogType),
            typeof(MainWindow),
            new PropertyMetadata(DialogType.Unknown)
        );
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSize { get; private set; }
        public DialogType CurrentDialogType
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (DialogType)GetValue(CurrentDialogTypeProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(CurrentDialogTypeProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(CurrentDialogType))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public DialogWindow(DialogType dialogType, string dialogTitle, string dialogMessage)
        {
            InitializeComponent();
            BaseSize = new Size(Width, Height);
            WindowUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSize,
                App.WindowsRenderingScale
            );
            Title = dialogTitle;
            label_dialogMessage.Content = dialogMessage;
            CalculateWindowDynamicSizeAndResizeWindow();
            CurrentDialogType = dialogType;
            switch (CurrentDialogType)
            {
                case DialogType.Information:
                case DialogType.Warning:
                case DialogType.Error:
                    button_ok.Focus();
                    break;
                case DialogType.Question:
                    button_yes.Focus();
                    break;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnLoadedEvent(object sender, RoutedEventArgs e)
        {
            switch (CurrentDialogType)
            {
                case DialogType.Information:
                    SystemSounds.Asterisk.Play();
                    break;
                case DialogType.Warning:
                    SystemSounds.Exclamation.Play();
                    break;
                case DialogType.Error:
                    SystemSounds.Hand.Play();
                    break;
                case DialogType.Question:
                    SystemSounds.Question.Play();
                    break;
            }
        }
        private void OnClosingEvent(object sender, CancelEventArgs e)
        {
            Owner?.Activate();
        }
        private void OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_ok ||
                senderButton == button_no)
            {
                Close();
            }
            else if (senderButton == button_yes)
            {
                DialogResult = true;
                Close();
            }
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
