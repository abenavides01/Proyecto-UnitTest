using Moq;
using NUnit.Framework;
using Project1.Core.Domain;
using Project1.Core.Interfaces;
using Project1.Core.Services;

namespace Project1.Tests.Services;

public class RedemptionRulesServiceTests
{
    [Test]
    public void DefineOrUpdateRule_CreatesNew_WhenNotExists()
    {
        var benefitId = Guid.NewGuid();
        var repo = new Mock<IRedemptionRuleRepository>();

        repo.Setup(r => r.GetByBenefitId(benefitId)).Returns((RedemptionRule?)null);

        var service = new RedemptionRulesService(repo.Object);

        var rule = service.DefineOrUpdateRule(benefitId, 2, true);

        Assert.That(rule.BenefitId, Is.EqualTo(benefitId));
        Assert.That(rule.MaxRedemptionsPerDay, Is.EqualTo(2));
        repo.Verify(r => r.Add(It.IsAny<RedemptionRule>()), Times.Once);
    }

    [Test]
    public void DefineOrUpdateRule_Throws_WhenBenefitIdEmpty()
    {
        var repo = new Mock<IRedemptionRuleRepository>();
        var service = new RedemptionRulesService(repo.Object);

        Assert.Throws<ArgumentException>(() => service.DefineOrUpdateRule(Guid.Empty, 1));
    }
}