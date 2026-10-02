using PostIQ.Core.Response;
using Published.Application.Response;

namespace PostIQ.API.Contracts
{
    public class ProfileResponse
    {
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ReferralCode { get; set; }
        public string Email { get; set; }
        //only contains baseurl and source, no other information about the job
        public ListResponse<UserJobResponse> Posts { get; set; }
    }
}
