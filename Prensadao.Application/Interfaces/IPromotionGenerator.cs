using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces;

public interface IPromotionGenerator
{
    Task<PromotionProductDto> GenerateWeeklyPromotionAsync(GeneratePromotionRequest request, CancellationToken cancellationToken);
}