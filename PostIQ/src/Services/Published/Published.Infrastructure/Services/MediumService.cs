using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Published.Core.Models;
using System.Xml;

namespace Published.Infrastructure.Services
{
    public class MediumService
    {
        private readonly HttpClient _httpClient;

        public MediumService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<MediumPost>> GetPostsAsync(string userBaseUrl)
        {
            var feedUrl = BuildFeedUrl(userBaseUrl);

            using var stream = await _httpClient.GetStreamAsync(feedUrl);
            using var xmlReader = XmlReader.Create(stream);

            var reader = new RssFeedReader(xmlReader);
            var posts = new List<MediumPost>();

            while (await reader.Read())
            {
                if (reader.ElementType == SyndicationElementType.Item)
                {
                    var item = await reader.ReadItem();

                    posts.Add(new MediumPost
                    {
                        Title = item.Title,
                        Link = item.Links.FirstOrDefault()?.Uri.ToString(),
                        PublishedAt = item.Published,
                        Author = item.Contributors.FirstOrDefault()?.Name,
                        Tags = item.Categories.Select(c => c.Name)
                    });
                }
            }

            return posts;
        }

        private string BuildFeedUrl(string userBaseUrl)
        {
            var uri = new Uri(userBaseUrl);

            // https://medium.com/@username
            if (uri.AbsolutePath.StartsWith("/@"))
                return $"{uri.Scheme}://{uri.Host}/feed{uri.AbsolutePath}";

            // https://username.medium.com
            return $"{uri.Scheme}://{uri.Host}/feed";
        }
    }

}
