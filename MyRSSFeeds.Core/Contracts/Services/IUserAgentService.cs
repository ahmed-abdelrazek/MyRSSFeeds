using LiteDB;
using MyRSSFeeds.Core.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Contracts.Services
{
    public interface IUserAgentService
    {
        Task<UserAgent> AddNewAgentAsync(UserAgent userAgent);
        Task<bool> DeleteAgentAsync(UserAgent userAgent);
        Task<ILiteQueryable<UserAgent>> GetAgentDataAsync(Expression<Func<UserAgent, bool>> predicate);
        Task<ILiteQueryable<UserAgent>> GetAgentsDataAsync();
        Task<UserAgent> GetCurrentAgentAsync();
        Task ResetAgentUseAsync();
        Task<bool> UpdateAgentAsync(UserAgent userAgent);
    }
}
