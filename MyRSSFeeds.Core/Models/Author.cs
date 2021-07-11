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

        public string? Name { get; set; }

        public string? Email { get; set; }

        [BsonIgnore]
        public string Username
        {
            get
            {
                if (string.IsNullOrEmpty(Email))
                {
                    Email = "";
                }
                if (string.IsNullOrEmpty(Name))
                {
                    Name = "";
                }
                return string.IsNullOrWhiteSpace(Name) ? Email : Name;
            }
        }

        [BsonIgnore]
        public Uri? SendEmail
        {
            get
            {
                var isValid = Uri.TryCreate($"mailto:{Email}", UriKind.RelativeOrAbsolute, out Uri? outUri);
                if (isValid)
                {
                    return outUri;
                }
                else
                {
                    return null;
                }
            }
        }

        public Uri? Uri { get; set; }

        public virtual ICollection<RSS> RSSs { get; set; }
    }
}
