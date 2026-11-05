using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Pantries;

public class Pantry : AggregateRoot<Guid>
{
    private readonly List<PantryItem> _items = new();

    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public IReadOnlyCollection<PantryItem> Items => _items.AsReadOnly();

    protected Pantry() { }   // para que EF Core pueda reconstruirlo

    public Pantry(Guid id, Guid userId, string name) : base(id)
    {
        UserId = Check.NotDefaultOrNull<Guid>(userId, nameof(userId));
        Name = Check.NotNullOrWhiteSpace(name?.Trim(), nameof(name), PantryConsts.MaxNameLength);
    }

    public PantryItem AddItem(Guid itemId, Guid productId, DateOnly? expirationDate = null)
    {
        var item = new PantryItem(itemId, productId, expirationDate);
        _items.Add(item);
        return item;
    }
}