namespace Project1.Core.Interfaces;

public interface IPointsRedemptionPort
{
    int RedeemPoints(string userId, Guid benefitId);
}