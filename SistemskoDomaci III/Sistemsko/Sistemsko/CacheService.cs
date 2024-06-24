using System;
using System.Collections.Generic;

namespace Sistemsko
{
    public class CacheService
    {
        private readonly Dictionary<string, (List<Clanak> Articles, DateTime Expiry)> _cache;
        private readonly TimeSpan _cacheDuration;

        public CacheService(TimeSpan cacheDuration)
        {
            _cache = new Dictionary<string, (List<Clanak>, DateTime)>();
            _cacheDuration = cacheDuration;
        }

        public bool TryGetArticles(string keyword, out List<Clanak> articles)
        {
            if (_cache.TryGetValue(keyword, out var cachedData))
            {
                if (DateTime.Now < cachedData.Expiry)
                {
                    articles = cachedData.Articles;
                    return true;
                }
                else
                {
                    _cache.Remove(keyword);
                }
            }
            articles = null;
            return false;
        }

        public void AddArticles(string keyword, List<Clanak> articles)
        {
            _cache[keyword] = (articles, DateTime.Now.Add(_cacheDuration));
        }
    }
}
