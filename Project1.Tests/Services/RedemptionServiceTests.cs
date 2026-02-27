using Moq;
using NUnit.Framework;
using Project1.Core.Domain;
using Project1.Core.Interfaces;
using Project1.Core.Services;

namespace Project1.Tests.Services;

public class RedemptionServiceTests
{
    [Test]
    public void RedeemWithRules_Throws_WhenDailyLimitReached()
    {
        var userId = "user1";
        var benefitId = Guid.NewGuid();
        var now = new DateTime(2026, 2, 26, 10, 0, 0, DateTimeKind.Utc);

        var ruleRepo = new Mock<IRedemptionRuleRepository>();
        ruleRepo.Setup(r => r.GetByBenefitId(benefitId))
            .Returns(new RedemptionRule
            {
                Id = Guid.NewGuid(),
                BenefitId = benefitId,
                MaxRedemptionsPerDay = 1,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            });

        var movementRepo = new Mock<IPointsMovementQueryRepository>();
        movementRepo.Setup(r => r.GetByUserId(userId))
            .Returns(new List<PointsMovement>
            {
                new() { UserId = userId, Type = PointsMovementType.Redeemed, Points = 10, Reference = $"BENEFIT:{benefitId}", CreatedAtUtc = now.AddHours(-1) }
            });

        var port = new Mock<IPointsRedemptionPort>();

        var service = new RedemptionService(port.Object, ruleRepo.Object, movementRepo.Object);

        Assert.Throws<InvalidOperationException>(() => service.RedeemWithRules(userId, benefitId, now));
        port.Verify(p => p.RedeemPoints(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void RedeemWithRules_CallsPort_WhenUnderLimit()
    {
        var userId = "user1";
        var benefitId = Guid.NewGuid();
        var now = new DateTime(2026, 2, 26, 10, 0, 0, DateTimeKind.Utc);

        var ruleRepo = new Mock<IRedemptionRuleRepository>();
        ruleRepo.Setup(r => r.GetByBenefitId(benefitId))
            .Returns(new RedemptionRule
            {
                Id = Guid.NewGuid(),
                BenefitId = benefitId,
                MaxRedemptionsPerDay = 2,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            });

        var movementRepo = new Mock<IPointsMovementQueryRepository>();
        movementRepo.Setup(r => r.GetByUserId(userId))
            .Returns(new List<PointsMovement>
            {
                new() { UserId = userId, Type = PointsMovementType.Redeemed, Points = 10, Reference = $"BENEFIT:{benefitId}", CreatedAtUtc = now.AddHours(-1) }
            });

        var port = new Mock<IPointsRedemptionPort>();
        port.Setup(p => p.RedeemPoints(userId, benefitId)).Returns(999);

        var service = new RedemptionService(port.Object, ruleRepo.Object, movementRepo.Object);

        var result = service.RedeemWithRules(userId, benefitId, now);

        Assert.That(result, Is.EqualTo(999));
        port.Verify(p => p.RedeemPoints(userId, benefitId), Times.Once);
    }
}