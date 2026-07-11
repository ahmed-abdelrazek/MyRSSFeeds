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
    /// Loads a syndication feed from xml, falling back to hand-rolled parsers
    /// for formats SyndicationFeed.Load refuses: legacy RSS versions (0.91-0.94,
    /// sites like pcworld.com still serve RSS 0.92) and RDF/RSS 1.0 (slashdot.org)
    /// </summary>
    public static class FeedLoader
    {
        private static readonly XNamespace RdfNs = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        private static readonly XNamespace Rss10Ns = "http://purl.org/rss/1.0/";
        private static readonly XNamespace DublinCoreNs = "http://purl.org/dc/elements/1.1/";

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
            catch (Exception ex) when (ex.Message.Contains("is not an allowed feed format"))
            {
                var root = XDocument.Parse(feedXml).Root;
                if (root?.Name != RdfNs + "RDF")
                {
                    throw;
                }
                return ParseRdfRss(root);
            }
        }

        /// <summary>
        /// RDF/RSS 1.0 parser. The channel holds the feed metadata while the
        /// items sit next to it, and dates/authors come from Dublin Core.
        /// </summary>
        private static SyndicationFeed ParseRdfRss(XElement root)
        {
            var channel = root.Element(Rss10Ns + "channel")
                ?? throw new XmlException("The feed has no rdf/channel element.");

            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent(channel.Element(Rss10Ns + "title")?.Value),
                Description = new TextSyndicationContent(channel.Element(Rss10Ns + "description")?.Value),
                Language = channel.Element(DublinCoreNs + "language")?.Value
            };

            if (TryParseUri(channel.Element(Rss10Ns + "link")?.Value, out Uri channelLink))
            {
                feed.Links.Add(new SyndicationLink(channelLink));
            }

            if (TryParseDate(channel.Element(DublinCoreNs + "date")?.Value, out DateTimeOffset updated))
            {
                feed.LastUpdatedTime = updated;
            }

            feed.Items = root.Elements(Rss10Ns + "item").Select(ParseRdfItem).ToList();
            return feed;
        }

        private static SyndicationItem ParseRdfItem(XElement item)
        {
            var syndicationItem = new SyndicationItem
            {
                Title = new TextSyndicationContent(item.Element(Rss10Ns + "title")?.Value),
                Summary = new TextSyndicationContent(item.Element(Rss10Ns + "description")?.Value)
            };

            string link = item.Element(Rss10Ns + "link")?.Value;
            if (TryParseUri(link, out Uri itemLink))
            {
                syndicationItem.Links.Add(new SyndicationLink(itemLink));
            }

            syndicationItem.Id = item.Attribute(RdfNs + "about")?.Value ?? link;

            if (TryParseDate(item.Element(DublinCoreNs + "date")?.Value, out DateTimeOffset published))
            {
                syndicationItem.PublishDate = published;
            }

            string author = item.Element(DublinCoreNs + "creator")?.Value;
            if (!string.IsNullOrWhiteSpace(author))
            {
                syndicationItem.Authors.Add(new SyndicationPerson(author));
            }

            return syndicationItem;
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
