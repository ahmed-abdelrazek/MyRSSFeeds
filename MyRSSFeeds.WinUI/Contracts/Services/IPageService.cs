using System;

namespace MyRSSFeeds.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);
    }
}
