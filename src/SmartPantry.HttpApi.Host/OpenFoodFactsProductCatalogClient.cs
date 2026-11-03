using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartPantry.Products;

namespace SmartPantry;

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenFoodFactsProductCatalogClient> _logger;

    public OpenFoodFactsProductCatalogClient(
        HttpClient httpClient,
        ILogger<OpenFoodFactsProductCatalogClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        const string fields =
            "code,product_name,product_name_es,brands,image_front_url";

        var path =
            $"product/{Uri.EscapeDataString(barcode)}?fields={Uri.EscapeDataString(fields)}";

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.GetAsync(path);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "Consulta externa finalizada. Tipo={QueryType}, Resultado={Result}, DuracionMs={DurationMs}",
                    "Barcode",
                    "NotFound",
                    stopwatch.ElapsedMilliseconds);

                return null;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "Consulta externa finalizada. Tipo={QueryType}, Resultado={Result}, DuracionMs={DurationMs}",
                    "Barcode",
                    "RateLimited",
                    stopwatch.ElapsedMilliseconds);

                throw new ExternalCatalogRateLimitException();
            }

            if (!response.IsSuccessStatusCode)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "Consulta externa finalizada. Tipo={QueryType}, Resultado={Result}, DuracionMs={DurationMs}",
                    "Barcode",
                    "ServiceUnavailable",
                    stopwatch.ElapsedMilliseconds);

                throw new ExternalCatalogUnavailableException();
            }

            using var document =
                JsonDocument.Parse(await response.Content.ReadAsStreamAsync());

            if (!document.RootElement.TryGetProperty("product", out var product))
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "Consulta externa finalizada. Tipo={QueryType}, Resultado={Result}, DuracionMs={DurationMs}",
                    "Barcode",
                    "InvalidResponse",
                    stopwatch.ElapsedMilliseconds);

                throw new ExternalCatalogUnavailableException();
            }

            var result = new ExternalProductDto
            {
                Barcode = GetString(product, "code") ?? barcode,

                Name =
                    GetString(product, "product_name_es")
                    ?? GetString(product, "product_name"),

                Brand = GetString(product, "brands"),

                ImageUrl = GetString(product, "image_front_url")
            };

            stopwatch.Stop();

            _logger.LogInformation(
                "Consulta externa finalizada. Tipo={QueryType}, Resultado={Result}, DuracionMs={DurationMs}",
                "Barcode",
                "Found",
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (ExternalCatalogRateLimitException)
        {
            throw;
        }
        catch (ExternalCatalogUnavailableException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                "Consulta externa finalizada. Tipo={QueryType}, Resultado={Result}, DuracionMs={DurationMs}",
                "Barcode",
                "Timeout",
                stopwatch.ElapsedMilliseconds);

            throw new ExternalCatalogUnavailableException();
        }
        catch (HttpRequestException)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                "Consulta externa finalizada. Tipo={QueryType}, Resultado={Result}, DuracionMs={DurationMs}",
                "Barcode",
                "NetworkError",
                stopwatch.ElapsedMilliseconds);

            throw new ExternalCatalogUnavailableException();
        }
        catch (JsonException)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                "Consulta externa finalizada. Tipo={QueryType}, Resultado={Result}, DuracionMs={DurationMs}",
                "Barcode",
                "InvalidJson",
                stopwatch.ElapsedMilliseconds);

            throw new ExternalCatalogUnavailableException();
        }
    }

    private static string? GetString(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }
}