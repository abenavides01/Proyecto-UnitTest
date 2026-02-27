using Project1.Core.Domain;
using Project1.Core.Interfaces;

namespace Project1.Core.Services;

public class RedemptionService
{
    private readonly IPointsRedemptionPort _redemptionPort;
    private readonly IRedemptionRuleRepository _rulesRepo;
    private readonly IPointsMovementQueryRepository _movementQuery;

    public RedemptionService(
        IPointsRedemptionPort redemptionPort,
        IRedemptionRuleRepository rulesRepo,
        IPointsMovementQueryRepository movementQuery)
    {
        _redemptionPort = redemptionPort;
        _rulesRepo = rulesRepo;
        _movementQuery = movementQuery;
    }

    public int RedeemWithRules(string userId, Guid benefitId, DateTime? nowUtc = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (benefitId == Guid.Empty)
            throw new ArgumentException("BenefitId is required.", nameof(benefitId));

        userId = userId.Trim();
        var now = nowUtc ?? DateTime.UtcNow;

        var rule = _rulesRepo.GetByBenefitId(benefitId);
        if (rule != null && rule.IsActive)
        {
            var from = now.Date;
            var to = from.AddDays(1);

            var redeemedToday = _movementQuery.GetByUserId(userId)
                .Count(m =>
                    m.Type == PointsMovementType.Redeemed &&
                    m.CreatedAtUtc >= from &&
                    m.CreatedAtUtc < to &&
                    m.Reference == $"BENEFIT:{benefitId}");

            if (redeemedToday >= rule.MaxRedemptionsPerDay)
                throw new InvalidOperationException("Daily redemption limit reached for this benefit.");
        }

        return _redemptionPort.RedeemPoints(userId, benefitId);
    }
}