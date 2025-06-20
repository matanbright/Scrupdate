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




using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Scrupdate.Classes.Utilities;


namespace Scrupdate.UiElements.Controls
{
    public partial class CustomGridViewColumnHeader : GridViewColumnHeader, INotifyPropertyChanged
    {
        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum SortingOrder
        {
            None,
            Ascending,
            Descending
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CustomGridViewColumnHeader),
            new PropertyMetadata(null)
        );
        public static readonly DependencyProperty IsSortableProperty = DependencyProperty.Register(
            nameof(IsSortable),
            typeof(bool),
            typeof(CustomGridViewColumnHeader),
            new PropertyMetadata(false)
        );
        public static readonly DependencyProperty ListViewItemsSortingOrderProperty = DependencyProperty.Register(
            nameof(ListViewItemsSortingOrder),
            typeof(SortingOrder),
            typeof(CustomGridViewColumnHeader),
            new PropertyMetadata(SortingOrder.None)
        );
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string Text
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (string)GetValue(TextProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(TextProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(Text))
                            );
                        }
                );
            }
        }
        public bool IsSortable
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (bool)GetValue(IsSortableProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(IsSortableProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(IsSortable))
                            );
                        }
                );
            }
        }
        public SortingOrder ListViewItemsSortingOrder
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (SortingOrder)GetValue(ListViewItemsSortingOrderProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(ListViewItemsSortingOrderProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(ListViewItemsSortingOrder))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CustomGridViewColumnHeader()
        {
            InitializeComponent();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
