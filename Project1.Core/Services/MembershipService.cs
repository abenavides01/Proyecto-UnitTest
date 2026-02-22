using Project1.Core.Domain;
using Project1.Core.Interfaces;
using System;

namespace Project1.Core.Services;

public class MembershipService
{
    private readonly IMembershipRepository _repository;

    public MembershipService(IMembershipRepository repository)
    {
        _repository = repository;
    }

    // Story 1: Admin registers new memberships
    public Membership RegisterMembership(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        userId = userId.Trim();

        var existing = _repository.GetByUserId(userId);
        if (existing != null)
            throw new InvalidOperationException("A membership already exists for this user.");

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = MembershipStatus.Active, // default assumption
            CreatedAtUtc = DateTime.UtcNow,
            StatusChangedAtUtc = null
        };

        _repository.Add(membership);
        return membership;
    }

    // Story 2: Admin activates or deactivates membership
    public void ChangeStatus(Guid membershipId, MembershipStatus newStatus)
    {
        var membership = _repository.GetById(membershipId);
        if (membership == null)
            throw new KeyNotFoundException("Membership not found.");

        // Idempotent behavior: if same status requested, do nothing
        if (membership.Status == newStatus)
            return;

        membership.Status = newStatus;
        membership.StatusChangedAtUtc = DateTime.UtcNow;

        _repository.Update(membership);
    }

    // Story 3: User checks membership status
    public MembershipStatus GetStatusByUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        userId = userId.Trim();

        var membership = _repository.GetByUserId(userId);
        if (membership == null)
            throw new KeyNotFoundException("Membership not found for the user.");

        return membership.Status;
    }

    // Helper for other modules (points/redemption)
    public bool IsActiveByUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        userId = userId.Trim();

        var membership = _repository.GetByUserId(userId);
        if (membership == null)
            throw new KeyNotFoundException("Membership not found for the user.");

        return membership.Status == MembershipStatus.Active;
    }
}