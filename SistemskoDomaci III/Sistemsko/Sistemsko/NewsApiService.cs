using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sistemsko
{
    public class NewsApiService
    {
        private static readonly string apiKey = "dc880fda11d04354b0f788bc72fb65f3";  
        private readonly NewsApiClient _newsApiClient;

        public NewsApiService()
        {
            _newsApiClient = new NewsApiClient(apiKey);
        }

        public async Task<List<Article>> GetArticlesWithMetadataAsync(string keyword)
        {
            var articlesResponse = await _newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = keyword,
                From = new DateTime(2024, 5, 25),
                SortBy = SortBys.Popularity,
                Language = Languages.EN
            });

            if (articlesResponse.Status == Statuses.Ok)
            {
                return MapArticles(articlesResponse.Articles);
            }
            else
            {
                throw new Exception($"Error fetching articles: {articlesResponse.Error.Message}");
            }
        }

        private List<Article> MapArticles(List<NewsAPI.Models.Article> apiArticles)
        {
            var articles = new List<Article>();

            foreach (var apiArticle in apiArticles)
            {
                var article = new Article
                {
                    Source = new Source { Id = apiArticle.Source.Id, Name = apiArticle.Source.Name },
                    Author = apiArticle.Author,
                    Title = apiArticle.Title,
                    Description = apiArticle.Description,
                    Url = apiArticle.Url,
                    UrlToImage = apiArticle.UrlToImage,
                    PublishedAt = apiArticle.PublishedAt ?? DateTime.MinValue, // Handing nullable DateTime
                    Content = apiArticle.Content
                };
                articles.Add(article);
            }

            return articles;
        }
    }

    public class Article
    {
        public Source Source { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string UrlToImage { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Content { get; set; }
    }

    public class Source
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
