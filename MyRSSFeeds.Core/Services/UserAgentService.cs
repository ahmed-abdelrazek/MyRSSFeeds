using LiteDB;
using MyRSSFeeds.Core.Contracts.Services;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Services
{
    public class UserAgentService : IUserAgentService
    {
        public async Task<ILiteQueryable<UserAgent>> GetAgentsDataAsync()
        {
            return await Task.Run(() =>
            {
                using LiteDatabase db = new(LiteDbContext.DbConnectionString);
                return db.GetCollection<UserAgent>(LiteDbContext.UserAgents).Query();
            });
        }

        public async Task<ILiteQueryable<UserAgent>> GetAgentDataAsync(Expression<Func<UserAgent, bool>> predicate)
        {
            return await Task.Run(() =>
            {
                using LiteDatabase db = new(LiteDbContext.DbConnectionString);
                return db.GetCollection<UserAgent>(LiteDbContext.UserAgents).Query().Where(predicate);
            });
        }

        public async Task<UserAgent> GetCurrentAgentAsync()
        {
            return await Task.Run(() =>
            {
                using LiteDatabase db = new(LiteDbContext.DbConnectionString);
                var agent = db.GetCollection<UserAgent>(LiteDbContext.UserAgents).Find(x => x.IsUsed).FirstOrDefault();
                return agent is null ? db.GetCollection<UserAgent>(LiteDbContext.UserAgents).Query().First() : agent;
            });
        }

        public async Task ResetAgentUseAsync()
        {
            List<UserAgent> ls = new();
            await Task.Run(() =>
            {
                using LiteDatabase db = new(LiteDbContext.DbConnectionString);
                ls = db.GetCollection<UserAgent>(LiteDbContext.UserAgents).Find(x => x.IsUsed).ToList();
            });

            foreach (var item in ls)
            {
                item.IsUsed = false;
                await UpdateAgentAsync(item);
            }
        }

        public async Task<UserAgent> AddNewAgentAsync(UserAgent userAgent)
        {
            return await Task.Run(() =>
            {
                using LiteDatabase db = new(LiteDbContext.DbConnectionString);
                var col = db.GetCollection<UserAgent>(LiteDbContext.UserAgents);
                col.Insert(userAgent);
                return userAgent;
            });
        }

        public async Task<bool> UpdateAgentAsync(UserAgent userAgent)
        {
            return await Task.Run(() =>
            {
                using LiteDatabase db = new(LiteDbContext.DbConnectionString);
                var col = db.GetCollection<UserAgent>(LiteDbContext.UserAgents);
                return col.Update(userAgent);
            });
        }

        public async Task<bool> DeleteAgentAsync(UserAgent userAgent)
        {
            return await Task.Run(() =>
            {
                using LiteDatabase db = new(LiteDbContext.DbConnectionString);
                return db.GetCollection<UserAgent>(LiteDbContext.UserAgents).Delete(userAgent.Id);
            });
        }
    }
}
