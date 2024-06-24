using System;

namespace Sistemsko
{
    public class ClanakObserver : IObserver<Clanak>
    {
        private readonly string name;

        public ClanakObserver(string name)
        {
            this.name = name;
        }

        public void OnNext(Clanak article)
        {
            Console.WriteLine($"Author: {article.Author}\nTitle: {article.Title}\n" +
                              $"Description: {article.Description}\nPublished: {article.PublishedAt}\n" +
                              $"Content: {article.Content}\n");
        }

        public void OnError(Exception e)
        {
            Console.WriteLine($"Error occurred: {e.Message}\n");
        }

        public void OnCompleted()
        {
            Console.WriteLine("All articles have been successfully retrieved.");
        }
    }
}
