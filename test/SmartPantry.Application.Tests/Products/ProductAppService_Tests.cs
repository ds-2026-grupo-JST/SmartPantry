using System;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Products;

public abstract class ProductAppService_Tests<TStartupModule>
    : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProductAppService _productAppService;
    private readonly IExternalProductCatalogClient _externalProductCatalogClient;

    protected ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();

        _externalProductCatalogClient =
            GetRequiredService<IExternalProductCatalogClient>();
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
        await _productAppService.CreateAsync(new CreateProductDto
        {
            Barcode = "7790000000001",
            Name = "Leche"
        });

        await _productAppService.CreateAsync(new CreateProductDto
        {
            Barcode = "7790000000002",
            Name = "Arroz"
        });

        await _productAppService.CreateAsync(new CreateProductDto
        {
            Barcode = "7790000000003",
            Name = "Yogur"
        });

        var page = await _productAppService.GetListAsync(
            new PagedAndSortedResultRequestDto
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

        var updated = await _productAppService.UpdateAsync(
            created.Id,
            new UpdateProductDto
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
            await _productAppService.UpdateAsync(
                created.Id,
                new UpdateProductDto
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
            await _productAppService.UpdateAsync(
                Guid.NewGuid(),
                new UpdateProductDto
                {
                    Name = "Yogur"
                }));
    }

    // TP07 - Producto encontrado en el catálogo externo
    [Fact]
    public async Task Should_Find_External_Product_By_Barcode()
    {
        _externalProductCatalogClient
            .GetByBarcodeAsync("3017620422003")
            .Returns(new ExternalProductDto
            {
                Barcode = "3017620422003",
                Name = "Nutella",
                Brand = "Nutella, Ferrero",
                ImageUrl = "https://example.com/nutella.jpg"
            });

        var result = await _productAppService.GetByBarcodeAsync(
            new GetProductByBarcodeInput
            {
                Barcode = "3017620422003"
            });

        result.Status.ShouldBe(ProductLookupStatus.Found);
        result.Barcode.ShouldBe("3017620422003");
        result.Name.ShouldBe("Nutella");
        result.Brand.ShouldBe("Nutella, Ferrero");
        result.ImageUrl.ShouldBe("https://example.com/nutella.jpg");
    }
    // TP07 - Producto inexistente en el catálogo externo
    [Fact]
    public async Task Should_Return_NotFound_When_External_Product_Does_Not_Exist()
    {
        _externalProductCatalogClient
            .GetByBarcodeAsync("0000000000000")
            .Returns((ExternalProductDto?)null);

        var result = await _productAppService.GetByBarcodeAsync(
            new GetProductByBarcodeInput
            {
                Barcode = "0000000000000"
            });

        result.Status.ShouldBe(ProductLookupStatus.NotFound);
        result.Barcode.ShouldBe("0000000000000");
        result.Name.ShouldBeNull();
        result.Brand.ShouldBeNull();
        result.ImageUrl.ShouldBeNull();
    }
    // TP07 - Producto encontrado con datos incompletos
    [Fact]
    public async Task Should_Keep_Missing_External_Data_As_Null()
    {
        _externalProductCatalogClient
            .GetByBarcodeAsync("7790000000001")
            .Returns(new ExternalProductDto
            {
                Barcode = "7790000000001",
                Name = "Producto de prueba",
                Brand = null,
                ImageUrl = null
            });

        var result = await _productAppService.GetByBarcodeAsync(
            new GetProductByBarcodeInput
            {
                Barcode = "7790000000001"
            });

        result.Status.ShouldBe(ProductLookupStatus.Found);
        result.Barcode.ShouldBe("7790000000001");
        result.Name.ShouldBe("Producto de prueba");
        result.Brand.ShouldBeNull();
        result.ImageUrl.ShouldBeNull();
    }
    // TP07 - Open Food Facts alcanzó el límite de solicitudes
    [Fact]
    public async Task Should_Return_RateLimited_When_External_Catalog_Reaches_Limit()
    {
        _externalProductCatalogClient
            .GetByBarcodeAsync("3017620422003")
            .Returns<Task<ExternalProductDto?>>(
                _ => throw new ExternalCatalogRateLimitException());

        var result = await _productAppService.GetByBarcodeAsync(
            new GetProductByBarcodeInput
            {
                Barcode = "3017620422003"
            });

        result.Status.ShouldBe(ProductLookupStatus.RateLimited);
        result.Barcode.ShouldBe("3017620422003");
    }
    // TP07 - El catálogo externo no está disponible
    [Fact]
    public async Task Should_Return_ServiceUnavailable_When_External_Catalog_Is_Unavailable()
    {
        _externalProductCatalogClient
            .GetByBarcodeAsync("3017620422003")
            .Returns<Task<ExternalProductDto?>>(
                _ => throw new ExternalCatalogUnavailableException());

        var result = await _productAppService.GetByBarcodeAsync(
            new GetProductByBarcodeInput
            {
                Barcode = "3017620422003"
            });

        result.Status.ShouldBe(ProductLookupStatus.ServiceUnavailable);
        result.Barcode.ShouldBe("3017620422003");
    }
}