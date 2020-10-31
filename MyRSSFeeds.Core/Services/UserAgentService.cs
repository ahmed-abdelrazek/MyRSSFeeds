using LiteDB;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Services
{
    public static class UserAgentService
    {
        public static async Task<UserAgent> GetAgentAsync(UserAgent userAgent)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<UserAgent>(LiteDbContext.UserAgents);
                return col.FindById(userAgent.Id);
            });
        }

        public static async Task<IEnumerable<UserAgent>> GetAgentDataAsync(Expression<Func<UserAgent, bool>> predicate)
        {
            List<UserAgent> ls = new List<UserAgent>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = db.GetCollection<UserAgent>(LiteDbContext.UserAgents).Find(predicate).ToList();
                }
                return ls;
            });
        }

        public static async Task<UserAgent> AddNewAgentAsync(UserAgent userAgent)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<UserAgent>(LiteDbContext.UserAgents);
                col.Insert(userAgent);
                return userAgent.Id > 0 ? userAgent : null;
            });
        }

        public static async Task<bool> UpdateAgentAsync(UserAgent userAgent)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<UserAgent>(LiteDbContext.UserAgents);
                return col.Update(userAgent);
            });
        }

        public static async Task<bool> DeleteAgentAsync(UserAgent userAgent)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                return db.GetCollection<UserAgent>(LiteDbContext.UserAgents).Delete(userAgent.Id);
            });
        }
    }
}
