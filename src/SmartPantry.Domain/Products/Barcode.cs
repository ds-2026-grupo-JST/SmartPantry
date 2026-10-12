using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace SmartPantry.Products;

public class Barcode : ValueObject
{
    public string Code { get; private set; } = default!;

    protected Barcode() { }   // para que EF Core pueda reconstruirlo

    public Barcode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code?.Trim(), nameof(code), ProductConsts.MaxBarcodeLength);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
}