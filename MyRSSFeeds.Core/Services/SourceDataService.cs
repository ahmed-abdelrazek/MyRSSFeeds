using LiteDB;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace MyRSSFeeds.Core.Services
{
    public static class SourceDataService
    {
        private static IEnumerable<Source> AllFeedsBySource(LiteDatabase db)
        {
            return AllSources(db).Include(x => x.RSSs).FindAll();
        }

        private static ILiteCollection<Source> AllSources(LiteDatabase db)
        {
            return db.GetCollection<Source>(LiteDbContext.Sources);
        }

        public static async Task<IEnumerable<Source>> GetSourcesDataAsync()
        {
            List<Source> ls = new List<Source>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = AllSources(db).FindAll().OrderByDescending(x => x.Id).ToList();
                }
                return ls;
            });
        }

        public static async Task<IEnumerable<Source>> GetSourcesDataWithFeedsAsync()
        {
            List<Source> ls = new List<Source>();

            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.ConnectionString))
                {
                    ls = AllFeedsBySource(db).ToList();
                }
                return ls;
            });
        }

        /// <summary>
        /// Check if Source Exist by its base Uri
        /// </summary>
        /// <param name="source">the base Uri for the source</param>
        /// <returns>Task true if the source already exist</returns>
        public static async Task<bool> SourceExistAsync(string source)
        {
            var link = new Uri(source);
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<Source>(LiteDbContext.Sources);
                return col.Exists(x => x.RssUrl == link);
            });
        }

        /// <summary>
        /// checks if a source works or not
        /// </summary>
        /// <param name="source">string for source rss url</param>
        /// <returns>Task true if works</returns>
        public static async Task<bool> IsSourceWorkingAsync(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                try
                {
                    var feedString = await RssRequest.GetFeedAsStringAsync(source);

                    if (feedString.Item1)
                    {
                        using (XmlReader xmlReader = XmlReader.Create(new StringReader(feedString.Item2)))
                        {
                            SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return false;
        }

        /// <summary>
        /// Get source info from rss url
        /// </summary>
        /// <param name="source">string for source rss url</param>
        /// <returns>Task Source with all of its info or null of there is a problem</returns>
        public static async Task<Source> GetSourceInfoFromRssAsync(string source)
        {
            var feedString = await RssRequest.GetFeedAsStringAsync(source);

            return await Task.Run(() =>
            {
                if (!string.IsNullOrWhiteSpace(source))
                {
                    try
                    {
                        if (feedString.Item1)
                        {
                            using XmlReader xmlReader = XmlReader.Create(new StringReader(feedString.Item2));
                            SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
                            Uri baseLink = feed.Links.FirstOrDefault(x => x.MediaType == null)?.Uri;

                            return new Source
                            {
                                SiteTitle = feed.Title.Text,
                                Description = feed.Description.Text,
                                Language = feed.Language,
                                LastBuildDate = feed.LastUpdatedTime,
                                BaseUrl = baseLink,
                                RssUrl = new Uri(source),
                                IsWorking = true
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
                return null;
            });
        }

        public static async Task<Source> AddNewSourceAsync(Source source)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<Source>(LiteDbContext.Sources);
                col.Insert(source);
                return source;
            });
        }

        public static async Task<Source> UpdateSourceAsync(Source source)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                var col = db.GetCollection<Source>(LiteDbContext.Sources);
                col.Update(source);
                return source;
            });
        }

        public static async Task<bool> DeleteSourceAsync(Source source)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.ConnectionString);
                return db.GetCollection<Source>(LiteDbContext.Sources).Delete(source.Id);
            });
        }
    }
}
