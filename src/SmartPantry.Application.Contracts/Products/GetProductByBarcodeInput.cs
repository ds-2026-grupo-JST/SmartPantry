using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Products;

public class GetProductByBarcodeInput
{
    [Required]
    [StringLength(
        14,
        MinimumLength = 8,
        ErrorMessage = "El código de barras debe contener entre 8 y 14 dígitos."
    )]
    [RegularExpression(
        @"^\d+$",
        ErrorMessage = "El código de barras debe contener solo dígitos."
    )]
    public string Barcode { get; set; } = default!;
}