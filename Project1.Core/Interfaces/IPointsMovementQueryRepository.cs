using Project1.Core.Domain;

namespace Project1.Core.Interfaces;

public interface IPointsMovementQueryRepository
{
    IReadOnlyList<PointsMovement> GetByUserId(string userId);
    IReadOnlyList<PointsMovement> GetAll();
}