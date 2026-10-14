using System;
using Xunit;

namespace SmartPantry.Products;

public class Product_Tests
{
    [Fact]
    public void Should_Create_Valid_Product_And_Normalize_Text()
    {
        var id = Guid.NewGuid();

        var product = new Product(
            id,
            new Barcode("  7790000000001  "),
            "  Leche entera  ",
            "  La Serenísima  ");

        Assert.Equal(id, product.Id);
        Assert.Equal("7790000000001", product.Barcode.Code);
        Assert.Equal("Leche entera", product.Name);
        Assert.Equal("La Serenísima", product.Brand);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Reject_Blank_Name(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new Product(Guid.NewGuid(), new Barcode("7790000000001"), name!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Reject_Blank_Barcode(string? code)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Barcode(code!));
    }

    [Fact]
    public void Update_Should_Change_And_Normalize_Name_And_Brand()
    {
        var product = new Product(Guid.NewGuid(), new Barcode("7790001"), "Leche", "Marca A");

        product.Update("  Yogur  ", "  Marca B  ");

        Assert.Equal("Yogur", product.Name);
        Assert.Equal("Marca B", product.Brand);
        Assert.Equal("7790001", product.Barcode.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_Should_Reject_Invalid_Name_And_Keep_State(string invalidName)
    {
        var product = new Product(Guid.NewGuid(), new Barcode("7790001"), "Leche", "Marca A");

        Assert.ThrowsAny<ArgumentException>(() => product.Update(invalidName, "Marca B"));

        Assert.Equal("Leche", product.Name);
        Assert.Equal("Marca A", product.Brand);
    }

    [Fact]
    public void Update_Should_Not_Leave_Partial_Changes_When_Brand_Is_Invalid()
    {
        var product = new Product(Guid.NewGuid(), new Barcode("7790001"), "Leche", "Marca A");
        var tooLongBrand = new string('x', ProductConsts.MaxBrandLength + 1);

        Assert.ThrowsAny<ArgumentException>(() => product.Update("Yogur", tooLongBrand));

        Assert.Equal("Leche", product.Name);
        Assert.Equal("Marca A", product.Brand);
    }
}