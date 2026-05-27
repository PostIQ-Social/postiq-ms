using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostIQ.Core.Response
{
    /// <summary>
    /// Marker interface to define list response
    /// </summary>
    public interface IListResponse { }

    /// <summary>
    /// Define a list response with a payload
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public interface IListResponse<TModel> : IListResponse where TModel : class
    {
        List<TModel> Data { get; }
    }
}
