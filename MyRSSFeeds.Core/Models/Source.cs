using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

        private string _description;

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;

                    if (string.IsNullOrWhiteSpace(_description))
                    {
                        IsShowDescription = false;
                    }
                    else
                    {
                        IsShowDescription = true;
                    }
                }
            }
        }

        private bool _isShowDescription;

        [JsonIgnore]
        [BsonIgnore]
        public bool IsShowDescription
        {
            get { return _isShowDescription; }
            set
            {
                Set(ref _isShowDescription, value);
            }
        }

        public DateTimeOffset LastBuildDate { get; set; }

        public DateTime LocalLastBuildDate { get => LastBuildDate.LocalDateTime; }

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

        private bool _isError;

        [JsonIgnore]
        [BsonIgnore]
        public bool IsError
        {
            get { return _isError; }
            set
            {
                Set(ref _isError, value);
            }
        }

        private string _errorMessage;

        [JsonIgnore]
        [BsonIgnore]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                Set(ref _errorMessage, value);
            }
        }

        [JsonIgnore]
        public virtual ICollection<RSS> RSSs { get; set; }
    }
}
