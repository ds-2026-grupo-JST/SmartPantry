using System.ComponentModel.DataAnnotations; //System.ComponentModel.DataAnnotations es una librería estándar de .NET. ABP (trae los atributos)
//                                            [Required] y [StringLength(...)] vienen de esa libreria
namespace SmartPantry.Products;

public class CreateProductDto 
{
    [Required]                                          //va arriba de la propiedad y significa "este campo es obligatorio"
    [StringLength(ProductConsts.MaxBarcodeLength)]
    public string Barcode { get; set; } = default!;     //es la propiedad. El = default! evita la advertencia de nulos.

    [Required]
    [StringLength(ProductConsts.MaxNameLength)]
    public string Name { get; set; } = default!;

    [StringLength(ProductConsts.MaxBrandLength)]
    public string? Brand { get; set; }

}