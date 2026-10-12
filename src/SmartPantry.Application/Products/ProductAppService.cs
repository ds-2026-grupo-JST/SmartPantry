using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

public class ProductAppService : SmartPantryAppService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductAppService(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        var product = new Product(
            GuidGenerator.Create(),
            new Barcode(input.Barcode),
            input.Name,
            input.Brand);

        await _productRepository.InsertAsync(product);

        return ToDto(product);
    }

    public async Task<ProductDto> GetAsync(Guid id)
    {
        var product = await _productRepository.GetAsync(id);
        return ToDto(product);
    }

    private static ProductDto ToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Barcode = product.Barcode.Code,
            Name = product.Name,
            Brand = product.Brand
        };
    }
}