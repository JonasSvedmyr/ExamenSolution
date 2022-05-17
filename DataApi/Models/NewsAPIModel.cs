namespace DataApi.Models
{

    public class NewsAPIModel
    {
        public string Status { get; set; }
        public int TotalResults { get; set; }

        public Articles[] articles { get; set; }
    }

    public class Articles
    {
        public Source Source { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string urlToImage { get; set; }
        public string publishedAt { get; set; }
        public string content { get; set; }
        public DAL.Data.Entities.Article Id { get; internal set; }
    }
    public class Source
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
