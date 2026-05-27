using System.Collections.Generic;
using System.Threading.Tasks;
using User.Core.Entities;

namespace User.Core.IServices;

public interface ISyncService
{
    string SourceName { get; }
    Task<List<Post>> FetchPostsAsync(string baseUrl);
}
