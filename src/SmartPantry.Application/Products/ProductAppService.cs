using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

public class ProductAppService :
    CrudAppService<Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateProductDto, UpdateProductDto>,
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository) : base(repository)
    {
    }

    // ADAPTADO: Product se crea con su constructor (valida) y Barcode es un Value Object
    public override async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        await CheckCreatePolicyAsync();
        var product = new Product(GuidGenerator.Create(), new Barcode(input.Barcode), input.Name, input.Brand);
        await Repository.InsertAsync(product);
        return MapToGetOutputDto(product);
    }

    // ADAPTADO: la regla de modificación vive en el dominio (Product.Update)
    public override async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input)
    {
        await CheckUpdatePolicyAsync();
        var product = await GetEntityByIdAsync(id);
        product.Update(input.Name, input.Brand);
        await Repository.UpdateAsync(product);
        return MapToGetOutputDto(product);
    }

    // DESACTIVADO: Product no se elimina (PantryItem lo referenciará por productId)
    [RemoteService(false)]
    public override Task DeleteAsync(Guid id)
    {
        throw new NotSupportedException("Product no se elimina.");
    }

    // ADAPTADO: mapeo a mano, porque Barcode es un Value Object y el DTO usa su Code
    protected override ProductDto MapToGetOutputDto(Product entity)
    {
        return new ProductDto
        {
            Id = entity.Id,
            Barcode = entity.Barcode.Code,
            Name = entity.Name,
            Brand = entity.Brand
        };
    }

    protected override ProductDto MapToGetListOutputDto(Product entity)
    {
        return MapToGetOutputDto(entity);
    }
}