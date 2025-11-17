using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Application.Interfaces.Services
{
    public interface IPromotionService
    {
        Task<Promotion?> GetPromotionByIdAsync(int id);
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
        Task<IEnumerable<Promotion>> GetActivePromotionsByGameIdAsync(int gameId);
        Task<Promotion> CreatePromotionAsync(Promotion promotion);
        Task<Promotion> UpdatePromotionAsync(Promotion promotion);
        Task DeletePromotionAsync(int id);
        Task<decimal> GetDiscountedPriceAsync(int gameId);
        Task<Promotion?> GetBestPromotionForGameAsync(int gameId);
    }
}
