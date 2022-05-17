using DAL.Data;
using DAL.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAccessAPI.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NewsAccessAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private NewsContext _newsContext;

        public NewsController(NewsContext newsContext)
        {
            _newsContext = newsContext;
        }
        [AllowAnonymous]
        [HttpPost("GetLatestNews")]
        public async Task<ActionResult> GetLatestNews([FromBody] List<GetLatestNewsModel> model)
        {
            var dbCategories = _newsContext.Categoris.ToList();
            var artticlePreviews = new List<ArticlePreviewModel>();
            foreach (var item in model)
            {
                var articales = _newsContext.Articles.Include(x => x.ArticleCategoris).Where(x => x.ArticleCategoris.Any(x => x.categoriId == item.CategoryId)).OrderBy(x => x.publishedAt).Take(item.Count).ToList();
                foreach (var article in articales)
                {
                    var categories = new List<string>();
                    foreach (var category in article.ArticleCategoris)
                    {
                        categories.Add(dbCategories.Where(x => x.Id == category.categoriId).FirstOrDefault().Name);
                    }
                    artticlePreviews.Add(new ArticlePreviewModel
                    {
                        Id = article.Id,
                        description = article.description,
                        title = article.title,
                        author = article.author,
                        publishedAt = article.publishedAt,
                        SourceName = article.SourceName,
                        url = article.url,
                        urlToImage = article.urlToImage,
                        Categories = categories

                    });
                }
            }
            return Ok(artticlePreviews);
        }

        [AllowAnonymous]
        [HttpGet("GetArticle/{id}")]
        public async Task<ActionResult> GetArticle([FromRoute] string id)
        {
            var article = _newsContext.Articles.Include(x => x.ArticleCategoris).Where(x => x.Id == id).FirstOrDefault();
            var categories = new List<string>();
            var dbCategories = _newsContext.Categoris.ToList();
            foreach (var category in article.ArticleCategoris)
            {
                categories.Add(dbCategories.Where(x => x.Id == category.categoriId).FirstOrDefault().Name);
            }
            var response = new
            {
                Id = article.Id,
                description = article.description,
                title = article.title,
                author = article.author,
                content = article.content,
                publishedAt = article.publishedAt,
                SourceName = article.SourceName,
                url = article.url,
                urlToImage = article.urlToImage,
                Categories = categories
            };
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("GetCategories")]
        public async Task<ActionResult> GetCategories()
        {
            return Ok(_newsContext.Categoris.ToList());
        }

        [AllowAnonymous]
        [HttpGet("Search/{query}")]
        public async Task<ActionResult> Search([FromRoute] string query)
        {
            var keyWords = query.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            var queryString = "SELECT a.[Id],[SourceId],[SourceName],[author],[title],[description],[url],[urlToImage],[publishedAt],[content] FROM [NewsDB].[dbo].[Articles] as a left join ArticleCategori as ac on a.Id = ac.articleId left join Categoris as c on ac.categoriId = c.Id where ";

            for (int i = 0; i < keyWords.Length; i++)
            {
                if (i != 0)
                {
                    queryString = queryString + " and ";
                }
                queryString = queryString + $"([SourceName] like '%{keyWords[i]}%' or [author] like '%{keyWords[i]}%' or [title] like '%{keyWords[i]}%' or [description] like '%{keyWords[i]}%' or [content] like '%{keyWords[i]}%' or [c].[Name] like '%{keyWords[i]}%')";
            }
            Debug.WriteLine(queryString);
            var articleIds = _newsContext.Articles.FromSqlRaw(queryString).Select(x => x.Id).ToList();
            var categories = new List<string>();
            var articlePreviews = new List<ArticlePreviewModel>();
            var dbCategories = _newsContext.Categoris.ToList();

            foreach (var id in articleIds)
            {
                var article = _newsContext.Articles.Include(x => x.ArticleCategoris).Where(x => x.Id == id).FirstOrDefault();
                foreach (var category in article.ArticleCategoris)
                {
                    categories.Add(dbCategories.Where(x => x.Id == category.categoriId).FirstOrDefault().Name);
                }
                articlePreviews.Add(new ArticlePreviewModel
                {
                    Id = article.Id,
                    description = article.description,
                    title = article.title,
                    author = article.author,
                    publishedAt = article.publishedAt,
                    SourceName = article.SourceName,
                    url = article.url,
                    urlToImage = article.urlToImage,
                    Categories = categories

                });
            }
            return Ok(articlePreviews);
        }
    }
}


