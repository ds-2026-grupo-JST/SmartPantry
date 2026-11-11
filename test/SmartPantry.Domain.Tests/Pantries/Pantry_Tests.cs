using System;
using System.Linq;
using Xunit;

namespace SmartPantry.Pantries;

public class Pantry_Tests
{
    [Fact]
    public void Should_Create_Valid_Pantry_Without_Items()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var pantry = new Pantry(id, userId, "Despensa de casa");

        Assert.Equal(id, pantry.Id);
        Assert.Equal(userId, pantry.UserId);
        Assert.Equal("Despensa de casa", pantry.Name);
        Assert.Empty(pantry.Items);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Reject_Blank_Name(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new Pantry(Guid.NewGuid(), Guid.NewGuid(), name!));
    }

    [Fact]
    public void Should_Reject_Empty_Owner()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new Pantry(Guid.NewGuid(), Guid.Empty, "Despensa"));
    }

    [Fact]
    public void AddItem_Should_Add_Available_Item_With_Expiration_Date()
    {
        var pantry = new Pantry(Guid.NewGuid(), Guid.NewGuid(), "Despensa");
        var itemId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var expiration = new DateOnly(2026, 11, 10);

        pantry.AddItem(itemId, productId, expiration);

        var item = Assert.Single(pantry.Items);
        Assert.Equal(itemId, item.Id);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(expiration, item.ExpirationDate);
        Assert.Equal(ItemStatus.Available, item.Status);
    }

    [Fact]
    public void AddItem_Without_Date_Should_Keep_It_Null()
    {
        var pantry = new Pantry(Guid.NewGuid(), Guid.NewGuid(), "Despensa");

        pantry.AddItem(Guid.NewGuid(), Guid.NewGuid());

        var item = Assert.Single(pantry.Items);
        Assert.Null(item.ExpirationDate);
    }

    [Fact]
    public void AddItem_Should_Reject_Empty_Product()
    {
        var pantry = new Pantry(Guid.NewGuid(), Guid.NewGuid(), "Despensa");

        Assert.ThrowsAny<ArgumentException>(() =>
            pantry.AddItem(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void Item_Should_Change_Expiration_Date()
    {
        var pantry = new Pantry(Guid.NewGuid(), Guid.NewGuid(), "Despensa");
        pantry.AddItem(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 11, 10));
        var item = pantry.Items.Single();

        item.ChangeExpirationDate(new DateOnly(2026, 12, 1));

        Assert.Equal(new DateOnly(2026, 12, 1), item.ExpirationDate);
    }

    [Fact]
    public void Item_Should_Be_Marked_As_Consumed()
    {
        var pantry = new Pantry(Guid.NewGuid(), Guid.NewGuid(), "Despensa");
        pantry.AddItem(Guid.NewGuid(), Guid.NewGuid());
        var item = pantry.Items.Single();

        item.MarkAsConsumed();

        Assert.Equal(ItemStatus.Consumed, item.Status);
    }
}