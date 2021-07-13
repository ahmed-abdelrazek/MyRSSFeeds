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

        public string? PostTitle { get; set; }

        [BsonIgnore]
        public string PostShortTitle
        {
            get => string.IsNullOrEmpty(PostTitle) ? "" : PostTitle.Length > 80 ? string.Concat(PostTitle.Substring(0, 80), " ...") : PostTitle;
        }

        public Uri? URL { get; set; }

        [BsonIgnore]
        public Uri LaunchURL
        {
            get => Uri.IsWellFormedUriString(Guid, UriKind.Absolute) && (Guid.StartsWith("http://") || Guid.StartsWith("https://"))
                    ? new Uri(Guid)
                    : URL is null ? new Uri("about:blank") : URL;
        }

        public string? Thumbnail { get; set; }

        public string? Guid { get; set; }

        public string Description { get; set; } = "";

        public virtual ICollection<Author> Authors { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        [BsonIgnore]
        public DateTime CreatedAtLocalTime => CreatedAt.LocalDateTime;

        private bool _isRead;

        public bool IsRead
        {
            get => _isRead;
            set => Set(ref _isRead, value);
        }

        public virtual Source? PostSource { get; set; }
    }
}
