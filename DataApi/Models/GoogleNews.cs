namespace DataApi.Models
{

    public class GoogleNews
    {
        public Feed feed { get; set; }
        public Article[] articles { get; set; }
    }

    public class Feed
    {
        public string title { get; set; }
        public string updated { get; set; }
        public string link { get; set; }
        public string language { get; set; }
        public string subtitle { get; set; }
        public string rights { get; set; }
    }

    public class Article
    {
        public string id { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public string published { get; set; }
        public Sub_Articles[] sub_articles { get; set; }
        public Source source { get; set; }
    }

    //public class Source
    //{
    //    public string href { get; set; }
    //    public string title { get; set; }
    //}

    public class Sub_Articles
    {
        public string url { get; set; }
        public string title { get; set; }
        public string publisher { get; set; }
    }


}
