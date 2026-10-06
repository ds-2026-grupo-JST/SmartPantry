using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Products;

public abstract class ProductAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProductAppService _productAppService;

    protected ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
    }

    [Fact]
    public async Task Should_Create_And_Then_Get_Product_By_Id()
    {
        var created = await _productAppService.CreateAsync(new CreateProductDto
        {
            Barcode = "  7790000000001  ",
            Name = "  Leche entera  ",
            Brand = "La Serenísima"
        });

        created.Id.ShouldNotBe(Guid.Empty);
        created.Barcode.ShouldBe("7790000000001");
        created.Name.ShouldBe("Leche entera");

        var found = await _productAppService.GetAsync(created.Id);

        found.Id.ShouldBe(created.Id);
        found.Barcode.ShouldBe("7790000000001");
        found.Name.ShouldBe("Leche entera");
        found.Brand.ShouldBe("La Serenísima");
    }

    [Fact]
    public async Task Should_Not_Create_Product_Without_Name()
    {
        await Assert.ThrowsAsync<AbpValidationException>(async () =>
            await _productAppService.CreateAsync(new CreateProductDto
            {
                Barcode = "7790000000001",
                Name = ""
            }));
    }

    [Fact]
    public async Task Should_Throw_NotFound_For_Unknown_Id()
    {
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(async () =>
            await _productAppService.GetAsync(Guid.NewGuid()));
    }
}