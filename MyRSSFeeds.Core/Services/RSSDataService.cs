using LiteDB;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Services
{
    public class RSSDataService : IRSSDataService
    {
        /// <summary>
        /// Gets all saved Feeds from database 
        /// </summary>
        /// <returns>IEnumerable of RSS</returns>
        public async Task<ILiteQueryable<RSS>> GetFeedsDataAsync()
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<RSS>(LiteDbContext.RSSs).Query().OrderByDescending(x => x.CreatedAt);
                }
            });
        }

        /// <summary>
        /// Gets limited number of saved feeds with sources
        /// </summary>
        /// <param name="limit">how many records to return</param>
        /// <returns>IEnumerable of RSS</returns>
        public async Task<IEnumerable<RSS>> GetFeedsDataAsync(int limit)
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return limit == 0
                        ? db.GetCollection<RSS>(LiteDbContext.RSSs).Include(x => x.PostSource).Query().OrderByDescending(x => x.CreatedAt).ToEnumerable()
                        : db.GetCollection<RSS>(LiteDbContext.RSSs).Include(x => x.PostSource).Query().OrderByDescending(x => x.CreatedAt).Limit(limit).ToEnumerable();
                }
            });
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <returns>IEnumerable of RSS</returns>
        public async Task<ILiteQueryable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate)
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<RSS>(LiteDbContext.RSSs).Query().Where(predicate).OrderByDescending(x => x.CreatedAt);
                }
            });
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <param name="skip">skip number of records</param>
        /// <returns>IEnumerable of RSS<RSS></returns>
        public async Task<IEnumerable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate, int skip)
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<RSS>(LiteDbContext.RSSs).Find(predicate, skip).OrderByDescending(x => x.CreatedAt);
                }
            });
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <param name="skip">skip x number of records</param>
        /// <param name="limit">how many records to return</param>
        /// <returns>IEnumerable<RSS></returns>
        public async Task<IEnumerable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate, int skip, int limit)
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<RSS>(LiteDbContext.RSSs).Find(predicate, skip, limit).OrderByDescending(x => x.CreatedAt);
                }
            });
        }

        /// <summary>
        /// Gets all the Feeds with source included
        /// </summary>
        /// <returns>IEnumerable of RSS</returns>
        public async Task<ILiteQueryable<RSS>> GetFeedsDataWithSourceAsync()
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<RSS>(LiteDbContext.RSSs).Include(x => x.PostSource).Query().OrderByDescending(x => x.CreatedAt);
                }
            });
        }

        /// <summary>
        /// Get a RSS Info by Guid proprty
        /// </summary>
        /// <param name="rss">the rss you want to find</param>
        /// <returns>Task Rss with its info from db</returns>
        public async Task<RSS> GetFeedAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                var col = db.GetCollection<RSS>(LiteDbContext.RSSs);
                return col.FindOne(x => x.Guid == rss.Guid);
            });
        }

        /// <summary>
        /// Check if the RSS exist in the db or not
        /// </summary>
        /// <param name="rss">The RSS you want to check exist</param>
        /// <returns>reutn true if the RSS exist</returns>
        public async Task<bool> FeedExistAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                var col = db.GetCollection<RSS>(LiteDbContext.RSSs);
                return col.Exists(x => x.Guid == rss.Guid);
            });
        }

        /// <summary>
        /// Add a new RSS to the database
        /// </summary>
        /// <param name="rss">the new RSS you want to add</param>
        /// <returns>Task The New RSS with the id from the database</returns>
        public async Task<RSS> AddNewFeedAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                var col = db.GetCollection<RSS>(LiteDbContext.RSSs);
                col.Insert(rss);
                return rss;
            });
        }

        /// <summary>
        /// Update a RSS to the database
        /// </summary>
        /// <param name="rss">the RSS you want to update</param>
        /// <returns>Task true if updated</returns>
        public async Task<bool> UpdateFeedAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                var col = db.GetCollection<RSS>(LiteDbContext.RSSs);
                return col.Update(rss);
            });
        }

        /// <summary>
        /// Delete a RSS from the database
        /// </summary>
        /// <param name="rss">the RSS you want to delete</param>
        /// <returns>Task true if deleted</returns>
        public async Task<bool> DeleteFeedAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                return db.GetCollection<RSS>(LiteDbContext.RSSs).Delete(rss.Id);
            });
        }

        /// <summary>
        /// Delete number of RSSs from the database
        /// </summary>
        /// <param name="predicate">the RSS where statement</param>
        /// <returns>number of deleted items</returns>
        public async Task<int> DeleteManyFeedsAsync(Expression<Func<RSS, bool>> predicate)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                return db.GetCollection<RSS>(LiteDbContext.RSSs).Include(x => x.PostSource).DeleteMany(predicate);
            });
        }
    }
}
