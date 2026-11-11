using System;
using Xunit;

namespace SmartPantry.Warnings;

public class ExpirationWarning_Tests
{
    private static ExpirationWarning CreateWarning(
        WarningType type = WarningType.ExpiringSoon,
        DateOnly? expirationDate = null)
    {
        return new ExpirationWarning(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            type,
            expirationDate ?? new DateOnly(2026, 11, 12));
    }

    [Fact]
    public void Should_Create_Active_Warning_With_Given_Data()
    {
        var id = Guid.NewGuid();
        var pantryId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var date = new DateOnly(2026, 11, 12);

        var warning = new ExpirationWarning(id, pantryId, itemId, WarningType.Expired, date);

        Assert.Equal(id, warning.Id);
        Assert.Equal(pantryId, warning.PantryId);
        Assert.Equal(itemId, warning.PantryItemId);
        Assert.Equal(WarningType.Expired, warning.Type);
        Assert.Equal(date, warning.ExpirationDate);
        Assert.True(warning.IsActive);
    }

    [Fact]
    public void Should_Reject_Empty_Pantry_Id()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new ExpirationWarning(
                Guid.NewGuid(), Guid.Empty, Guid.NewGuid(),
                WarningType.Expired, new DateOnly(2026, 11, 12)));
    }

    [Fact]
    public void Should_Reject_Empty_Item_Id()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new ExpirationWarning(
                Guid.NewGuid(), Guid.NewGuid(), Guid.Empty,
                WarningType.Expired, new DateOnly(2026, 11, 12)));
    }

    [Fact]
    public void Should_Reject_Undefined_Warning_Type()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new ExpirationWarning(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                (WarningType)99, new DateOnly(2026, 11, 12)));
    }

    [Fact]
    public void Deactivate_Should_Turn_Off_The_Warning_Without_Losing_Data()
    {
        var warning = CreateWarning(expirationDate: new DateOnly(2026, 11, 12));

        warning.Deactivate();

        Assert.False(warning.IsActive);
        Assert.Equal(new DateOnly(2026, 11, 12), warning.ExpirationDate);
    }

    [Fact]
    public void Activate_Should_Reactivate_And_Update_Expiration_Date()
    {
        var warning = CreateWarning();
        warning.Deactivate();

        warning.Activate(new DateOnly(2026, 11, 20));

        Assert.True(warning.IsActive);
        Assert.Equal(new DateOnly(2026, 11, 20), warning.ExpirationDate);
    }
}