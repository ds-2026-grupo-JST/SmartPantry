using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Products;

public class Product : AggregateRoot<Guid>
{
    public Barcode Barcode { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Brand { get; private set; }

    protected Product() { }   // para que EF Core pueda reconstruirlo

    public Product(Guid id, Barcode barcode, string name, string? brand = null) : base(id)
    {
        Barcode = Check.NotNull(barcode, nameof(barcode));
        Name = Check.NotNullOrWhiteSpace(name?.Trim(), nameof(name), ProductConsts.MaxNameLength);
        Brand = string.IsNullOrWhiteSpace(brand)
            ? null
            : Check.Length(brand.Trim(), nameof(brand), ProductConsts.MaxBrandLength);
    }
}