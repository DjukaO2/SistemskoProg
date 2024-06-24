using System;
using System.Threading.Tasks;

namespace Sistemsko
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Unesite ključnu reč za pretragu članaka:");
            var keyword = Console.ReadLine();

            var clanakStream = new ClanakStream();

            clanakStream.Subscribe(new ClanakObserver("ArticleObserver"));

            try
            {
                await clanakStream.GetArticlesAsync(keyword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.ReadLine();
        }
    }
}