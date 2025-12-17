using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostIQ.Core.Response
{
    public class ListResponse<TModel> : BaseResponse //, IListResponse<TModel> where TModel : class
    {
        public ListResponse(List<TModel> model, List<KeyValuePair<string, string[]>> validationErrors = null) : base(validationErrors)
        {
            Data = model;
        }

        public int Size { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
        public int Count { get; set; }
        public int TotalPages { get; set; }
        public List<TModel> Data { get; set; }  //this was only get --  public List<TModel> Data { get; } 
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}
