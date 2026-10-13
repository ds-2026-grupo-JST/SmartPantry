using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartPantry.Products;

public interface IProductAppService : IApplicationService
{
    Task<ProductDto> CreateAsync(CreateProductDto input);
    Task<ProductDto> GetAsync(Guid id);
    Task<PagedResultDto<ProductDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input);
}