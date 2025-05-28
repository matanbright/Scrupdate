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
using System.Windows.Media;
using System.Windows.Media.Animation;
using Scrupdate.Classes.Utilities;


namespace Scrupdate.UiElements.Controls
{
    /// <summary>
    /// Interaction logic for CustomProgressBar.xaml
    /// </summary>
    public partial class CustomProgressBar : ProgressBar, INotifyPropertyChanged
    {
        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty ProgressDurationProperty = DependencyProperty.Register(
            nameof(ProgressDuration),
            typeof(int),
            typeof(CustomProgressBar),
            new PropertyMetadata(0)
        );
        public static readonly DependencyProperty GlowColorProperty = DependencyProperty.Register(
            nameof(GlowColor),
            typeof(Color),
            typeof(CustomProgressBar),
            new PropertyMetadata(Colors.White)
        );
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int ProgressDuration
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (int)GetValue(ProgressDurationProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(ProgressDurationProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(ProgressDuration))
                            );
                        }
                );
            }
        }
        public Color GlowColor
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Color)GetValue(GlowColorProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(GlowColorProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(GlowColor))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CustomProgressBar()
        {
            InitializeComponent();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void ChangeValueSmoothly(double value)
        {
            BeginAnimation(
                ValueProperty,
                new DoubleAnimation(
                    value,
                    new Duration(
                        new System.TimeSpan(0, 0, 0, 0, IsIndeterminate ? 0 : ProgressDuration)
                    )
                )
            );
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
