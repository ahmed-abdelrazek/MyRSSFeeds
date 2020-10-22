using LiteDB;
using MyRSSFeeds.Core.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Models
{
    public class Source : BaseModel
    {
        public Source()
        {
            RSSs = new HashSet<RSS>();
        }

        public int Id { get; set; }

        public string SiteTitle { get; set; }

        public Uri BaseUrl { get; set; }

        public Uri RssUrl { get; set; }

        public string Description { get; set; }

        public DateTimeOffset LastBuildDate { get; set; }

        public string Language { get; set; }

        private bool _isChecking;

        [JsonIgnore]
        [BsonIgnore]
        public bool IsChecking
        {
            get { return _isChecking; }
            set
            {
                Set(ref _isChecking, value);
            }
        }

        private bool _isWorking;

        [JsonIgnore]
        [BsonIgnore]
        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                Set(ref _isWorking, value);
            }
        }

        [JsonIgnore]
        public virtual ICollection<RSS> RSSs { get; set; }

        public async Task CheckIfSourceWorking()
        {
            IsChecking = true;

            if (RssUrl != null)
            {
                IsWorking = await SourceDataService.IsSourceWorkingAsync(RssUrl.AbsoluteUri);
            }

            IsChecking = false;
        }
    }
}
