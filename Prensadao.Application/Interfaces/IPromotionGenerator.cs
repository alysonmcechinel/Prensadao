using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces;

public interface IPromotionGenerator
{
    Task<PromotionProductDto> GeneratePromotionAsync(GeneratePromotionRequest request, CancellationToken cancellationToken);
}
