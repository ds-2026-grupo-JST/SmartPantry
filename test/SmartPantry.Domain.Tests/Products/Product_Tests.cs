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
}