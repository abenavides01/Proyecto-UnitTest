using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.Domain
{
    public enum PointsMovementType
    {
        Earned = 1,
        Redeemed =2
    }
    public class PointsMovement
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = "";
        public PointsMovementType Type { get; set; }
        public int Points { get; set; }
        public string Reference { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
    }
}
