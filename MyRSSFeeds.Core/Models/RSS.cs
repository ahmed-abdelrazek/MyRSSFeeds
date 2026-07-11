using LiteDB;
using System;
using System.Collections.Generic;

namespace MyRSSFeeds.Core.Models
{
    public class RSS : BaseModel
    {
        public RSS()
        {
            Authors = new HashSet<Author>();
        }

        public int Id { get; set; }

        public string PostTitle { get; set; }

        [BsonIgnore]
        public string PostShortTitle
        {
            get
            {
                if (PostTitle.Length > 80)
                {
                    return System.Net.WebUtility.HtmlDecode(string.Concat(PostTitle.Substring(0, 80), " ..."));
                }
                else
                {
                    return System.Net.WebUtility.HtmlDecode(PostTitle);
                }
            }
        }

        public Uri URL { get; set; }

        [BsonIgnore]
        public Uri LaunchURL
        {
            get
            {
                if (Uri.IsWellFormedUriString(Guid, UriKind.Absolute) && (Guid.StartsWith("http://") || Guid.StartsWith("https://")))
                {
                    return new Uri(Guid);
                }
                else
                {
                    return URL;
                }
            }
        }

        public string Thumbnail { get; set; }

        public string Guid { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Author> Authors { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        [BsonIgnore]
        public DateTime CreatedAtLocalTime => CreatedAt.LocalDateTime;

        private bool _isRead;

        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                Set(ref _isRead, value);
            }
        }

        public virtual Source PostSource { get; set; }
    }
}
