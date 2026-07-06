using LiteDB;
using LiteDB.Engine;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace MyRSSFeeds.Core.Data
{
    public class LiteDbContext
    {
        public static bool IsFirstRun { get; set; } = true;
        public static string DbPath { get; set; }

        public static string RSSs { get; set; } = "RSSs";
        public static string Sources { get; set; } = "Sources";
        public static string UserAgents { get; set; } = "UserAgents";

        public static ConnectionString ConnectionString { get; set; }

        /// <summary>
        /// Set the Database Connection string and initialize the database
        /// </summary>
        /// <returns>The open database; the caller owns it for the lifetime of the app</returns>
        public static LiteDatabase InitializeDatabase()
        {
            if (string.IsNullOrEmpty(DbPath))
            {
                throw new ArgumentNullException("Db Path is Null or Empty");
            }

            ConnectionString = new ConnectionString
            {
                Filename = DbPath,
                Connection = ConnectionType.Shared
            };

            var liteDb = new LiteDatabase(ConnectionString);
            try
            {
                liteDb.Mapper.RegisterType<Uri>
                (
                    serialize: (uri) => uri.AbsoluteUri,
                    deserialize: (bson) => new Uri(bson.AsString)
                );

                liteDb.Mapper.Entity<RSS>()
                    .DbRef(x => x.PostSource, Sources);

                liteDb.Mapper.Entity<Source>()
                    .DbRef(x => x.RSSs, RSSs);

                var RSSsCollection = liteDb.GetCollection<RSS>(RSSs).Query();
                RSSsCollection.ToEnumerable();
                var SourcesCollection = liteDb.GetCollection<Source>(Sources).Query();
                SourcesCollection.ToEnumerable();

                var UserAgentsCollection = liteDb.GetCollection<UserAgent>(UserAgents);
                if (UserAgentsCollection.Count() == 0)
                {
                    //get the system info to add to the user agent
                    //if one of them is missing will just put an empty string
                    // and the http client will see that and use the default hardcoded one there
                    string defaultAgentString =
                        string.IsNullOrEmpty(SystemInfo.AppVersion) || string.IsNullOrEmpty(SystemInfo.OperatingSystemArchitecture)
                            ? string.Empty
                            : $"MyRSSFeeds/{SystemInfo.AppVersion} (Windows NT 10.0; {SystemInfo.OperatingSystemArchitecture})";

                    var agentsList = new List<UserAgent> {
                    new UserAgent{
                        Name = "App Default",
                        AgentString = defaultAgentString,
                        IsUsed = true,
                        IsDeletable = false
                    },
                    new UserAgent
                    {
                        Name = "Chrome 129.0.0, Windows",
                        AgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.3",
                    },
                    new UserAgent
                    {
                        Name = "Edge 129.0.0, Windows",
                        AgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36 Edg/129.0.0.",
                    },
                    new UserAgent
                    {
                        Name = "Firefox 130.0, Windows 10/11",
                        AgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:130.0) Gecko/20100101 Firefox/130.0",
                    },
                    new UserAgent
                    {
                        Name = "Chrome 129.0.0, Linux",
                        AgentString = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36",
                    },
                    new UserAgent
                    {
                        Name = "IE 10 Windows 8",
                        AgentString = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)",
                    },
                    new UserAgent
                    {
                        Name = "MS Edge Windows 10",
                        AgentString = " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763",
                    },
                    new UserAgent
                    {
                        Name = "IE 11 Windows 10",
                        AgentString = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729; rv:11.0) like Gecko",
                    },
                    new UserAgent
                    {
                        Name = "Firefox 73 Windows 10",
                        AgentString = " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0",
                    },
                    new UserAgent
                    {
                        Name = "Chrome 85 Windows 10",
                        AgentString = " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36",
                    }};

                    UserAgentsCollection.Insert(agentsList);
                }
                else
                {
                    IsFirstRun = false;
                }

                liteDb.Rebuild(new RebuildOptions { Collation = new Collation($"{CultureInfo.CurrentCulture.TextInfo.CultureName}/IgnoreCase,IgnoreSymbols") });
                return liteDb;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                liteDb.Dispose();
                throw;
            }
        }
    }
}
