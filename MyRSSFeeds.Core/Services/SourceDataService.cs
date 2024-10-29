using LiteDB;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace MyRSSFeeds.Core.Services
{
    public class SourceDataService
    {
        private readonly LiteDatabase _liteDatabase;

        public SourceDataService(LiteDatabase liteDatabase)
        {
            _liteDatabase = liteDatabase;
        }

        private IEnumerable<Source> AllFeedsBySource()
        {
            return AllSources().Include(x => x.RSSs).FindAll();
        }

        private ILiteCollection<Source> AllSources()
        {
            return _liteDatabase.GetCollection<Source>(LiteDbContext.Sources);
        }

        public IEnumerable<Source> GetSourcesData()
        {
            return AllSources().FindAll().OrderByDescending(x => x.Id).ToList();
        }

        public IEnumerable<Source> GetSourcesDataWithFeeds()
        {
            return AllFeedsBySource().ToList();
        }

        /// <summary>
        /// Check if Source Exist by its base Uri
        /// </summary>
        /// <param name="source">the base Uri for the source</param>
        /// <returns>Task true if the source already exist</returns>
        public bool SourceExist(string source)
        {
            var link = new Uri(source);
            var col = _liteDatabase.GetCollection<Source>(LiteDbContext.Sources);
            return col.Exists(x => x.RssUrl == link);
        }

        /// <summary>
        /// Check if Source Exist by its base Uri
        /// </summary>
        /// <param name="source">the base Uri for the source</param>
        /// <returns>Task true if the source already exist</returns>
        public bool SourceExist(Uri source)
        {
            var col = _liteDatabase.GetCollection<Source>(LiteDbContext.Sources);
            return col.Exists(x => x.RssUrl == source);
        }

        /// <summary>
        /// checks if a source works with last updated time and rss items count
        /// </summary>
        /// <param name="source">string for source rss url</param>
        /// <returns>Task (true if works, datetime offset for the last time website updated, int for rss items count)</returns>
        public async Task<(bool, DateTimeOffset, int)> IsSourceWorkingAsync(string source)
        {
            var feedString = await RssRequest.GetFeedAsStringAsync(source, new System.Threading.CancellationToken());

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(feedString)))
            {
                SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
                var lastUpdatedTime = feed.LastUpdatedTime;
                var rssItemsCount = feed.Items.Count();

                if (lastUpdatedTime.Year < 2020)
                {
                    if (rssItemsCount > 0)
                    {
                        var latestDateItem = feed.Items.OrderByDescending(x => x.PublishDate).FirstOrDefault();

                        if (latestDateItem == null || lastUpdatedTime.Year < 2020)
                        {
                            latestDateItem = feed.Items.OrderByDescending(x => x.LastUpdatedTime).FirstOrDefault();
                            lastUpdatedTime = latestDateItem.LastUpdatedTime;
                        }
                        else
                        {
                            lastUpdatedTime = latestDateItem.PublishDate;
                        }
                        if (lastUpdatedTime.Year < 2020)
                        {
                            lastUpdatedTime = DateTimeOffset.Now;
                        }
                    }
                    else
                    {
                        lastUpdatedTime = new DateTimeOffset(2020, 10, 30, 20, 00, 00, new TimeSpan(2, 0, 0));
                    }
                }
                return (true, lastUpdatedTime, rssItemsCount);
            }
        }

        /// <summary>
        /// Get source info from rss url
        /// </summary>
        /// <param name="source">string for rss/xml content</param>        
        /// <param name="rssUrl">string for source rss url</param>
        /// <returns>Task Source with all of its info or null of there is a problem</returns>
        public Source GetSourceInfoFromRss(string source, string rssUrl)
        {
            using XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { Async = true, IgnoreWhitespace = true, IgnoreComments = true });
            SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
            Uri baseLink = feed.Links.FirstOrDefault(x => x.MediaType == null)?.Uri;

            return new Source
            {
                SiteTitle = feed.Title.Text,
                Description = feed.Description.Text,
                Language = feed.Language,
                LastBuildDate = feed.LastUpdatedTime,
                BaseUrl = baseLink,
                RssUrl = new Uri(rssUrl),
                IsWorking = true
            };
        }

        public Source AddNewSource(Source source)
        {
            var col = _liteDatabase.GetCollection<Source>(LiteDbContext.Sources);
            col.Insert(source);
            return source;
        }

        public Source UpdateSource(Source source)
        {
            var col = _liteDatabase.GetCollection<Source>(LiteDbContext.Sources);
            col.Update(source);
            return source;
        }

        public bool DeleteSource(Source source)
        {
            return _liteDatabase.GetCollection<Source>(LiteDbContext.Sources).Delete(source.Id);
        }
    }
}
