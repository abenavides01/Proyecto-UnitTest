using Project1.Core.Interfaces;

namespace Project1.Core.Services;

public class PointsRedemptionPort : IPointsRedemptionPort
{
    private readonly PointsService _pointsService;

    public PointsRedemptionPort(PointsService pointsService)
    {
        _pointsService = pointsService;
    }

    public int RedeemPoints(string userId, Guid benefitId)
        => _pointsService.RedeemPoints(userId, benefitId);
}