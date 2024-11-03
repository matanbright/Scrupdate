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
using Scrupdate.Classes.Utilities;


namespace Scrupdate.UiElements.Controls
{
    /// <summary>
    /// Interaction logic for CustomButton.xaml
    /// </summary>
    public partial class CustomButton : Button, INotifyPropertyChanged
    {
        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(CustomButton),
            new PropertyMetadata(new CornerRadius())
        );
        public static readonly DependencyProperty HoveredBorderBrushProperty = DependencyProperty.Register(
            nameof(HoveredBorderBrush),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty PressedBorderBrushProperty = DependencyProperty.Register(
            nameof(PressedBorderBrush),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty DisabledBorderBrushProperty = DependencyProperty.Register(
            nameof(DisabledBorderBrush),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty HoveredForegroundProperty = DependencyProperty.Register(
            nameof(HoveredForeground),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty PressedForegroundProperty = DependencyProperty.Register(
            nameof(PressedForeground),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty DisabledForegroundProperty = DependencyProperty.Register(
            nameof(DisabledForeground),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty HoveredBackgroundProperty = DependencyProperty.Register(
            nameof(HoveredBackground),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            nameof(PressedBackground),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty DisabledBackgroundProperty = DependencyProperty.Register(
            nameof(DisabledBackground),
            typeof(Brush),
            typeof(CustomButton),
            new PropertyMetadata(null)
        );
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CornerRadius CornerRadius
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (CornerRadius)GetValue(CornerRadiusProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(CornerRadiusProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(CornerRadius))
                            );
                        }
                );
            }
        }
        public Brush HoveredBorderBrush
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(HoveredBorderBrushProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(HoveredBorderBrushProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(HoveredBorderBrush))
                            );
                        }
                );
            }
        }
        public Brush PressedBorderBrush
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(PressedBorderBrushProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(PressedBorderBrushProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(PressedBorderBrush))
                            );
                        }
                );
            }
        }
        public Brush DisabledBorderBrush
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(DisabledBorderBrushProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(DisabledBorderBrushProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(DisabledBorderBrush))
                            );
                        }
                );
            }
        }
        public Brush HoveredForeground
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(HoveredForegroundProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(HoveredForegroundProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(HoveredForeground))
                            );
                        }
                );
            }
        }
        public Brush PressedForeground
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(PressedForegroundProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(PressedForegroundProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(PressedForeground))
                            );
                        }
                );
            }
        }
        public Brush DisabledForeground
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(DisabledForegroundProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(DisabledForegroundProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(DisabledForeground))
                            );
                        }
                );
            }
        }
        public Brush HoveredBackground
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(HoveredBackgroundProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(HoveredBackgroundProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(HoveredBackground))
                            );
                        }
                );
            }
        }
        public Brush PressedBackground
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(PressedBackgroundProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(PressedBackgroundProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(PressedBackground))
                            );
                        }
                );
            }
        }
        public Brush DisabledBackground
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Brush)GetValue(DisabledBackgroundProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(DisabledBackgroundProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(DisabledBackground))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CustomButton()
        {
            InitializeComponent();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
