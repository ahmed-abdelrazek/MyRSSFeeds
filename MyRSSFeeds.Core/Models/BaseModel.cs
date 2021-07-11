using LiteDB;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyRSSFeeds.Core.Models
{
    public class BaseModel : INotifyPropertyChanged
    {
        [BsonIgnore]
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            if (!string.IsNullOrEmpty(propertyName))
            {
                OnPropertyChanged(propertyName);
            }
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
