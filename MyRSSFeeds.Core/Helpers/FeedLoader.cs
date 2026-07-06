using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace MyRSSFeeds.Core.Helpers
{
    /// <summary>
    /// Loads a syndication feed from xml, falling back to a hand-rolled parser
    /// for legacy RSS versions (0.91-0.94) that Rss20FeedFormatter refuses -
    /// sites like pcworld.com still serve RSS 0.92
    /// </summary>
    public static class FeedLoader
    {
        public static SyndicationFeed Load(string feedXml)
        {
            try
            {
                using XmlReader xmlReader = XmlReader.Create(
                    new StringReader(feedXml),
                    new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true });
                return SyndicationFeed.Load(xmlReader);
            }
            catch (Exception ex) when (ex.Message.Contains("does not support RSS version"))
            {
                return ParseLegacyRss(feedXml);
            }
        }

        /// <summary>
        /// RSS 0.91-0.94 parser. Every channel/item element is optional in
        /// those specs, so all fields are read tolerantly.
        /// </summary>
        private static SyndicationFeed ParseLegacyRss(string feedXml)
        {
            var channel = XDocument.Parse(feedXml).Root?.Element("channel")
                ?? throw new XmlException("The feed has no rss/channel element.");

            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent(channel.Element("title")?.Value),
                Description = new TextSyndicationContent(channel.Element("description")?.Value),
                Language = channel.Element("language")?.Value
            };

            if (TryParseUri(channel.Element("link")?.Value, out Uri channelLink))
            {
                feed.Links.Add(new SyndicationLink(channelLink));
            }

            if (TryParseDate(channel.Element("lastBuildDate")?.Value ?? channel.Element("pubDate")?.Value, out DateTimeOffset updated))
            {
                feed.LastUpdatedTime = updated;
            }

            feed.Items = channel.Elements("item").Select(ParseLegacyItem).ToList();
            return feed;
        }

        private static SyndicationItem ParseLegacyItem(XElement item)
        {
            var syndicationItem = new SyndicationItem
            {
                Title = new TextSyndicationContent(item.Element("title")?.Value),
                Summary = new TextSyndicationContent(item.Element("description")?.Value)
            };

            string link = item.Element("link")?.Value;
            if (TryParseUri(link, out Uri itemLink))
            {
                syndicationItem.Links.Add(new SyndicationLink(itemLink));
            }

            // guid arrived in 0.94/2.0 but tolerate it anywhere; fall back to the link
            syndicationItem.Id = item.Element("guid")?.Value ?? link;

            if (TryParseDate(item.Element("pubDate")?.Value, out DateTimeOffset published))
            {
                syndicationItem.PublishDate = published;
            }

            string author = item.Element("author")?.Value;
            if (!string.IsNullOrWhiteSpace(author))
            {
                syndicationItem.Authors.Add(new SyndicationPerson(author));
            }

            return syndicationItem;
        }

        private static bool TryParseUri(string value, out Uri uri)
        {
            uri = null;
            return !string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value.Trim(), UriKind.Absolute, out uri);
        }

        private static bool TryParseDate(string value, out DateTimeOffset date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.Trim();
            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                || DateTimeOffset.TryParseExact(value, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date);
        }
    }
}
