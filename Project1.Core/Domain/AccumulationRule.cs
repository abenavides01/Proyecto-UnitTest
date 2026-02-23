using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.Domain
{
    public class AccumulationRule
    {
        public Guid Id { get; set; }
        public string ServiceCode { get; set; } = "";
        public int PointsPerUse { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
