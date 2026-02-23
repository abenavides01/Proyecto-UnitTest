using Moq;
using NUnit.Framework;
using Project1.Core.Domain;
using Project1.Core.Interfaces;
using Project1.Core.Services;

namespace Project1.Tests.Services;

[TestFixture]
public class PointsServiceTests
{
    private Mock<IMembershipRepository> _membershipRepoMock = null!;
    private Mock<IPointsAccountRepository> _accountRepoMock = null!;
    private Mock<IPointsMovementRepository> _movementRepoMock = null!;
    private Mock<IAccumulationRuleRepository> _ruleRepoMock = null!;
    private Mock<IBenefitRepository> _benefitRepoMock = null!;
    private PointsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _membershipRepoMock = new Mock<IMembershipRepository>();
        _accountRepoMock = new Mock<IPointsAccountRepository>();
        _movementRepoMock = new Mock<IPointsMovementRepository>();
        _ruleRepoMock = new Mock<IAccumulationRuleRepository>();
        _benefitRepoMock = new Mock<IBenefitRepository>();

        _service = new PointsService(
            _membershipRepoMock.Object,
            _accountRepoMock.Object,
            _movementRepoMock.Object,
            _ruleRepoMock.Object,
            _benefitRepoMock.Object);
    }

    // ===== HU 6: DefineOrUpdateRule =====

    [Test]
    public void DefineOrUpdateRule_WhenNoExistingRule_CreatesAndSavesRule()
    {
        // Arrange
        var serviceCode = "GYM_VISIT";
        _ruleRepoMock.Setup(r => r.GetByServiceCode(serviceCode)).Returns((AccumulationRule?)null);

        // Act
        var rule = _service.DefineOrUpdateRule(serviceCode, 10, true);

        // Assert
        Assert.That(rule.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(rule.ServiceCode, Is.EqualTo(serviceCode));
        Assert.That(rule.PointsPerUse, Is.EqualTo(10));
        Assert.That(rule.IsActive, Is.True);

        _ruleRepoMock.Verify(r => r.Add(It.IsAny<AccumulationRule>()), Times.Once);
        _ruleRepoMock.Verify(r => r.Update(It.IsAny<AccumulationRule>()), Times.Never);
    }

    [Test]
    public void DefineOrUpdateRule_WhenPointsPerUseIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        Assert.That(() => _service.DefineOrUpdateRule("X", 0),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    // ===== HU 4: AccumulatePoints =====

    [Test]
    public void AccumulatePoints_WhenMembershipActiveAndRuleActive_AddsPointsAndRegistersMovement()
    {
        // Arrange
        var userId = "user-1";
        var serviceCode = "GYM_VISIT";

        _membershipRepoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new Membership { UserId = userId, Status = MembershipStatus.Active });

        _ruleRepoMock.Setup(r => r.GetByServiceCode(serviceCode))
            .Returns(new AccumulationRule { ServiceCode = serviceCode, PointsPerUse = 10, IsActive = true });

        _accountRepoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new PointsAccount { Id = Guid.NewGuid(), UserId = userId, Balance = 5 });

        // Act
        var newBalance = _service.AccumulatePoints(userId, serviceCode);

        // Assert
        Assert.That(newBalance, Is.EqualTo(15));

        _accountRepoMock.Verify(r => r.Update(It.Is<PointsAccount>(a => a.Balance == 15)), Times.Once);
        _movementRepoMock.Verify(r => r.Add(It.Is<PointsMovement>(m =>
            m.UserId == userId &&
            m.Type == PointsMovementType.Earned &&
            m.Points == 10 &&
            m.Reference.Contains(serviceCode)
        )), Times.Once);
    }

    [Test]
    public void AccumulatePoints_WhenNoActiveRule_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = "user-2";
        var serviceCode = "UNKNOWN";

        _membershipRepoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new Membership { UserId = userId, Status = MembershipStatus.Active });

        _ruleRepoMock.Setup(r => r.GetByServiceCode(serviceCode))
            .Returns((AccumulationRule?)null);

        // Act + Assert
        Assert.That(() => _service.AccumulatePoints(userId, serviceCode),
            Throws.TypeOf<InvalidOperationException>());

        _movementRepoMock.Verify(r => r.Add(It.IsAny<PointsMovement>()), Times.Never);
    }

    // ===== HU 5: RedeemPoints =====

    [Test]
    public void RedeemPoints_WhenEnoughBalance_DeductsPointsAndRegistersMovement()
    {
        // Arrange
        var userId = "user-3";
        var benefitId = Guid.NewGuid();

        _membershipRepoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new Membership { UserId = userId, Status = MembershipStatus.Active });

        _benefitRepoMock.Setup(r => r.GetById(benefitId))
            .Returns(new Benefit { Id = benefitId, Name = "Free Coffee", PointsCost = 20, IsActive = true });

        _accountRepoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new PointsAccount { Id = Guid.NewGuid(), UserId = userId, Balance = 30 });

        // Act
        var newBalance = _service.RedeemPoints(userId, benefitId);

        // Assert
        Assert.That(newBalance, Is.EqualTo(10));

        _accountRepoMock.Verify(r => r.Update(It.Is<PointsAccount>(a => a.Balance == 10)), Times.Once);
        _movementRepoMock.Verify(r => r.Add(It.Is<PointsMovement>(m =>
            m.UserId == userId &&
            m.Type == PointsMovementType.Redeemed &&
            m.Points == 20 &&
            m.Reference.Contains("BENEFIT")
        )), Times.Once);
    }

    [Test]
    public void RedeemPoints_WhenInsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = "user-4";
        var benefitId = Guid.NewGuid();

        _membershipRepoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new Membership { UserId = userId, Status = MembershipStatus.Active });

        _benefitRepoMock.Setup(r => r.GetById(benefitId))
            .Returns(new Benefit { Id = benefitId, Name = "Discount", PointsCost = 50, IsActive = true });

        _accountRepoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new PointsAccount { Id = Guid.NewGuid(), UserId = userId, Balance = 10 });

        // Act + Assert
        Assert.That(() => _service.RedeemPoints(userId, benefitId),
            Throws.TypeOf<InvalidOperationException>());

        _movementRepoMock.Verify(r => r.Add(It.IsAny<PointsMovement>()), Times.Never);
    }
}
