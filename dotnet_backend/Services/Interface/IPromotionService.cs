using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;

public interface IPromotionService
{
    Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync();
    Task<PromotionDto> GetPromotionByIdAsync(int id);
    Task<PromotionDto> CreatePromotionAsync(PromotionDto dto);
    Task<PromotionDto> UpdatePromotionAsync(int id, PromotionDto dto);
    Task<bool> DeletePromotionAsync(int id);
    Task<ApplyPromoResponseDto> ApplyPromotionAsync(ApplyPromoRequestDto request);
}