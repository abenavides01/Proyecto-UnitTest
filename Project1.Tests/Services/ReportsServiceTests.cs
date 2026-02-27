using Moq;
using NUnit.Framework;
using Project1.Core.Domain;
using Project1.Core.Interfaces;
using Project1.Core.Services;

namespace Project1.Tests.Services;

public class ReportsServiceTests
{
    [Test]
    public void GetMembershipStatusReport_CountsActiveAndInactive()
    {
        var membershipRepo = new Mock<IMembershipQueryRepository>();
        var movementRepo = new Mock<IPointsMovementQueryRepository>();

        membershipRepo.Setup(r => r.GetAll()).Returns(new List<Membership>
        {
            new() { Status = MembershipStatus.Active },
            new() { Status = MembershipStatus.Active },
            new() { Status = MembershipStatus.Inactive },
        });

        movementRepo.Setup(r => r.GetAll()).Returns(new List<PointsMovement>());

        var service = new ReportsService(membershipRepo.Object, movementRepo.Object);

        var report = service.GetMembershipStatusReport();

        Assert.That(report.ActiveCount, Is.EqualTo(2));
        Assert.That(report.InactiveCount, Is.EqualTo(1));
    }

    [Test]
    public void GetPointsSummaryReport_SumsEarnedAndRedeemed()
    {
        var membershipRepo = new Mock<IMembershipQueryRepository>();
        var movementRepo = new Mock<IPointsMovementQueryRepository>();

        membershipRepo.Setup(r => r.GetAll()).Returns(new List<Membership>());

        movementRepo.Setup(r => r.GetAll()).Returns(new List<PointsMovement>
        {
            new() { Type = PointsMovementType.Earned, Points = 10, CreatedAtUtc = DateTime.UtcNow.AddDays(-1) },
            new() { Type = PointsMovementType.Earned, Points = 5, CreatedAtUtc = DateTime.UtcNow.AddDays(-1) },
            new() { Type = PointsMovementType.Redeemed, Points = 7, CreatedAtUtc = DateTime.UtcNow.AddDays(-1) }
        });

        var service = new ReportsService(membershipRepo.Object, movementRepo.Object);

        var report = service.GetPointsSummaryReport();

        Assert.That(report.EarnedTotal, Is.EqualTo(15));
        Assert.That(report.RedeemedTotal, Is.EqualTo(7));
    }
}