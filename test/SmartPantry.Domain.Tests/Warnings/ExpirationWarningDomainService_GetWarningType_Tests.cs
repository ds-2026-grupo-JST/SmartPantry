using System;
using System.Linq;
using SmartPantry.Pantries;
using Xunit;

namespace SmartPantry.Warnings;

public class ExpirationWarningDomainService_GetWarningType_Tests
{
    private static readonly DateOnly Today = new(2026, 11, 10);
    private const int Threshold = 3;

    private static PantryItem CreateItem(DateOnly? expirationDate)
    {
        var pantry = new Pantry(Guid.NewGuid(), Guid.NewGuid(), "Despensa");
        pantry.AddItem(Guid.NewGuid(), Guid.NewGuid(), expirationDate);
        return pantry.Items.Single();
    }

    [Theory]
    [InlineData(-30, WarningType.Expired)]
    [InlineData(-1, WarningType.Expired)]
    [InlineData(0, WarningType.ExpiringSoon)]
    [InlineData(1, WarningType.ExpiringSoon)]
    [InlineData(3, WarningType.ExpiringSoon)]
    [InlineData(4, null)]
    [InlineData(30, null)]
    public void Should_Classify_Item_By_Days_To_Expiration(
        int daysFromToday, WarningType? expected)
    {
        var item = CreateItem(Today.AddDays(daysFromToday));

        var result = ExpirationWarningDomainService.GetWarningType(item, Today, Threshold);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Should_Not_Warn_When_Item_Has_No_Expiration_Date()
    {
        var item = CreateItem(null);

        var result = ExpirationWarningDomainService.GetWarningType(item, Today, Threshold);

        Assert.Null(result);
    }

    [Fact]
    public void Should_Not_Warn_When_Item_Is_Consumed_Even_If_Expired()
    {
        var item = CreateItem(Today.AddDays(-5));
        item.MarkAsConsumed();

        var result = ExpirationWarningDomainService.GetWarningType(item, Today, Threshold);

        Assert.Null(result);
    }

    [Fact]
    public void Should_Respect_The_Given_Threshold()
    {
        var onLimit = CreateItem(Today.AddDays(7));
        var beyondLimit = CreateItem(Today.AddDays(8));

        Assert.Equal(WarningType.ExpiringSoon,
            ExpirationWarningDomainService.GetWarningType(onLimit, Today, 7));
        Assert.Null(
            ExpirationWarningDomainService.GetWarningType(beyondLimit, Today, 7));
    }
}