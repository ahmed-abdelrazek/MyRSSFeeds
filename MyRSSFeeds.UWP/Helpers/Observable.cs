using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyRSSFeeds.UWP.Helpers
{
    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify the UI that this Property has changed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage">The variable that will set the property value</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property you going to notify the UI about
        /// (Optional if you are going to end it here and using it with the property you want to notify about)</param>
        /// <param name="action">Execute some code after the notify like nofity CanExecute for a command (Optional)</param>
        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null, Action action = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            if (action is null)
            {
                return;
            }
            action();
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
