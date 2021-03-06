using DAL;
using DAL.Data;
using DAL.Data.Entities;
using DAL.Services;
using log4net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAccessAPI.Data;
using NewsAccessAPI.Data.Entities;
using NewsAccessAPI.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NewsAccessAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private NewsContext _newsContext;
        private UserManager<User> _userManager;
        private Context _context;
        private ILog _logger;

        public NewsController(NewsContext newsContext, Context context, UserManager<User> userManager, ILogger logger)
        {
            _newsContext = newsContext;
            _userManager = userManager;
            _context = context;
            _logger = logger.GetLogger(typeof(NewsController));
        }
        [AllowAnonymous]
        [HttpPost("GetLatestNews")]
        public async Task<ActionResult> GetLatestNews()
        {
            try
            {
                var auth = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                if (auth.Succeeded)//TODO if no usersettings landinpage will be null, add defualt
                {
                    var claimsPrincipal = auth.Principal;
                    var user = await _userManager.FindByNameAsync(claimsPrincipal.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value);

                    List<LatestNewsModel> latestNewsModels = new List<LatestNewsModel>();

                    var categoris = _context.categoris.Where(x => x.UserId == user.Id).ToList();
                    var sourceBlackList = _context.sourceBlackList.Where(x => x.UserId == user.Id).ToList();
                    string BlackListString = "";
                        if (sourceBlackList != null && sourceBlackList.Count > 0)
                        {
                            var temp = "and (";
                            for (int i = 0; i < sourceBlackList.Count; i++)
                            {
                                if (i != 0)
                                {
                                    temp += " and ";
                                }

                                temp += $"a.[SourceName] != '{sourceBlackList[i].SourceName}'";
                            }
                            temp += ")";

                        BlackListString = temp;
                        }
                    foreach (var categori in categoris)
                    {
                        var query = $"SELECT a.[Id],[SourceId],[SourceName],[author],[title],[description],[url],[urlToImage],[publishedAt],[content] FROM [NewsDB].[dbo].[Articles] as a left join ArticleCategori as ac on a.Id = ac.articleId left join Categoris as c on ac.categoriId = c.Id where c.[Name] = '{categori.CategoriName}' " + BlackListString;
                        Debug.WriteLine(query);
                        var articleIds = _newsContext.Articles.FromSqlRaw(query).OrderBy(x => x.publishedAt).Take(3).Select(x => x.Id).ToList();
                        List<ArticlePreviewModel> articlePreviews = GetArticels(articleIds);

                        latestNewsModels.Add(new LatestNewsModel { Articels = articlePreviews, Categori = categori.CategoriName });
                    }
                    return Ok(latestNewsModels);
                }
                else
                {
                    List<LatestNewsModel> latestNewsModels = new List<LatestNewsModel>();

                    var dbCategories = _newsContext.Categoris.ToList();

                    foreach (var categori in dbCategories)
                    {
                        var articlePreviews = new List<ArticlePreviewModel>();

                        var articales = _newsContext.Articles.Include(x => x.ArticleCategoris).Where(x => x.ArticleCategoris.Any(x => x.categoriId == categori.Id)).OrderBy(x => x.publishedAt).Take(3).ToList();
                        foreach (var article in articales)
                        {
                            var categories = new List<string>();
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

                        latestNewsModels.Add(new LatestNewsModel { Categori = categori.Name, Articels = articlePreviews });
                    }
                    return Ok(latestNewsModels);
                }

            }
            catch (System.Exception e)
            {
                _logger.Error(e);
                Debug.WriteLine(e);
                return StatusCode(500);
            }

        }

        [AllowAnonymous]
        [HttpGet("GetArticle/{id}")]
        public async Task<ActionResult> GetArticle([FromRoute] string id)
        {
            try
            {
                var article = _newsContext.Articles.Include(x => x.ArticleCategoris).Where(x => x.Id == id).FirstOrDefault();
                if(article != null)
                {
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
                else
                {
                    return BadRequest();
                }
            }
            catch (System.Exception e)
            {
                _logger.Error(e);
                return StatusCode(500);
            }
        }

        [AllowAnonymous]
        [HttpGet("GetCategories")]
        public async Task<ActionResult> GetCategories()
        {
            try
            {
                return Ok(_newsContext.Categoris.ToList());
            }
            catch (System.Exception e)
            {
                _logger.Error(e);
                return StatusCode(500);
            }
        }

        [AllowAnonymous]
        [HttpGet("Search/{query}")]
        public async Task<ActionResult> Search([FromRoute] string query)
        {
            try
            {
                //TODO Sanatise query
                var keyWords = query.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                var queryString = "SELECT a.[Id],[SourceId],[SourceName],[author],[title],[description],[url],[urlToImage],[publishedAt],[content] FROM [NewsDB].[dbo].[Articles] as a left join ArticleCategori as ac on a.Id = ac.articleId left join Categoris as c on ac.categoriId = c.Id where ";

                for (int i = 0; i < keyWords.Length; i++)
                {
                    if (i != 0)
                    {
                        queryString = queryString + " and ";
                    }
                    queryString = queryString + $"([SourceName] like '%{keyWords[i]}%' or [c].[Name] like '%{keyWords[i]}%' or [author] like '%{keyWords[i]}%' or [title] like '%{keyWords[i]}%' or [description] like '%{keyWords[i]}%' or [content] like '%{keyWords[i]}%' or [a].[Id] like '%{keyWords[i]}%')";
                }
                Debug.WriteLine(queryString);
                var articleIds = _newsContext.Articles.FromSqlRaw(queryString).Select(x => x.Id).ToList();
                List<ArticlePreviewModel> articlePreviews = GetArticels(articleIds);
                return Ok(articlePreviews);
            }
            catch (System.Exception e)
            {
                _logger.Error(e);
                return StatusCode(500);
            }


        }

        private List<ArticlePreviewModel> GetArticels(List<string> articleIds)
        {
            var categories = new List<string>();
            var articlePreviews = new List<ArticlePreviewModel>();
            var dbCategories = _newsContext.Categoris.ToList();

            foreach (var id in articleIds)
            {
                categories.Clear();
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

            return articlePreviews;
        }
    }
}


