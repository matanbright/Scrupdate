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
using System.Windows.Threading;


namespace Scrupdate.Classes.Utilities
{
    public static class ThreadingUtilities
    {
        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void RunOnAnotherThread(Dispatcher dispatcherOfThread, Action function)
        {
            if (dispatcherOfThread == null)
                throw new ArgumentNullException(nameof(dispatcherOfThread));
            if (function == null)
                throw new ArgumentNullException(nameof(function));
            try
            {
                dispatcherOfThread.Invoke(function);
            }
            catch { }
        }
        public static TReturn RunOnAnotherThread<TReturn>(Dispatcher dispatcherOfThread, Func<TReturn> function)
        {
            if (dispatcherOfThread == null)
                throw new ArgumentNullException(nameof(dispatcherOfThread));
            if (function == null)
                throw new ArgumentNullException(nameof(function));
            try
            {
                return dispatcherOfThread.Invoke(function);
            }
            catch
            {
                return default;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
