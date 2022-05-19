using DAL;
using DAL.Data;
using DAL.Data.Entities;
using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DataApi.Job
{
    public class NewsAPIJob : IJob
    {
        private readonly ILog _logger;
        public NewsAPIJob()//Must be an empthy constructor
        {
            var logger = new Logger("DataApi.txt");
            _logger = logger.GetLogger(typeof(NewsAPIJob));
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Debug.WriteLine("Starting fetch");
                var Categories = new List<Categori>();
                using (NewsContext newsContext = new NewsContext())
                {
                    Categories = newsContext.Categoris.ToList();
                }
                var articels = new List<Article>();
                Config config = new Config();
                using (StreamReader r = new StreamReader("Config.json"))
                {
                    string json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<Config>(json);
                }
                foreach (var categori in Categories)
                {
                    var url = "https://newsapi.org/v2/top-headlines?country=us&category=" + categori.Name + "&apiKey=" + config.Key;
                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(url),
                    };
                    Debug.WriteLine($"Getting Data:{categori}");
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var body = await response.Content.ReadFromJsonAsync<Models.NewsAPIModel>();

                        foreach (var article in body.articles)
                        {
                            var articleCategori = new ArticleCategori { categoriId = categori.Id, id = Guid.NewGuid().ToString() };

                            var newArticle = new Article
                            {
                                Id = Guid.NewGuid().ToString(),
                                author = article.author,
                                content = article.content,
                                description = article.description,
                                publishedAt = article.publishedAt,
                                SourceId = article.Source.Id,
                                SourceName = article.Source.Name,
                                title = article.title,
                                url = article.url,
                                urlToImage = article.urlToImage,
                            };

                            newArticle.ArticleCategoris = new List<ArticleCategori>();
                            newArticle.ArticleCategoris.Add(articleCategori);

                            articels.Add(newArticle);
                            Debug.WriteLine($"Source:{newArticle.SourceName} Author:{newArticle.author} Title:{newArticle.title} Categori:{categori.Name}");
                        }
                    }
                }
                try
                {
                    var tempArticels = new List<Article>();
                    articels = articels.OrderBy(x => x.SourceName).ToList();
                    foreach (var article in articels)
                    {
                        var tempArticle = tempArticels.Where(x => x.SourceId == article.SourceId && x.SourceName == article.SourceName && x.author == article.author && x.title == article.title && x.publishedAt == article.publishedAt).FirstOrDefault();

                        if (tempArticle != null)
                        {
                            if (!tempArticle.ArticleCategoris.Contains(article.ArticleCategoris.FirstOrDefault()))
                            {
                                tempArticle.ArticleCategoris.Add(article.ArticleCategoris.FirstOrDefault());
                            }
                        }
                        else
                        {
                            tempArticels.Add(article);
                        }
                    }
                    tempArticels = tempArticels.OrderBy(x => x.SourceName).ToList();
                    Debug.WriteLine("Saving Data");
                    using (NewsContext newsContext = new NewsContext())
                    {
                        foreach (var tempArticle in tempArticels)
                        {
                            Debug.WriteLine($"Source:{tempArticle.SourceName} Author:{tempArticle.author} Title:{tempArticle.title} Categoris:{tempArticle.ArticleCategoris.Count()}");
                            var dbArticle = newsContext.Articles.Include(x => x.ArticleCategoris).Where(x => x.SourceId == tempArticle.SourceId && x.SourceName == tempArticle.SourceName && x.author == tempArticle.author && x.title == tempArticle.title && x.publishedAt == tempArticle.publishedAt).FirstOrDefault();
                            if (dbArticle != null)
                            {
                                foreach (var articleCategori in tempArticle.ArticleCategoris)
                                {

                                    if (dbArticle.ArticleCategoris.Where(x => x.categoriId == articleCategori.categoriId).Count() == 0)
                                    {
                                        dbArticle.ArticleCategoris.Add(articleCategori);
                                    }
                                }
                            }
                            else
                            {
                                newsContext.Articles.Add(tempArticle);
                            }

                        }
                        newsContext.SaveChanges();
                        
                    }
                    Debug.WriteLine("Fetch complete");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    Debug.WriteLine(e.StackTrace);
                    _logger.Error(e);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debug.WriteLine(e.StackTrace);
                _logger.Error(e);
            }
            
        }
    }
}
