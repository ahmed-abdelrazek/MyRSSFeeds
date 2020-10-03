using System;
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
        public async static void FireAndGet(this Task task, Action completedCallback, Action<Exception> errorCallback)
        {
            try
            {
                await task;
                completedCallback?.Invoke();
            }
            catch (Exception ex)
            {
                errorCallback?.Invoke(ex);
            }
        }
    }
}
