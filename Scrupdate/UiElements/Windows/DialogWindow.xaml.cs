using System;
using System.ComponentModel;
using System.Media;
using System.Windows;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;


namespace Scrupdate.UiElements.Windows
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window, INotifyPropertyChanged
    {
        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum DialogType
        {
            Unknown,
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
