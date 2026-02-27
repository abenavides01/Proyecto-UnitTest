using Project1.Core.Domain;
using Project1.Core.Domain.Reports;
using Project1.Core.Interfaces;

namespace Project1.Core.Services;

public class ReportsService
{
    private readonly IMembershipQueryRepository _membershipQuery;
    private readonly IPointsMovementQueryRepository _movementQuery;

    public ReportsService(IMembershipQueryRepository membershipQuery,
                          IPointsMovementQueryRepository movementQuery)
    {
        _membershipQuery = membershipQuery;
        _movementQuery = movementQuery;
    }

    public MembershipStatusReport GetMembershipStatusReport()
    {
        var memberships = _membershipQuery.GetAll();

        return new MembershipStatusReport
        {
            ActiveCount = memberships.Count(m => m.Status == MembershipStatus.Active),
            InactiveCount = memberships.Count(m => m.Status == MembershipStatus.Inactive)
        };
    }

    public PointsSummaryReport GetPointsSummaryReport(DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var movements = _movementQuery.GetAll();

        if (fromUtc.HasValue)
            movements = movements.Where(m => m.CreatedAtUtc >= fromUtc.Value).ToList();

        if (toUtc.HasValue)
            movements = movements.Where(m => m.CreatedAtUtc <= toUtc.Value).ToList();

        return new PointsSummaryReport
        {
            EarnedTotal = movements.Where(m => m.Type == PointsMovementType.Earned).Sum(m => m.Points),
            RedeemedTotal = movements.Where(m => m.Type == PointsMovementType.Redeemed).Sum(m => m.Points)
        };
    }
}