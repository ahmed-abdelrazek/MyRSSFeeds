using LiteDB;
using MyRSSFeeds.Core.Models;
using System;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Services.Interfaces
{
    public interface ISourceDataService
    {
        Task<Source> AddNewSourceAsync(Source source);
        Task<bool> DeleteSourceAsync(Source source);
        Task<Source?> GetSourceInfoFromRssAsync(string? source, string? rssUrl);
        Task<ILiteQueryable<Source>> GetSourcesDataAsync();
        Task<ILiteQueryable<Source>> GetSourcesDataWithFeedsAsync();
        Task<(bool, DateTimeOffset, int)> IsSourceWorkingAsync(string source);
        Task<bool> SourceExistAsync(string source);
        Task<Source> UpdateSourceAsync(Source source);
    }
}