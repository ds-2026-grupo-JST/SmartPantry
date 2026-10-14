using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
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

    [Fact]
    public async Task Should_List_Products_Paged_And_Sorted_By_Name()
    {
        await _productAppService.CreateAsync(new CreateProductDto { Barcode = "7790000000001", Name = "Leche" });
        await _productAppService.CreateAsync(new CreateProductDto { Barcode = "7790000000002", Name = "Arroz" });
        await _productAppService.CreateAsync(new CreateProductDto { Barcode = "7790000000003", Name = "Yogur" });

        var page = await _productAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            SkipCount = 0,
            MaxResultCount = 2,
            Sorting = "Name"
        });

        page.TotalCount.ShouldBe(3);
        page.Items.Count.ShouldBe(2);
        page.Items[0].Name.ShouldBe("Arroz");
        page.Items[1].Name.ShouldBe("Leche");
    }

    [Fact]
    public async Task Should_Update_Product_And_Keep_Barcode()
    {
        var created = await _productAppService.CreateAsync(new CreateProductDto
        {
            Barcode = "7790000000001",
            Name = "Leche",
            Brand = "Marca A"
        });

        var updated = await _productAppService.UpdateAsync(created.Id, new UpdateProductDto
        {
            Name = "  Yogur  ",
            Brand = "  Marca B  "
        });

        updated.Name.ShouldBe("Yogur");
        updated.Brand.ShouldBe("Marca B");

        var found = await _productAppService.GetAsync(created.Id);

        found.Name.ShouldBe("Yogur");
        found.Brand.ShouldBe("Marca B");
        found.Barcode.ShouldBe("7790000000001");
    }

    [Fact]
    public async Task Should_Not_Update_Product_With_Invalid_Data()
    {
        var created = await _productAppService.CreateAsync(new CreateProductDto
        {
            Barcode = "7790000000001",
            Name = "Leche",
            Brand = "Marca A"
        });

        await Assert.ThrowsAsync<AbpValidationException>(async () =>
            await _productAppService.UpdateAsync(created.Id, new UpdateProductDto
            {
                Name = "",
                Brand = "Marca B"
            }));

        var found = await _productAppService.GetAsync(created.Id);

        found.Name.ShouldBe("Leche");
        found.Brand.ShouldBe("Marca A");
    }

    [Fact]
    public async Task Should_Throw_NotFound_When_Updating_Unknown_Id()
    {
        await Assert.ThrowsAnyAsync<EntityNotFoundException>(async () =>
            await _productAppService.UpdateAsync(Guid.NewGuid(), new UpdateProductDto
            {
                Name = "Yogur"
            }));
    }
}