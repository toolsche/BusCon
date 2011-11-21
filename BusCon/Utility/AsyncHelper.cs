using System;
using System.Threading;
using System.Windows;

namespace BusCon.Utility
{
    public class AsyncHelper
    {
        public static void RunAsync(Action action)
        {
            ThreadPool.QueueUserWorkItem((WaitCallback)(o => AsyncHelper.SafeExecute(action)));
        }

        public static void RunOnMainThread(Action action)
        {
            Deployment.Current.Dispatcher.BeginInvoke((Action)(() => AsyncHelper.SafeExecute(action)));
        }

        public static void SafeExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex1)
            {
                try
                {
                    GlobalExceptionHandler.HandleException(ex1);
                }
                catch (Exception ex2)
                {
                }
            }
        }
    }
}
