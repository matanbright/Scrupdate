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




using System;
using System.Windows;
using Scrupdate.UiElements.Windows;


namespace Scrupdate.Classes.Utilities
{
    public static class DialogUtilities
    {
        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool? ShowWarningDialog(string dialogTitle, string dialogMessage, Window ownerWindow)
        {
            return ShowDialog(DialogWindow.DialogType.Warning, dialogTitle, dialogMessage, ownerWindow);
        }
        public static bool? ShowErrorDialog(string dialogTitle, string dialogMessage, Window ownerWindow)
        {
            return ShowDialog(DialogWindow.DialogType.Error, dialogTitle, dialogMessage, ownerWindow);
        }
        public static bool? ShowQuestionDialog(string dialogTitle, string dialogMessage, Window ownerWindow)
        {
            return ShowDialog(DialogWindow.DialogType.Question, dialogTitle, dialogMessage, ownerWindow);
        }
        private static bool? ShowDialog(DialogWindow.DialogType dialogType, string dialogTitle, string dialogMessage, Window ownerWindow)
        {
            if (dialogTitle == null)
                throw new ArgumentNullException(nameof(dialogTitle));
            if (dialogMessage == null)
                throw new ArgumentNullException(nameof(dialogMessage));
            bool? returnValue = null;
            Action showDialogCallback =
                () =>
                    {
                        DialogWindow dialogWindow = new DialogWindow(dialogType, dialogTitle, dialogMessage);
                        if (ownerWindow == null)
                            dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        else
                            dialogWindow.Owner = ownerWindow;
                        returnValue = dialogWindow.ShowDialog();
                    };
            if (ownerWindow != null)
                ThreadingUtilities.RunOnAnotherThread(ownerWindow.Dispatcher, showDialogCallback);
            else
                showDialogCallback.Invoke();
            return returnValue;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
