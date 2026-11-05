using System;

namespace SmartPantry.Products;

public class ExternalCatalogRateLimitException : Exception
{
    public ExternalCatalogRateLimitException()
        : base("El catálogo externo alcanzó el límite de solicitudes.")
    {
    }
}

public class ExternalCatalogUnavailableException : Exception
{
    public ExternalCatalogUnavailableException()
        : base("El catálogo externo no está disponible.")
    {
    }
}