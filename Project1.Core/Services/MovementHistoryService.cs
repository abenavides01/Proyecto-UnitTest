using Project1.Core.Domain;
using Project1.Core.Interfaces;

namespace Project1.Core.Services;

public class MovementHistoryService
{
    private readonly IPointsMovementQueryRepository _queryRepo;

    public MovementHistoryService(IPointsMovementQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public IReadOnlyList<PointsMovement> GetHistory(
        string userId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        PointsMovementType? type = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        userId = userId.Trim();
        var movements = _queryRepo.GetByUserId(userId);

        if (fromUtc.HasValue)
            movements = movements.Where(m => m.CreatedAtUtc >= fromUtc.Value).ToList();

        if (toUtc.HasValue)
            movements = movements.Where(m => m.CreatedAtUtc <= toUtc.Value).ToList();

        if (type.HasValue)
            movements = movements.Where(m => m.Type == type.Value).ToList();

        return movements.OrderByDescending(m => m.CreatedAtUtc).ToList();
    }
}