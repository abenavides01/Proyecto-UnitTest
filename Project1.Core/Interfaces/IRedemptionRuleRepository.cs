using Project1.Core.Domain;

namespace Project1.Core.Interfaces;

public interface IRedemptionRuleRepository
{
    RedemptionRule? GetByBenefitId(Guid benefitId);
    void Add(RedemptionRule rule);
    void Update(RedemptionRule rule);
}