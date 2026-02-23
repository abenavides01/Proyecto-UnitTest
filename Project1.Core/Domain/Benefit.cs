using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.Domain
{
    public class Benefit
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int PointsCost { get; set; }
        public bool IsActive { get; set; }
    }
}
