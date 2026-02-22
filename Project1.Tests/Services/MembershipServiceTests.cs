using Moq;
using NUnit.Framework;
using Project1.Core.Domain;
using Project1.Core.Interfaces;
using Project1.Core.Services;

namespace Project1.Tests.Services;

[TestFixture]
public class MembershipServiceTests
{
    private Mock<IMembershipRepository> _repoMock = null!;
    private MembershipService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IMembershipRepository>();
        _service = new MembershipService(_repoMock.Object);
    }

    // ===== Story 1: Register membership =====

    [Test]
    public void RegisterMembership_WhenUserIsValidAndNoExistingMembership_CreatesAndSavesMembership()
    {
        // Arrange
        var userId = "user-123";
        _repoMock.Setup(r => r.GetByUserId(userId)).Returns((Membership?)null);

        // Act
        var result = _service.RegisterMembership(userId);

        // Assert
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.Status, Is.EqualTo(MembershipStatus.Active));
        Assert.That(result.CreatedAtUtc, Is.Not.EqualTo(default(DateTime)));

        _repoMock.Verify(r => r.Add(It.Is<Membership>(m =>
            m.UserId == userId &&
            m.Status == MembershipStatus.Active
        )), Times.Once);
    }

    [Test]
    public void RegisterMembership_WhenUserIdIsWhitespace_ThrowsArgumentException()
    {
        Assert.That(() => _service.RegisterMembership("   "),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void RegisterMembership_WhenMembershipAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = "user-123";
        _repoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new Membership { Id = Guid.NewGuid(), UserId = userId, Status = MembershipStatus.Active });

        // Act + Assert
        Assert.That(() => _service.RegisterMembership(userId),
            Throws.TypeOf<InvalidOperationException>());

        _repoMock.Verify(r => r.Add(It.IsAny<Membership>()), Times.Never);
    }

    // ===== Story 2: Change status =====

    [Test]
    public void ChangeStatus_WhenMembershipExists_UpdatesStatusAndSaves()
    {
        // Arrange
        var id = Guid.NewGuid();
        var membership = new Membership { Id = id, UserId = "u1", Status = MembershipStatus.Active };

        _repoMock.Setup(r => r.GetById(id)).Returns(membership);

        // Act
        _service.ChangeStatus(id, MembershipStatus.Inactive);

        // Assert
        Assert.That(membership.Status, Is.EqualTo(MembershipStatus.Inactive));
        Assert.That(membership.StatusChangedAtUtc, Is.Not.Null);

        _repoMock.Verify(r => r.Update(membership), Times.Once);
    }

    [Test]
    public void ChangeStatus_WhenMembershipDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetById(id)).Returns((Membership?)null);

        // Act + Assert
        Assert.That(() => _service.ChangeStatus(id, MembershipStatus.Inactive),
            Throws.TypeOf<KeyNotFoundException>());

        _repoMock.Verify(r => r.Update(It.IsAny<Membership>()), Times.Never);
    }

    [Test]
    public void ChangeStatus_WhenSameStatusRequested_DoesNotUpdateOrSave()
    {
        // Arrange
        var id = Guid.NewGuid();
        var membership = new Membership { Id = id, UserId = "u1", Status = MembershipStatus.Active };

        _repoMock.Setup(r => r.GetById(id)).Returns(membership);

        // Act
        _service.ChangeStatus(id, MembershipStatus.Active);

        // Assert
        _repoMock.Verify(r => r.Update(It.IsAny<Membership>()), Times.Never);
    }

    // ===== Story 3: Get status by user =====

    [Test]
    public void GetStatusByUser_WhenMembershipExists_ReturnsStatus()
    {
        // Arrange
        var userId = "user-1";
        _repoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new Membership { UserId = userId, Status = MembershipStatus.Inactive });

        // Act
        var status = _service.GetStatusByUser(userId);

        // Assert
        Assert.That(status, Is.EqualTo(MembershipStatus.Inactive));
    }

    [Test]
    public void GetStatusByUser_WhenMembershipDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = "missing-user";
        _repoMock.Setup(r => r.GetByUserId(userId)).Returns((Membership?)null);

        // Act + Assert
        Assert.That(() => _service.GetStatusByUser(userId),
            Throws.TypeOf<KeyNotFoundException>());
    }

    // ===== Helper: IsActiveByUser =====

    [Test]
    public void IsActiveByUser_WhenActive_ReturnsTrue()
    {
        // Arrange
        var userId = "user-2";
        _repoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new Membership { UserId = userId, Status = MembershipStatus.Active });

        // Act
        var result = _service.IsActiveByUser(userId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsActiveByUser_WhenInactive_ReturnsFalse()
    {
        // Arrange
        var userId = "user-3";
        _repoMock.Setup(r => r.GetByUserId(userId))
            .Returns(new Membership { UserId = userId, Status = MembershipStatus.Inactive });

        // Act
        var result = _service.IsActiveByUser(userId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsActiveByUser_WhenMembershipDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = "missing-user";
        _repoMock.Setup(r => r.GetByUserId(userId)).Returns((Membership?)null);

        // Act + Assert
        Assert.That(() => _service.IsActiveByUser(userId),
            Throws.TypeOf<KeyNotFoundException>());
    }
}