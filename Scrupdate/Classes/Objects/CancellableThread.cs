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
using System.Threading;


namespace Scrupdate.Classes.Objects
{
    public class CancellableThread
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class ThreadIsAlreadyRunningOrTerminatedException : Exception
        {
            private const string EXCEPTION_MESSAGE = "Thread is already running or terminated!";
            public ThreadIsAlreadyRunningOrTerminatedException() : base(EXCEPTION_MESSAGE) { }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Delegates ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public delegate void StartMethodDelegate(CancellationToken cancellationToken);
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Variables ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private Thread thread;
        private CancellationTokenSource cancellationTokenSource;
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Constructors ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CancellableThread(StartMethodDelegate startMethodDelegate) : this(startMethodDelegate, false) { }
        public CancellableThread(StartMethodDelegate startMethodDelegate, bool backgroundThread)
        {
            cancellationTokenSource = new CancellationTokenSource();
            thread = new Thread(() => startMethodDelegate.Invoke(cancellationTokenSource.Token));
            thread.IsBackground = backgroundThread;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Start()
        {
            if (!thread.ThreadState.HasFlag(ThreadState.Unstarted))
                throw new ThreadIsAlreadyRunningOrTerminatedException();
            thread.Start();
        }
        public void Join()
        {
            thread.Join();
        }
        public void Join(int timeoutInMilliseconds)
        {
            thread.Join(timeoutInMilliseconds);
        }
        public void RequestCancellation()
        {
            cancellationTokenSource.Cancel();
        }
        public bool IsBackgroundThread()
        {
            return thread.IsBackground;
        }
        public bool WasAlreadyStarted()
        {
            return !thread.ThreadState.HasFlag(ThreadState.Unstarted);
        }
        public bool IsAlive()
        {
            return thread.IsAlive;
        }
        public bool IsCancellationRequested()
        {
            return cancellationTokenSource.IsCancellationRequested;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
