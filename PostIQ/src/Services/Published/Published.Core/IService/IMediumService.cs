using Published.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Published.Core.IService
{
    public interface IMediumService
    {
        Task<IEnumerable<MediumPost>> GetPostsAsync(string userBaseUrl);
    }
}
