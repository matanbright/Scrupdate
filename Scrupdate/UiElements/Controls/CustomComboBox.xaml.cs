﻿// Copyright © 2021 Matan Brightbert
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




using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Scrupdate.Classes.Utilities;


namespace Scrupdate.UiElements.Controls
{
    /// <summary>
    /// Interaction logic for CustomComboBox.xaml
    /// </summary>
    public partial class CustomComboBox : ComboBox, INotifyPropertyChanged
    {
        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register(
            nameof(HintText),
            typeof(string),
            typeof(CustomComboBox),
            new PropertyMetadata("")
        );
        public static readonly DependencyProperty PopupBackgroundProperty = DependencyProperty.Register(
            nameof(PopupBackground),
            typeof(Brush),
            typeof(CustomComboBox),
            new PropertyMetadata(
                (SolidColorBrush)Application.Current.FindResource(
                    App.RESOURCE_KEY__WHITE_SOLID_COLOR_BRUSH
                )
            )
        );
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string HintText
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (string)GetValue(HintTextProperty)
                );
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(HintTextProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(HintText))
                            );
                        }
                );
            }
        }
        public Brush PopupBackground
        {
            get
            {
                return ThreadsUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(PopupBackgroundProperty)
                );
            }
            set
            {
                ThreadsUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(PopupBackgroundProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(PopupBackground))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        


        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CustomComboBox()
        {
            InitializeComponent();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
