using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.Domain
{
    public class PointsAccount
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = "";
        public int Balance { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
