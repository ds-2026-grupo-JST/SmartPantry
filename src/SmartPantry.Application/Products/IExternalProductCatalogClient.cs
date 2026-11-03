using System.Threading.Tasks;

namespace SmartPantry.Products;

public interface IExternalProductCatalogClient
{
    Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);
}