using LiteDB;
using MyRSSFeeds.Core.Models;
using System;
using System.Diagnostics;

namespace MyRSSFeeds.Core.Data
{
    public class LiteDbContext
    {
        public static string DbPath { get; set; }

        public static string RSSs { get; set; } = "RSSs";
        public static string Sources { get; set; } = "Sources";

        public static string ConnectionString { get; set; }

        public static LiteDatabase LiteDb { get; set; }

        /// <summary>
        /// Set the Database Connection string and initialize the database
        /// </summary>
        public static void InitializeDatabase()
        {
            if (string.IsNullOrEmpty(DbPath))
            {
                throw new ArgumentNullException("Db Path is Null or Empty");
            }

            ConnectionString = $"Filename={DbPath};Mode=Shared";

            LiteDb = new LiteDatabase(ConnectionString);
            try
            {
                LiteDb.Mapper.RegisterType<Uri>
                (
                    serialize: (uri) => uri.AbsoluteUri,
                    deserialize: (bson) => new Uri(bson.AsString)
                );

                LiteDb.Mapper.Entity<RSS>()
                    .DbRef(x => x.PostSource, Sources);

                LiteDb.Mapper.Entity<Source>()
                    .DbRef(x => x.RSSs, RSSs);

                var col1 = LiteDb.GetCollection<RSS>(RSSs).Query();
                col1.ToEnumerable();
                var col2 = LiteDb.GetCollection<Source>(Sources).Query();
                col2.ToEnumerable();

                LiteDb.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw ex;
            }
        }
    }
}
