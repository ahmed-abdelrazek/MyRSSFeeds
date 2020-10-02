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
    public static class RSSDataService
    {
        /// <summary>
        /// Gets all the Feeds with source included
        /// </summary>
        /// <param name="db">the database context</param>
        /// <returns>IEnumerable of RSS</returns>
        private static IEnumerable<RSS> AllFeedsWithSource(LiteDatabase db)
        {
            return db.GetCollection<RSS>(LiteDbContext.RSSs).Include(x => x.PostSource).FindAll();
        }

        /// <summary>
        /// Gets all the Feeds
        /// </summary>
        /// <param name="db">the database context</param>
        /// <returns>return IEnumerable of RSS</returns>
        private static ILiteCollection<RSS> AllFeeds(LiteDatabase db)
        {
            return db.GetCollection<RSS>(LiteDbContext.RSSs);
        }

        /// <summary>
        /// Gets all saved Feeds from database 
        /// </summary>
        /// <returns>IEnumerable of RSS</returns>
        public static async Task<IEnumerable<RSS>> GetFeedsDataAsync()
        {
            List<RSS> ls = new List<RSS>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = AllFeeds(db).FindAll().OrderByDescending(x => x.CreatedAt).ToList();
                }
                return ls;
            });
        }

        /// <summary>
        /// Gets limited number of saved feeds with sources
        /// </summary>
        /// <param name="limit">how many records to return</param>
        /// <returns>IEnumerable of RSS</returns>
        public static async Task<IEnumerable<RSS>> GetFeedsDataAsync(int limit)
        {
            List<RSS> ls = new List<RSS>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = limit == 0
                        ? AllFeedsWithSource(db).OrderByDescending(x => x.CreatedAt).ToList()
                        : AllFeedsWithSource(db).OrderByDescending(x => x.CreatedAt).Take(limit).ToList();
                }
                return ls;
            });
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <returns>IEnumerable of RSS</returns>
        public static async Task<IEnumerable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate)
        {
            List<RSS> ls = new List<RSS>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = AllFeeds(db).Find(predicate).OrderByDescending(x => x.CreatedAt).ToList();
                }
                return ls;
            });
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <param name="skip">skip number of records</param>
        /// <returns>IEnumerable of RSS<RSS></returns>
        public static async Task<IEnumerable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate, int skip)
        {
            List<RSS> ls = new List<RSS>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = AllFeeds(db).Find(predicate, skip).OrderByDescending(x => x.CreatedAt).ToList();
                }
                return ls;
            });
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <param name="skip">skip x number of records</param>
        /// <param name="limit">how many records to return</param>
        /// <returns>IEnumerable<RSS></returns>
        public static async Task<IEnumerable<RSS>> GetFeedsDataAsync(Expression<Func<RSS, bool>> predicate, int skip, int limit)
        {
            List<RSS> ls = new List<RSS>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = AllFeeds(db).Find(predicate, skip, limit).OrderByDescending(x => x.CreatedAt).ToList();
                }
                return ls;
            });
        }

        /// <summary>
        /// Gets all the Feeds with source included
        /// </summary>
        /// <returns>IEnumerable of RSS</returns>
        public static async Task<IEnumerable<RSS>> GetFeedsDataWithSourceAsync()
        {
            List<RSS> ls = new List<RSS>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = AllFeedsWithSource(db).OrderByDescending(x => x.CreatedAt).ToList();
                }
                return ls;
            });
        }

        /// <summary>
        /// Get a RSS Info by Guid proprty
        /// </summary>
        /// <param name="rss">the rss you want to find</param>
        /// <returns>Task Rss with its info from db</returns>
        public static async Task<RSS> GetFeedAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<RSS>(LiteDbContext.RSSs);
                return col.FindOne(x => x.Guid == rss.Guid);
            });
        }

        /// <summary>
        /// Check if the RSS exist in the db or not
        /// </summary>
        /// <param name="rss">The RSS you want to check exist</param>
        /// <returns>reutn true if the RSS exist</returns>
        public static async Task<bool> FeedExistAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<RSS>(LiteDbContext.RSSs);
                return col.Exists(x => x.Guid == rss.Guid);
            });
        }

        /// <summary>
        /// Add a new RSS to the database
        /// </summary>
        /// <param name="rss">the new RSS you want to add</param>
        /// <returns>Task The New RSS with the id from the database</returns>
        public static async Task<RSS> AddNewFeedAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<RSS>(LiteDbContext.RSSs);
                col.Insert(rss);
                return rss.Id > 0 ? rss : null;
            });
        }

        /// <summary>
        /// Update a RSS to the database
        /// </summary>
        /// <param name="rss">the RSS you want to update</param>
        /// <returns>Task true if updated</returns>
        public static async Task<bool> UpdateFeedAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<RSS>(LiteDbContext.RSSs);
                return col.Update(rss);
            });
        }

        /// <summary>
        /// Delete a RSS from the database
        /// </summary>
        /// <param name="rss">the RSS you want to delete</param>
        /// <returns>Task true if deleted</returns>
        public static async Task<bool> DeleteFeedAsync(RSS rss)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                return db.GetCollection<RSS>(LiteDbContext.Sources).Delete(rss.Id);
            });
        }
    }
}
