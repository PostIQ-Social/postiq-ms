using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostIQ.Core.Response
{

    public class CreatedResponse<TModel> : BaseResponse 
    {
        public CreatedResponse(TModel model, List<KeyValuePair<string, string[]>> validationErrors = null) : base(validationErrors)
        {
            Data = model;
        }

        public string Id { get; set; }
        public TModel Data { get; set; }

    }
}
