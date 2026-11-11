using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Warnings;

public class ExpirationWarning : AggregateRoot<Guid>
{
    public Guid PantryId { get; private set; }
    public Guid PantryItemId { get; private set; }
    public WarningType Type { get; private set; }
    public DateOnly ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }

    protected ExpirationWarning() { }   // para que EF Core pueda reconstruirlo

    public ExpirationWarning(
        Guid id,
        Guid pantryId,
        Guid pantryItemId,
        WarningType type,
        DateOnly expirationDate) : base(id)
    {
        PantryId = Check.NotDefaultOrNull<Guid>(pantryId, nameof(pantryId));
        PantryItemId = Check.NotDefaultOrNull<Guid>(pantryItemId, nameof(pantryItemId));

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        Type = type;
        ExpirationDate = expirationDate;
        IsActive = true;
    }

    public void Activate(DateOnly expirationDate)
    {
        ExpirationDate = expirationDate;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}