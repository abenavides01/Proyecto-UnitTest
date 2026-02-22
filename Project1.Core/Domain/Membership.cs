using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.Domain
{
    public class Membership
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = "";
        public MembershipStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? StatusChangedAtUtc { get; set; }
    }
}
