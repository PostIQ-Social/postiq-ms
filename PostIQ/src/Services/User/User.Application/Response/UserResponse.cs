using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Application.Response
{
    public record UserResponse(long UserId, string FirstName, string LastName);
}
