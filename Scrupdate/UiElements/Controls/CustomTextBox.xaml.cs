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




using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scrupdate.Classes.Utilities;


namespace Scrupdate.UiElements.Controls
{
    public partial class CustomTextBox : TextBox, INotifyPropertyChanged
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class DisabledRoutedEventArgs : RoutedEventArgs
        {
            public DisabledRoutedEventArgs(RoutedEvent disabledRoutedEvent) : base(disabledRoutedEvent) { }
        }
        public class EnabledRoutedEventArgs : RoutedEventArgs
        {
            public EnabledRoutedEventArgs(RoutedEvent enabledRoutedEvent) : base(enabledRoutedEvent) { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Delegates ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public delegate void DisabledRoutedEventHandler();
        public delegate void EnabledRoutedEventHandler();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register(
            nameof(HintText),
            typeof(string),
            typeof(CustomTextBox),
            new PropertyMetadata("")
        );
        public static readonly DependencyProperty IsShowingClearButtonProperty = DependencyProperty.Register(
            nameof(IsShowingClearButton),
            typeof(bool),
            typeof(CustomTextBox),
            new PropertyMetadata(false)
        );
        public static readonly RoutedEvent EnabledEvent = EventManager.RegisterRoutedEvent(
            nameof(Enabled),
            RoutingStrategy.Bubble,
            typeof(EnabledRoutedEventHandler),
            typeof(CustomTextBox)
        );
        public static readonly RoutedEvent DisabledEvent = EventManager.RegisterRoutedEvent(
            nameof(Disabled),
            RoutingStrategy.Bubble,
            typeof(DisabledRoutedEventHandler),
            typeof(CustomTextBox)
        );
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string HintText
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (string)GetValue(HintTextProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
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
        public bool IsShowingClearButton
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (bool)GetValue(IsShowingClearButtonProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(IsShowingClearButtonProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(IsShowingClearButton))
                            );
                        }
                );
            }
        }
        public event EnabledRoutedEventHandler Enabled
        {
            add
            {
                AddHandler(EnabledEvent, value);
            }
            remove
            {
                RemoveHandler(EnabledEvent, value);
            }
        }
        public event DisabledRoutedEventHandler Disabled
        {
            add
            {
                AddHandler(DisabledEvent, value);
            }
            remove
            {
                RemoveHandler(DisabledEvent, value);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CustomTextBox()
        {
            InitializeComponent();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnLoadedEvent(object sender, RoutedEventArgs e)
        {
            RaiseEvent(
                (IsEnabled ?
                    new EnabledRoutedEventArgs(EnabledEvent) :
                    new DisabledRoutedEventArgs(DisabledEvent))
            );
        }
        private void OnIsEnabledChangedEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            RaiseEvent(
                (IsEnabled ?
                    new EnabledRoutedEventArgs(EnabledEvent) :
                    new DisabledRoutedEventArgs(DisabledEvent))
            );
        }
        private void OnBorderMouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Text = "";
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
