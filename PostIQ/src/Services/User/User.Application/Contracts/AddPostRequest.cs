using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace User.Application.Contracts
{
    public class AddPostRequest
    {
        [Required]
        public string Source { get; set; } = null!;

        [Required]
        public string BaseUrl { get; set; } = null!;
    }
}
