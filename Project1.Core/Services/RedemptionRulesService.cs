using Project1.Core.Domain;
using Project1.Core.Interfaces;

namespace Project1.Core.Services;

public class RedemptionRulesService
{
    private readonly IRedemptionRuleRepository _repo;

    public RedemptionRulesService(IRedemptionRuleRepository repo)
    {
        _repo = repo;
    }

    public RedemptionRule DefineOrUpdateRule(Guid benefitId, int maxRedemptionsPerDay, bool isActive = true)
    {
        if (benefitId == Guid.Empty)
            throw new ArgumentException("BenefitId is required.", nameof(benefitId));

        if (maxRedemptionsPerDay <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRedemptionsPerDay), "Must be greater than 0.");

        var existing = _repo.GetByBenefitId(benefitId);
        if (existing == null)
        {
            var rule = new RedemptionRule
            {
                Id = Guid.NewGuid(),
                BenefitId = benefitId,
                MaxRedemptionsPerDay = maxRedemptionsPerDay,
                IsActive = isActive,
                CreatedAtUtc = DateTime.UtcNow
            };

            _repo.Add(rule);
            return rule;
        }

        existing.MaxRedemptionsPerDay = maxRedemptionsPerDay;
        existing.IsActive = isActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        _repo.Update(existing);
        return existing;
    }
}