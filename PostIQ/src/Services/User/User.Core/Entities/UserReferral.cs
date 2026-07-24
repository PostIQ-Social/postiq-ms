using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace User.Core.Entities
{
    public partial class UserReferral
    {
        [Key]
        public long ReferralId { get; set; }
		public long UserId { get; set; }
		public string UserName { get; set; } = null!;
		public string ReferralCode { get; set; } = null!;
		public long ReferredById { get; set; }
		public string ReferredByName { get; set; } = null!;
		public bool IsActive { get; set; }
		public DateTime CreatedOn { get; set; }
		public long CreatedBy { get; set; }
	}
}
