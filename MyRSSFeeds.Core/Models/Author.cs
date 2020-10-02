using LiteDB;
using System;
using System.Collections.Generic;

namespace MyRSSFeeds.Core.Models
{
    public class Author
    {
        public Author()
        {
            RSSs = new HashSet<RSS>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        [BsonIgnore]
        public string Username
        {
            get
            {
                return string.IsNullOrWhiteSpace(Name) ? Email : Name;
            }
        }

        [BsonIgnore]
        public string SendEmail
        {
            get
            {
                return $"mailto:{Email}";
            }
        }

        public Uri Uri { get; set; }

        public virtual ICollection<RSS> RSSs { get; set; }
    }
}
