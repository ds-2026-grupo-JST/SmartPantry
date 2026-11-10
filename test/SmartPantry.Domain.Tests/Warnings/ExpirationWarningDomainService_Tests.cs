using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartPantry.Pantries;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartPantry.Warnings;

public abstract class ExpirationWarningDomainService_Tests<TStartupModule>
    : SmartPantryDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    private readonly ExpirationWarningDomainService _service;
    private readonly IRepository<Pantry, Guid> _pantryRepository;
    private readonly IRepository<ExpirationWarning, Guid> _warningRepository;

    protected ExpirationWarningDomainService_Tests()
    {
        _service = GetRequiredService<ExpirationWarningDomainService>();
        _pantryRepository = GetRequiredService<IRepository<Pantry, Guid>>();
        _warningRepository = GetRequiredService<IRepository<ExpirationWarning, Guid>>();
    }

    // ---------- Ayudas ----------

    private async Task<(Guid PantryId, Guid ItemId)> CreatePantryWithItemAsync(
        DateOnly? expirationDate, bool consumed = false)
    {
        var pantryId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var pantry = new Pantry(pantryId, Guid.NewGuid(), "Despensa");
        pantry.AddItem(itemId, Guid.NewGuid(), expirationDate);
        if (consumed)
        {
            pantry.Items.Single().MarkAsConsumed();
        }

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryRepository.InsertAsync(pantry, autoSave: true);
        });

        return (pantryId, itemId);
    }

    private async Task UpdateItemAsync(Guid pantryId, Guid itemId, Action<PantryItem> change)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var pantry = await _pantryRepository.GetAsync(pantryId, includeDetails: true);
            change(pantry.Items.Single(i => i.Id == itemId));
            await _pantryRepository.UpdateAsync(pantry, autoSave: true);
        });
    }

    private async Task ProcessAsync(DateOnly today)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await _service.ProcessAsync(today);
        });
    }

    private async Task<List<ExpirationWarning>> GetWarningsAsync()
    {
        return await _warningRepository.GetListAsync();
    }

    // ---------- Pruebas ----------

    [Fact]
    public async Task Should_Create_ExpiringSoon_Warning_For_Item_Within_Threshold()
    {
        var (pantryId, itemId) = await CreatePantryWithItemAsync(Today.AddDays(2));

        await ProcessAsync(Today);

        var warning = Assert.Single(await GetWarningsAsync());
        Assert.Equal(pantryId, warning.PantryId);
        Assert.Equal(itemId, warning.PantryItemId);
        Assert.Equal(WarningType.ExpiringSoon, warning.Type);
        Assert.Equal(Today.AddDays(2), warning.ExpirationDate);
        Assert.True(warning.IsActive);
    }

    [Fact]
    public async Task Should_Create_Expired_Warning_For_Expired_Item()
    {
        await CreatePantryWithItemAsync(Today.AddDays(-1));

        await ProcessAsync(Today);

        var warning = Assert.Single(await GetWarningsAsync());
        Assert.Equal(WarningType.Expired, warning.Type);
        Assert.True(warning.IsActive);
    }

    [Fact]
    public async Task Should_Not_Create_Warnings_For_Items_That_Do_Not_Need_One()
    {
        await CreatePantryWithItemAsync(Today.AddDays(10));              // lejos del umbral
        await CreatePantryWithItemAsync(null);                            // sin fecha
        await CreatePantryWithItemAsync(Today.AddDays(-5), consumed: true); // consumido

        await ProcessAsync(Today);

        Assert.Empty(await GetWarningsAsync());
    }

    [Fact]
    public async Task Should_Keep_One_Warning_When_Run_Twice()
    {
        await CreatePantryWithItemAsync(Today.AddDays(2));

        await ProcessAsync(Today);
        await ProcessAsync(Today);

        Assert.Single(await GetWarningsAsync());
    }

    [Fact]
    public async Task Should_Update_Warning_When_Expiration_Date_Changes_Within_Threshold()
    {
        var (pantryId, itemId) = await CreatePantryWithItemAsync(Today.AddDays(2));
        await ProcessAsync(Today);

        await UpdateItemAsync(pantryId, itemId, i => i.ChangeExpirationDate(Today.AddDays(3)));
        await ProcessAsync(Today);

        var warning = Assert.Single(await GetWarningsAsync());
        Assert.Equal(Today.AddDays(3), warning.ExpirationDate);
        Assert.True(warning.IsActive);
    }

    [Fact]
    public async Task Should_Deactivate_Warning_When_Item_Leaves_The_Threshold()
    {
        var (pantryId, itemId) = await CreatePantryWithItemAsync(Today.AddDays(2));
        await ProcessAsync(Today);

        await UpdateItemAsync(pantryId, itemId, i => i.ChangeExpirationDate(Today.AddDays(30)));
        await ProcessAsync(Today);

        var warning = Assert.Single(await GetWarningsAsync());
        Assert.False(warning.IsActive);
    }

    [Fact]
    public async Task Should_Reactivate_The_Same_Warning_When_Item_Returns_To_Threshold()
    {
        var (pantryId, itemId) = await CreatePantryWithItemAsync(Today.AddDays(2));
        await ProcessAsync(Today);
        await UpdateItemAsync(pantryId, itemId, i => i.ChangeExpirationDate(Today.AddDays(30)));
        await ProcessAsync(Today);

        await UpdateItemAsync(pantryId, itemId, i => i.ChangeExpirationDate(Today.AddDays(1)));
        await ProcessAsync(Today);

        var warning = Assert.Single(await GetWarningsAsync());
        Assert.True(warning.IsActive);
        Assert.Equal(Today.AddDays(1), warning.ExpirationDate);
    }

    [Fact]
    public async Task Should_Replace_ExpiringSoon_With_Expired_As_Days_Pass()
    {
        await CreatePantryWithItemAsync(Today.AddDays(1));

        await ProcessAsync(Today);
        await ProcessAsync(Today.AddDays(2)); // dos días después, el ítem ya venció

        var warnings = await GetWarningsAsync();
        Assert.Equal(2, warnings.Count);
        Assert.False(warnings.Single(w => w.Type == WarningType.ExpiringSoon).IsActive);
        Assert.True(warnings.Single(w => w.Type == WarningType.Expired).IsActive);
    }

    [Fact]
    public async Task Should_Deactivate_Warning_When_Item_Is_Consumed()
    {
        var (pantryId, itemId) = await CreatePantryWithItemAsync(Today.AddDays(2));
        await ProcessAsync(Today);

        await UpdateItemAsync(pantryId, itemId, i => i.MarkAsConsumed());
        await ProcessAsync(Today);

        var warning = Assert.Single(await GetWarningsAsync());
        Assert.False(warning.IsActive);
    }

    [Fact]
    public async Task Database_Should_Reject_A_Duplicate_Warning_For_The_Same_Item_And_Type()
    {
        var pantryId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _warningRepository.InsertAsync(
                new ExpirationWarning(Guid.NewGuid(), pantryId, itemId, WarningType.Expired, Today),
                autoSave: true);
        });

        await Assert.ThrowsAnyAsync<Exception>(() => WithUnitOfWorkAsync(async () =>
        {
            await _warningRepository.InsertAsync(
                new ExpirationWarning(Guid.NewGuid(), pantryId, itemId, WarningType.Expired, Today),
                autoSave: true);
        }));
    }
}