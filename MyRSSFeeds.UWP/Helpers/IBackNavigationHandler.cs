using System;

namespace MyRSSFeeds.Helpers
{
    public interface IBackNavigationHandler
    {
        event EventHandler<bool> OnPageCanGoBackChanged;

        void GoBack();
    }
}
