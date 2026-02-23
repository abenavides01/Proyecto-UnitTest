using Project1.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.Interfaces
{
    public interface IPointsAccountRepository
    {
        PointsAccount? GetByUserId(string userId);
        void Add(PointsAccount account);
        void Update(PointsAccount account);
    }
}
