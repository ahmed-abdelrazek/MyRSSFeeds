using LiteDB;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MyRSSFeeds.Core.Services
{
    public class UserAgentService
    {
        private readonly LiteDatabase _liteDatabase;

        public UserAgentService(LiteDatabase liteDatabase)
        {
            _liteDatabase = liteDatabase;
        }

        public IEnumerable<UserAgent> GetAgentsData()
        {
            return _liteDatabase.GetCollection<UserAgent>(LiteDbContext.UserAgents).FindAll().ToList();
        }

        public IEnumerable<UserAgent> GetAgentData(Expression<Func<UserAgent, bool>> predicate)
        {
            return _liteDatabase.GetCollection<UserAgent>(LiteDbContext.UserAgents).Find(predicate).ToList();
        }

        public void ResetAgentUse()
        {
            List<UserAgent> ls = _liteDatabase.GetCollection<UserAgent>(LiteDbContext.UserAgents).Find(x => x.IsUsed).ToList();

            foreach (var item in ls)
            {
                item.IsUsed = false;
                UpdateAgent(item);
            }
        }

        public UserAgent AddNewAgent(UserAgent userAgent)
        {
            var col = _liteDatabase.GetCollection<UserAgent>(LiteDbContext.UserAgents);
            col.Insert(userAgent);
            return userAgent.Id > 0 ? userAgent : null;
        }

        public bool UpdateAgent(UserAgent userAgent)
        {
            var col = _liteDatabase.GetCollection<UserAgent>(LiteDbContext.UserAgents);
            return col.Update(userAgent);
        }

        public bool DeleteAgent(UserAgent userAgent)
        {
            return _liteDatabase.GetCollection<UserAgent>(LiteDbContext.UserAgents).Delete(userAgent.Id);
        }
    }
}
