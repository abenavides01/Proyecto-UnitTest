using Moq;
using NUnit.Framework;
using Project1.Core.Domain;
using Project1.Core.Interfaces;
using Project1.Core.Services;

namespace Project1.Tests.Services;

public class MovementHistoryServiceTests
{
    [Test]
    public void GetHistory_ReturnsFilteredByType()
    {
        var userId = "user1";
        var repo = new Mock<IPointsMovementQueryRepository>();

        repo.Setup(r => r.GetByUserId(userId)).Returns(new List<PointsMovement>
        {
            new() { UserId = userId, Type = PointsMovementType.Earned, Points = 10, CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10) },
            new() { UserId = userId, Type = PointsMovementType.Redeemed, Points = 5, CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5) }
        });

        var service = new MovementHistoryService(repo.Object);

        var result = service.GetHistory(userId, type: PointsMovementType.Redeemed);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Type, Is.EqualTo(PointsMovementType.Redeemed));
    }

    [Test]
    public void GetHistory_Throws_WhenUserIdIsEmpty()
    {
        var repo = new Mock<IPointsMovementQueryRepository>();
        var service = new MovementHistoryService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.GetHistory("   "));
    }
}