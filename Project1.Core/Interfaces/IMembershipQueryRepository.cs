using Project1.Core.Domain;

namespace Project1.Core.Interfaces;

public interface IMembershipQueryRepository
{
    IReadOnlyList<Membership> GetAll();
}