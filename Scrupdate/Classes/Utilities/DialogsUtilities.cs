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
using System.Windows;
using Scrupdate.UiElements.Windows;

namespace Scrupdate.Classes.Utilities
{
    public static class DialogsUtilities
    {
        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool? ShowErrorDialog(string dialogTitle, string dialogMessage, Window ownerWindow)
        {
            if (dialogTitle == null || dialogMessage == null)
                throw new ArgumentNullException();
            bool? returnValue = null;
            Action showErrorDialogCallback = (() =>
            {
                ErrorDialogWindow errorDialogWindow = new ErrorDialogWindow(dialogTitle, dialogMessage);
                if (ownerWindow == null)
                    errorDialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                else
                    errorDialogWindow.Owner = ownerWindow;
                returnValue = errorDialogWindow.ShowDialog();
            });
            if (ownerWindow != null)
                ThreadsUtilities.RunOnAnotherThread(ownerWindow.Dispatcher, showErrorDialogCallback);
            else
                showErrorDialogCallback.Invoke();
            return returnValue;
        }
        public static bool? ShowQuestionDialog(string dialogTitle, string dialogMessage, Window ownerWindow)
        {
            if (dialogTitle == null || dialogMessage == null)
                throw new ArgumentNullException();
            bool? returnValue = null;
            Action showQuestionDialogCallback = (() =>
            {
                QuestionDialogWindow questionDialogWindow = new QuestionDialogWindow(dialogTitle, dialogMessage);
                if (ownerWindow == null)
                    questionDialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                else
                    questionDialogWindow.Owner = ownerWindow;
                returnValue = questionDialogWindow.ShowDialog();
            });
            if (ownerWindow != null)
                ThreadsUtilities.RunOnAnotherThread(ownerWindow.Dispatcher, showQuestionDialogCallback);
            else
                showQuestionDialogCallback.Invoke();
            return returnValue;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
