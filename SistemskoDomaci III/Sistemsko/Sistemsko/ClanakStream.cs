using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Sistemsko
{
    public class ClanakStream : IObservable<Clanak>, IDisposable
    {
        private readonly Subject<Clanak> clanakSubject = new Subject<Clanak>();
        private readonly NewsApiService newsApiService = new NewsApiService();
        private readonly TopicModeling topicModeling = new TopicModeling();
        private readonly CacheService cacheService = new CacheService(TimeSpan.FromMinutes(30));

        public IDisposable Subscribe(IObserver<Clanak> observer)
        {
            return clanakSubject.Subscribe(observer);
        }

        public async Task GetArticlesAsync(string keyword)
        {
            try
            {
                if (cacheService.TryGetArticles(keyword, out var cachedArticles))
                {
                    foreach (var article in cachedArticles)
                    {
                        clanakSubject.OnNext(article);
                    }
                    DisplayTopicModelingResults(cachedArticles);
                    clanakSubject.OnCompleted();
                    return;
                }

                var articles = await newsApiService.GetArticlesWithMetadataAsync(keyword);
                var transformedArticles = new List<Clanak>();

                var tasks = articles.Select(article => Task.Run(() =>
                {
                    var transformedArticle = TransformArticle(article);
                    transformedArticles.Add(transformedArticle);
                    clanakSubject.OnNext(transformedArticle);
                })).ToList();

                await Task.WhenAll(tasks);

                cacheService.AddArticles(keyword, transformedArticles);
                DisplayTopicModelingResults(transformedArticles);
                clanakSubject.OnCompleted();
            }
            catch (Exception ex)
            {
                clanakSubject.OnError(ex);
            }
        }

        private void DisplayTopicModelingResults(List<Clanak> articles)
        {
            var descriptions = articles.Select(a => a.Description).ToList();
            var transformedData = topicModeling.PerformTopicModeling(descriptions);

            Console.WriteLine("LDA Topic Modeling Results:");
            for (int i = 0; i < transformedData.Count; i++)
            {
                var row = transformedData[i];
                Console.WriteLine($"Text: {row.Text}");
                Console.WriteLine($"LDA Features: {string.Join(", ", row.LdaFeatures)}");
                Console.WriteLine();
            }
        }

        private Clanak TransformArticle(Article article)
        {
            return new Clanak
            {
                Author = article.Author,
                Title = article.Title,
                Description = article.Description,
                PublishedAt = article.PublishedAt,
                Content = article.Content
            };
        }

        public void Dispose()
        {
            clanakSubject.Dispose();
        }
    }
}