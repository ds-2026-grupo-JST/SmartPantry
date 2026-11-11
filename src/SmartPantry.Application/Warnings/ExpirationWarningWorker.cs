using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SmartPantry.Warnings;

public class ExpirationWarningWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ExpirationWarningWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 60 * 60 * 1000; // cada 1 hora (en milisegundos)
        Timer.RunOnStart = true;       // y también al arrancar la aplicación
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        try
        {
            // Fecha de hoy en UTC, de forma explícita.
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Se piden los servicios en cada ejecución: el worker no guarda repositorios.
            var unitOfWorkManager = workerContext.ServiceProvider
                .GetRequiredService<IUnitOfWorkManager>();
            var domainService = workerContext.ServiceProvider
                .GetRequiredService<ExpirationWarningDomainService>();

            using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await domainService.ProcessAsync(today);
                await uow.CompleteAsync();
            }

            Logger.LogInformation("Advertencias de vencimiento procesadas para {Today}.", today);
        }
        catch (Exception ex)
        {
            // Se registra el error y se sigue: la próxima ejecución lo vuelve a intentar.
            Logger.LogError(ex, "Falló el procesamiento de advertencias de vencimiento.");
        }
    }
}