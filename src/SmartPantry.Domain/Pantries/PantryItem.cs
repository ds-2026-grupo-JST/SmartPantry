using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Pantries;

public class PantryItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public ItemStatus Status { get; private set; }

    protected PantryItem() { }   // para que EF Core pueda reconstruirlo

    internal PantryItem(Guid id, Guid productId, DateOnly? expirationDate) : base(id)
    {
        ProductId = Check.NotDefaultOrNull<Guid>(productId, nameof(productId));
        ExpirationDate = expirationDate;
        Status = ItemStatus.Available;
    }

    public void ChangeExpirationDate(DateOnly? expirationDate)
    {
        ExpirationDate = expirationDate;
    }

    public void MarkAsConsumed()
    {
        Status = ItemStatus.Consumed;
    }
}