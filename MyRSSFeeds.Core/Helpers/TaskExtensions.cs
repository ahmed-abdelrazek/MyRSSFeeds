using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Helpers
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Fire Task method in property or constructor 
        /// that can handle await and exception
        /// </summary>
        /// <param name="task">The task to complete</param>
        /// <param name="completedCallback">To do on complete</param>
        /// <param name="errorCallback">To do on exception</param>
        public static async void FireAndGet(this Task task, Action? completedCallback = null, Action<Exception>? errorCallback = null)
        {
            try
            {
                await task;
                completedCallback?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                errorCallback?.Invoke(ex);
            }
        }
    }
}
