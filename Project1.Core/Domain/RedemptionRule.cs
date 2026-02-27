namespace Project1.Core.Domain;

public class RedemptionRule
{
    public Guid Id { get; set; }
    public Guid BenefitId { get; set; }
    public int MaxRedemptionsPerDay { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}