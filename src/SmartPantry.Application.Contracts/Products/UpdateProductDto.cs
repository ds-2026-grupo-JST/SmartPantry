using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Products;

public class UpdateProductDto
{
    [Required]
    [StringLength(ProductConsts.MaxNameLength)]
    public string Name { get; set; } = default!;

    [StringLength(ProductConsts.MaxBrandLength)]
    public string? Brand { get; set; }
}