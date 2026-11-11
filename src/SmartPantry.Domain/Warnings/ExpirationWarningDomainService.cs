using System;
using System.Linq;
using System.Threading.Tasks;
using SmartPantry.Pantries;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SmartPantry.Warnings;

public class ExpirationWarningDomainService : DomainService
{
    private readonly IRepository<Pantry, Guid> _pantryRepository;
    private readonly IRepository<ExpirationWarning, Guid> _warningRepository;

    public ExpirationWarningDomainService(
        IRepository<Pantry, Guid> pantryRepository,
        IRepository<ExpirationWarning, Guid> warningRepository)
    {
        _pantryRepository = pantryRepository;
        _warningRepository = warningRepository;
    }

    // Qué advertencia le corresponde a un ítem en la fecha "today" (null = ninguna)
    public static WarningType? GetWarningType(PantryItem item, DateOnly today, int thresholdDays)
    {
        if (item.Status != ItemStatus.Available || item.ExpirationDate is null)
        {
            return null;
        }

        var expirationDate = item.ExpirationDate.Value;

        if (expirationDate < today)
        {
            return WarningType.Expired;
        }

        if (expirationDate <= today.AddDays(thresholdDays))
        {
            return WarningType.ExpiringSoon;
        }

        return null;
    }

    public async Task ProcessAsync(
        DateOnly today,
        int thresholdDays = ExpirationWarningConsts.DefaultThresholdDays)
    {
        var pantries = await _pantryRepository.GetListAsync(includeDetails: true);
        var warnings = await _warningRepository.GetListAsync();
        var existing = warnings.ToDictionary(w => (w.PantryItemId, w.Type));

        foreach (var pantry in pantries)
        {
            foreach (var item in pantry.Items)
            {
                var expected = GetWarningType(item, today, thresholdDays);

                foreach (var type in Enum.GetValues<WarningType>())
                {
                    existing.TryGetValue((item.Id, type), out var warning);

                    if (type == expected)
                    {
                        var expirationDate = item.ExpirationDate!.Value;

                        if (warning is null)
                        {
                            await _warningRepository.InsertAsync(new ExpirationWarning(
                                GuidGenerator.Create(), pantry.Id, item.Id, type, expirationDate));
                        }
                        else if (!warning.IsActive || warning.ExpirationDate != expirationDate)
                        {
                            warning.Activate(expirationDate);
                            await _warningRepository.UpdateAsync(warning);
                        }
                    }
                    else if (warning is not null && warning.IsActive)
                    {
                        warning.Deactivate();
                        await _warningRepository.UpdateAsync(warning);
                    }
                }
            }
        }
    }
}