using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Scrupdate.Classes.Objects;
using Scrupdate.Classes.Utilities;
using Scrupdate.UiElements.Controls;


namespace Scrupdate.UiElements.Windows
{
    public partial class ProgramUpdateCheckConfigurationCheckingWindow : Window, INotifyPropertyChanged
    {
        // Constants ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private const string QUESTION_DIALOG_MESSAGE__ARE_YOU_SURE_YOU_WANT_TO_CANCEL_THE_OPERATION_FORCEFULLY = "Are You Sure You Want to Cancel the Operation Forcefully?\r\n\r\n•  If you cancel the operation forcefully, ChromeDriver will not have a chance to delete its temporary files.";
        private const string STATUS_MESSAGE__CHECKING = "Checking…";
        private const string STATUS_MESSAGE__CANCELLING = "Cancelling…";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Enums ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum Operation
        {
            None,
            CancellingOperation,
            CheckingProgramUpdateCheckConfiguration
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly DependencyProperty CurrentOperationProperty = DependencyProperty.Register(
            nameof(CurrentOperation),
            typeof(Operation),
            typeof(ProgramUpdateCheckConfigurationCheckingWindow),
            new PropertyMetadata(Operation.None)
        );
        private bool showingBrowserWindow;
        private Program programToCheck;
        private Exception programUpdateCheckConfigurationCheckingException;
        private Program._UpdateCheckConfigurationError programUpdateCheckConfigurationError;
        private string foundProgramVersionString;
        private CancellableThread programUpdateCheckConfigurationCheckingCancellableThread;
        private volatile bool forceClosing;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Properties //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Size BaseSize { get; private set; }
        public Operation CurrentOperation
        {
            get
            {
                return ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () => (Operation)GetValue(CurrentOperationProperty)
                );
            }
            set
            {
                ThreadingUtilities.RunOnAnotherThread(
                    Dispatcher,
                    () =>
                        {
                            SetValue(CurrentOperationProperty, value);
                            PropertyChanged?.Invoke(
                                this,
                                new PropertyChangedEventArgs(nameof(CurrentOperation))
                            );
                        }
                );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ProgramUpdateCheckConfigurationCheckingWindow(Program programToCheck) : this(programToCheck, false) { }
        public ProgramUpdateCheckConfigurationCheckingWindow(Program programToCheck, bool showBrowserWindow)
        {
            showingBrowserWindow = showBrowserWindow;
            this.programToCheck = programToCheck;
            programUpdateCheckConfigurationCheckingException = null;
            programUpdateCheckConfigurationError = Program._UpdateCheckConfigurationError.None;
            foundProgramVersionString = null;
            InitializeComponent();
            BaseSize = new Size(Width, Height);
            WindowUtilities.ChangeWindowRenderingScaleAndMoveWindowIntoScreenBoundaries(
                this,
                BaseSize,
                App.WindowsRenderingScale
            );
            button_cancel.Focus();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Events //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnLoadedEvent(object sender, RoutedEventArgs e)
        {
            StartProgramUpdateCheckConfigurationCheckingTask();
        }
        private void OnClosingEvent(object sender, CancelEventArgs e)
        {
            if (forceClosing)
                return;
            if (CurrentOperation == Operation.CancellingOperation ||
                CurrentOperation == Operation.CheckingProgramUpdateCheckConfiguration)
            {
                e.Cancel = true;
                if (CurrentOperation == Operation.CancellingOperation)
                {
                    if (DialogUtilities.ShowQuestionDialog(
                            "",
                            QUESTION_DIALOG_MESSAGE__ARE_YOU_SURE_YOU_WANT_TO_CANCEL_THE_OPERATION_FORCEFULLY,
                            this
                        ) == true)
                    {
                        PrepareWindowForClosing(true);
                    }
                }
                else
                    CancelOperation();
                if (forceClosing)
                    e.Cancel = false;
            }
            else
            {
                PrepareWindowForClosing();
                Owner?.Activate();
            }
        }
        private void OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (CurrentOperation != Operation.CancellingOperation)
                    CancelOperation();
            }
        }
        private void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            CustomButton senderButton = (CustomButton)sender;
            if (senderButton == button_cancel)
                CancelOperation();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ChangeStatusMessage(string statusMessage)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () => label_statusMessage.Content = statusMessage
            );
        }
        private void StartProgramUpdateCheckConfigurationCheckingTask()
        {
            CurrentOperation = Operation.CheckingProgramUpdateCheckConfiguration;
            programUpdateCheckConfigurationCheckingCancellableThread = new CancellableThread(
                cancellationToken =>
                    {
                        ChangeStatusMessage(STATUS_MESSAGE__CHECKING);
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                foundProgramVersionString =
                                    ProgramsScanAndUpdatesCheckUtilities.CheckForAProgramUpdateAndGetLatestVersion(
                                        programToCheck,
                                        showingBrowserWindow,
                                        App.SettingsHandler,
                                        cancellationToken,
                                        out programUpdateCheckConfigurationError
                                    );
                            }
                            catch (Exception e)
                            {
                                programUpdateCheckConfigurationCheckingException = e;
                            }
                        }
                        CurrentOperation = Operation.None;
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            ThreadingUtilities.RunOnAnotherThread(
                                Dispatcher,
                                () => DialogResult = true
                            );
                        }
                        PrepareWindowForClosing(true);
                        programUpdateCheckConfigurationCheckingCancellableThread = null;
                    }
            );
            programUpdateCheckConfigurationCheckingCancellableThread.Start();
        }
        private void CancelOperation()
        {
            if (CurrentOperation != Operation.None)
            {
                if (CurrentOperation != Operation.CancellingOperation)
                {
                    CurrentOperation = Operation.CancellingOperation;
                    programUpdateCheckConfigurationCheckingCancellableThread?.RequestCancellation();
                }
                ChangeStatusMessage(STATUS_MESSAGE__CANCELLING);
            }
        }
        private void PrepareWindowForClosing()
        {
            PrepareWindowForClosing(false);
        }
        private void PrepareWindowForClosing(bool forceCloseWindow)
        {
            ThreadingUtilities.RunOnAnotherThread(
                Dispatcher,
                () =>
                    {
                        programUpdateCheckConfigurationCheckingCancellableThread?.RequestCancellation();
                        if (forceCloseWindow)
                        {
                            try
                            {
                                foreach (Process chromeDriverProcess in
                                         Process.GetProcessesByName(
                                             Path.GetFileNameWithoutExtension(
                                                 ChromeDriverUtilities.chromeDriverExecutableFilePath
                                             )
                                         ))
                                {
                                    chromeDriverProcess.Kill(true);
                                }
                            }
                            catch { }
                            forceClosing = true;
                            DialogUtilities.CloseDialogs(this);
                            try
                            {
                                ThreadingUtilities.RunOnAnotherThread(Dispatcher, Close);
                            }
                            catch { }
                        }
                    }
            );
        }
        public Exception GetProgramUpdateCheckConfigurationCheckingException()
        {
            return programUpdateCheckConfigurationCheckingException;
        }
        public Program._UpdateCheckConfigurationError GetProgramUpdateCheckConfigurationError()
        {
            return programUpdateCheckConfigurationError;
        }
        public string GetFoundProgramVersionString()
        {
            return foundProgramVersionString;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
