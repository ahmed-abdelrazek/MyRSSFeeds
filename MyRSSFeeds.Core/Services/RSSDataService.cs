using LiteDB;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MyRSSFeeds.Core.Services
{
    public class RSSDataService
    {
        private readonly LiteDatabase _liteDatabase;

        public RSSDataService(LiteDatabase liteDatabase)
        {
            _liteDatabase = liteDatabase;
        }

        /// <summary>
        /// Gets all the Feeds with source included
        /// </summary>
        /// <param name="db">the database context</param>
        /// <returns>IEnumerable of RSS</returns>
        private IEnumerable<RSS> AllFeedsWithSource()
        {
            return _liteDatabase.GetCollection<RSS>(LiteDbContext.RSSs).Include(x => x.PostSource).FindAll();
        }

        /// <summary>
        /// Gets all the Feeds
        /// </summary>
        /// <param name="db">the database context</param>
        /// <returns>return IEnumerable of RSS</returns>
        private ILiteCollection<RSS> AllFeeds()
        {
            return _liteDatabase.GetCollection<RSS>(LiteDbContext.RSSs);
        }

        /// <summary>
        /// Gets all saved Feeds from database 
        /// </summary>
        /// <returns>IEnumerable of RSS</returns>
        public IEnumerable<RSS> GetFeedsData()
        {
            return AllFeeds().FindAll().OrderByDescending(x => x.CreatedAt).ToList();
        }

        /// <summary>
        /// Gets limited number of saved feeds with sources
        /// </summary>
        /// <param name="limit">how many records to return</param>
        /// <returns>IEnumerable of RSS</returns>
        public IEnumerable<RSS> GetFeedsData(int limit)
        {
            return limit == 0
                ? AllFeedsWithSource().OrderByDescending(x => x.CreatedAt).ToList()
                : AllFeedsWithSource().OrderByDescending(x => x.CreatedAt).Take(limit).ToList();
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <returns>IEnumerable of RSS</returns>
        public IEnumerable<RSS> GetFeedsData(Expression<Func<RSS, bool>> predicate)
        {
            return AllFeeds().Find(predicate).OrderByDescending(x => x.CreatedAt).ToList();
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <param name="skip">skip number of records</param>
        /// <returns>IEnumerable of RSS<RSS></returns>
        public IEnumerable<RSS> GetFeedsData(Expression<Func<RSS, bool>> predicate, int skip)
        {
            return AllFeeds().Find(predicate, skip).OrderByDescending(x => x.CreatedAt).ToList();
        }

        /// <summary>
        /// Gets filtered feeds from db
        /// </summary>
        /// <param name="predicate">Where statement to filter the data</param>
        /// <param name="skip">skip x number of records</param>
        /// <param name="limit">how many records to return</param>
        /// <returns>IEnumerable<RSS></returns>
        public IEnumerable<RSS> GetFeedsData(Expression<Func<RSS, bool>> predicate, int skip, int limit)
        {
            return AllFeeds().Find(predicate, skip, limit).OrderByDescending(x => x.CreatedAt).ToList();
        }

        /// <summary>
        /// Gets all the Feeds with source included
        /// </summary>
        /// <returns>IEnumerable of RSS</returns>
        public IEnumerable<RSS> GetFeedsDataWithSource()
        {
            return AllFeedsWithSource().OrderByDescending(x => x.CreatedAt).ToList();
        }

        /// <summary>
        /// Get a RSS Info by Guid proprty
        /// </summary>
        /// <param name="rss">the rss you want to find</param>
        /// <returns>Task Rss with its info from db</returns>
        public RSS GetFeed(RSS rss)
        {
            var col = _liteDatabase.GetCollection<RSS>(LiteDbContext.RSSs);
            return col.FindOne(x => x.Guid == rss.Guid);
        }

        /// <summary>
        /// Check if the RSS exist in the db or not
        /// </summary>
        /// <param name="rss">The RSS you want to check exist</param>
        /// <returns>reutn true if the RSS exist</returns>
        public bool FeedExist(RSS rss)
        {
            var col = _liteDatabase.GetCollection<RSS>(LiteDbContext.RSSs);
            return col.Exists(x => x.Guid == rss.Guid);
        }

        /// <summary>
        /// Add a new RSS to the database
        /// </summary>
        /// <param name="rss">the new RSS you want to add</param>
        /// <returns>Task The New RSS with the id from the database</returns>
        public RSS AddNewFeed(RSS rss)
        {
            var col = _liteDatabase.GetCollection<RSS>(LiteDbContext.RSSs);
            col.Insert(rss);
            return rss.Id > 0 ? rss : null;
        }

        /// <summary>
        /// Update a RSS to the database
        /// </summary>
        /// <param name="rss">the RSS you want to update</param>
        /// <returns>Task true if updated</returns>
        public bool UpdateFeed(RSS rss)
        {
            var col = _liteDatabase.GetCollection<RSS>(LiteDbContext.RSSs);
            return col.Update(rss);
        }

        /// <summary>
        /// Delete a RSS from the database
        /// </summary>
        /// <param name="rss">the RSS you want to delete</param>
        /// <returns>Task true if deleted</returns>
        public bool DeleteFeed(RSS rss)
        {
            return _liteDatabase.GetCollection<RSS>(LiteDbContext.RSSs).Delete(rss.Id);
        }

        /// <summary>
        /// Delete number of RSSs from the database
        /// </summary>
        /// <param name="predicate">the RSS where statement</param>
        /// <returns>number of deleted items</returns>
        public int DeleteManyFeeds(Expression<Func<RSS, bool>> predicate)
        {
            return _liteDatabase.GetCollection<RSS>(LiteDbContext.RSSs).Include(x => x.PostSource).DeleteMany(predicate);
        }
    }
}
