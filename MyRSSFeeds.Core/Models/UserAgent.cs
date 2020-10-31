namespace MyRSSFeeds.Core.Models
{
    public class UserAgent
    {
        public UserAgent()
        {
            IsDeletable = true;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string AgentString { get; set; }

        public bool IsUsed { get; set; }

        public bool IsDeletable { get; set; }
    }
}
