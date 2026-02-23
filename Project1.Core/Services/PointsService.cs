using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1.Core.Domain;
using Project1.Core.Interfaces;

namespace Project1.Core.Services;

public class PointsService
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPointsAccountRepository _pointsAccountRepository;
    private readonly IPointsMovementRepository _movementRepository;
    private readonly IAccumulationRuleRepository _ruleRepository;
    private readonly IBenefitRepository _benefitRepository;

    public PointsService(
        IMembershipRepository membershipRepository,
        IPointsAccountRepository pointsAccountRepository,
        IPointsMovementRepository movementRepository,
        IAccumulationRuleRepository ruleRepository,
        IBenefitRepository benefitRepository)
    {
        _membershipRepository = membershipRepository;
        _pointsAccountRepository = pointsAccountRepository;
        _movementRepository = movementRepository;
        _ruleRepository = ruleRepository;
        _benefitRepository = benefitRepository;
    }

    // HU 6: Definir reglas de acumulación
    public AccumulationRule DefineOrUpdateRule(string serviceCode, int pointsPerUse, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(serviceCode))
            throw new ArgumentException("ServiceCode is required.", nameof(serviceCode));

        if (pointsPerUse <= 0)
            throw new ArgumentOutOfRangeException(nameof(pointsPerUse), "PointsPerUse must be greater than 0.");

        serviceCode = serviceCode.Trim();

        var existing = _ruleRepository.GetByServiceCode(serviceCode);
        if (existing == null)
        {
            var rule = new AccumulationRule
            {
                Id = Guid.NewGuid(),
                ServiceCode = serviceCode,
                PointsPerUse = pointsPerUse,
                IsActive = isActive,
                CreatedAtUtc = DateTime.UtcNow
            };

            _ruleRepository.Add(rule);
            return rule;
        }

        existing.PointsPerUse = pointsPerUse;
        existing.IsActive = isActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        _ruleRepository.Update(existing);
        return existing;
    }

    // HU 4: Acumular puntos por uso del servicio
    public int AccumulatePoints(string userId, string serviceCode)
    {
        userId = NormalizeUserId(userId);
        serviceCode = NormalizeServiceCode(serviceCode);

        EnsureMembershipActive(userId);

        var rule = _ruleRepository.GetByServiceCode(serviceCode);
        if (rule == null || !rule.IsActive)
            throw new InvalidOperationException("No active accumulation rule exists for this service.");

        var pointsToAdd = rule.PointsPerUse;

        var account = _pointsAccountRepository.GetByUserId(userId);
        if (account == null)
        {
            account = new PointsAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _pointsAccountRepository.Add(account);
        }

        account.Balance += pointsToAdd;
        account.UpdatedAtUtc = DateTime.UtcNow;
        _pointsAccountRepository.Update(account);

        _movementRepository.Add(new PointsMovement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PointsMovementType.Earned,
            Points = pointsToAdd,
            Reference = $"SERVICE:{serviceCode}",
            CreatedAtUtc = DateTime.UtcNow
        });

        return account.Balance;
    }

    // HU 5: Redimir puntos por beneficios disponibles
    public int RedeemPoints(string userId, Guid benefitId)
    {
        userId = NormalizeUserId(userId);
        EnsureMembershipActive(userId);

        var benefit = _benefitRepository.GetById(benefitId);
        if (benefit == null)
            throw new KeyNotFoundException("Benefit not found.");

        if (!benefit.IsActive)
            throw new InvalidOperationException("Benefit is not available.");

        if (benefit.PointsCost <= 0)
            throw new InvalidOperationException("Benefit cost is invalid.");

        var account = _pointsAccountRepository.GetByUserId(userId);
        if (account == null)
            throw new InvalidOperationException("Points account not found.");

        if (account.Balance < benefit.PointsCost)
            throw new InvalidOperationException("Insufficient points balance.");

        account.Balance -= benefit.PointsCost;
        account.UpdatedAtUtc = DateTime.UtcNow;
        _pointsAccountRepository.Update(account);

        _movementRepository.Add(new PointsMovement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PointsMovementType.Redeemed,
            Points = benefit.PointsCost,
            Reference = $"BENEFIT:{benefit.Id}",
            CreatedAtUtc = DateTime.UtcNow
        });

        return account.Balance;
    }

    private void EnsureMembershipActive(string userId)
    {
        var membership = _membershipRepository.GetByUserId(userId);
        if (membership == null)
            throw new KeyNotFoundException("Membership not found for the user.");

        if (membership.Status != MembershipStatus.Active)
            throw new InvalidOperationException("Membership is not active.");
    }

    private static string NormalizeUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));
        return userId.Trim();
    }

    private static string NormalizeServiceCode(string serviceCode)
    {
        if (string.IsNullOrWhiteSpace(serviceCode))
            throw new ArgumentException("ServiceCode is required.", nameof(serviceCode));
        return serviceCode.Trim();
    }
}
