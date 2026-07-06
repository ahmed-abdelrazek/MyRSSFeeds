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

        private const string FirefoxAgentName = "Firefox 152, Windows";
        private const string FirefoxAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:152.0) Gecko/20100101 Firefox/152.0";
        private const string EdgeAgentName = "Edge 150, Windows";
        private const string EdgeAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36 Edg/150.0.0.0";
        private const string ChromeWindowsAgentName = "Chrome 149, Windows";
        private const string ChromeWindowsAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36";
        private const string ChromeLinuxAgentName = "Chrome 149, Linux";
        private const string ChromeLinuxAgentString = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36";
        private const string LegacyEdgeAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763";
        private const string LegacyFirefoxAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0";
        private const string LegacyChromeAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36";

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
                        Name = ChromeWindowsAgentName,
                        AgentString = ChromeWindowsAgentString,
                    },
                    new UserAgent
                    {
                        Name = EdgeAgentName,
                        AgentString = EdgeAgentString,
                    },
                    new UserAgent
                    {
                        Name = FirefoxAgentName,
                        AgentString = FirefoxAgentString,
                    },
                    new UserAgent
                    {
                        Name = ChromeLinuxAgentName,
                        AgentString = ChromeLinuxAgentString,
                    },
                    new UserAgent
                    {
                        Name = "IE 10 Windows 8",
                        AgentString = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)",
                    },
                    new UserAgent
                    {
                        Name = "MS Edge Windows 10",
                        AgentString = LegacyEdgeAgentString,
                    },
                    new UserAgent
                    {
                        Name = "IE 11 Windows 10",
                        AgentString = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729; rv:11.0) like Gecko",
                    },
                    new UserAgent
                    {
                        Name = "Firefox 73 Windows 10",
                        AgentString = LegacyFirefoxAgentString,
                    },
                    new UserAgent
                    {
                        Name = "Chrome 85 Windows 10",
                        AgentString = LegacyChromeAgentString,
                    }};

                    UserAgentsCollection.Insert(agentsList);
                }
                else
                {
                    IsFirstRun = false;
                    MigrateBuiltInUserAgents(UserAgentsCollection);
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

        /// <summary>
        /// Databases seeded by older versions carry outdated or typo'd browser agent
        /// strings (truncated "Safari/537.3", leading spaces, stale versions). Fix the
        /// known built-in entries in place - matched by their exact seeded names so
        /// user-added agents are never touched. IsUsed flags are preserved and the
        /// renames make re-runs no-ops.
        /// </summary>
        private static void MigrateBuiltInUserAgents(ILiteCollection<UserAgent> agents)
        {
            var fixes = new (string OldName, string NewName, string NewAgentString)[]
            {
                ("Firefox 130.0, Windows 10/11", FirefoxAgentName, FirefoxAgentString),
                ("Edge 129.0.0, Windows", EdgeAgentName, EdgeAgentString),
                ("Chrome 129.0.0, Windows", ChromeWindowsAgentName, ChromeWindowsAgentString),
                ("Chrome 129.0.0, Linux", ChromeLinuxAgentName, ChromeLinuxAgentString),
                ("MS Edge Windows 10", "MS Edge Windows 10", LegacyEdgeAgentString),
                ("Firefox 73 Windows 10", "Firefox 73 Windows 10", LegacyFirefoxAgentString),
                ("Chrome 85 Windows 10", "Chrome 85 Windows 10", LegacyChromeAgentString),
            };

            foreach (var (oldName, newName, newAgentString) in fixes)
            {
                var agent = agents.FindOne(x => x.Name == oldName);
                if (agent != null && (agent.Name != newName || agent.AgentString != newAgentString))
                {
                    agent.Name = newName;
                    agent.AgentString = newAgentString;
                    agents.Update(agent);
                }
            }

            // databases seeded before system info was available at startup carry an
            // empty "App Default" string, silently falling back to the hardcoded agent
            if (!string.IsNullOrEmpty(SystemInfo.AppVersion) && !string.IsNullOrEmpty(SystemInfo.OperatingSystemArchitecture))
            {
                var appDefault = agents.FindOne(x => x.Name == "App Default");
                if (appDefault != null && string.IsNullOrEmpty(appDefault.AgentString))
                {
                    appDefault.AgentString = $"MyRSSFeeds/{SystemInfo.AppVersion} (Windows NT 10.0; {SystemInfo.OperatingSystemArchitecture})";
                    agents.Update(appDefault);
                }
            }
        }
    }
}
