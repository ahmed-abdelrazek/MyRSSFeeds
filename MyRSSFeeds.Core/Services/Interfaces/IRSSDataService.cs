using LiteDB;
using MyRSSFeeds.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Services.Interfaces
{
    public interface IRSSDataService
    {
        Task<RSS> AddNewFeedAsync(RSS rss);
        Task<bool> DeleteFeedAsync(RSS rss);
        Task<int> DeleteManyFeedsAsync(Expression<Func<RSS, bool>> predicate);
        Task<bool> FeedExistAsync(RSS rss);
        Task<RSS> GetFeedAsync(RSS rss);
        Task<ILiteQueryable<RSS>> GetFeedsDataAsync();
        Task<ILiteQueryable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate);
        Task<IEnumerable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate, int skip);
        Task<IEnumerable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate, int skip, int limit);
        Task<IEnumerable<RSS>> GetFeedsDataAsync(int limit);
        Task<ILiteQueryable<RSS>> GetFeedsDataWithSourceAsync();
        Task<bool> UpdateFeedAsync(RSS rss);
    }
}