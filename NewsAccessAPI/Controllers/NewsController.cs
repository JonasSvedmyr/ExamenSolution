using DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAccessAPI.Models;
using System.Collections.Generic;
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
    }
}


