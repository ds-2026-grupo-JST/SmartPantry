using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Products;

public class ProductDto : EntityDto<Guid>
{
    public string Barcode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
}