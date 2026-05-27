using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using User.Core.Entities;
using User.Core.IServices;

namespace User.Infrastructure.Services
{
    public class MediumSyncService : ISyncService
    {
        private readonly HttpClient _httpClient;

        public MediumSyncService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string SourceName => "Medium";

        public async Task<List<Post>> FetchPostsAsync(string baseUrl)
        {
            // Determine feed URL
            // Profile: https://medium.com/@username -> https://medium.com/feed/@username
            // Custom Domain/Pub: https://medium.com/publication -> https://medium.com/feed/publication
            // We'll simplistic logic: insert /feed/ before the last segment if not present.
            // Better: User provides just the username or full profile URL.
            
            string feedUrl = baseUrl;
            if (!baseUrl.Contains("/feed/"))
            {
                 // Handle common formats
                 if (baseUrl.Contains("medium.com/"))
                 {
                     feedUrl = baseUrl.Replace("medium.com/", "medium.com/feed/");
                 }
            }

            try 
            {
                var response = await _httpClient.GetStringAsync(feedUrl);
                var xdoc = XDocument.Parse(response);
                XNamespace dc = "http://purl.org/dc/elements/1.1/";
                XNamespace content = "http://purl.org/rss/1.0/modules/content/";

                var posts = new List<Post>();

                foreach (var item in xdoc.Descendants("item"))
                {
                    var post = new Post
                    {
                        Source = SourceName,
                        Title = item.Element("title")?.Value ?? "Untitled",
                        Link = item.Element("link")?.Value ?? "",
                        // Guid is usually the link or a specific guid tag
                        ExternalId = item.Element("guid")?.Value ?? item.Element("link")?.Value ?? Guid.NewGuid().ToString(),
                        PublishedDate = DateTime.TryParse(item.Element("pubDate")?.Value, out var date) ? date : DateTime.UtcNow,
                        Content = item.Element(content + "encoded")?.Value ?? item.Element("description")?.Value,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow
                    };
                    
                    // Harvest categories
                    var categories = item.Elements("category").Select(x => x.Value).ToList();
                    if(categories.Any())
                    {
                        post.Categories = string.Join(",", categories);
                    }

                    posts.Add(post);
                }

                return posts;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error fetching Medium feed: {ex.Message}");
                return new List<Post>();
            }
        }
    }
}
