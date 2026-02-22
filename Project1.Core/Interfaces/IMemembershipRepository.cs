using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1.Core.Domain;

namespace Project1.Core.Interfaces
{
    public interface IMemembershipRepository
    {
        Membership? GetById(Guid id);
        Membership? GetByUserId(string userId);
        void Add(Membership membership);
        void Update(Membership membership);
    }
}
