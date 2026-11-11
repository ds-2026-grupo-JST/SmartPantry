using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SmartPantry.EntityFrameworkCore;

/* El conector de MySQL devuelve las columnas "date" como DateTime.
 * Este conversor traduce entre DateOnly (dominio) y DateTime (base de datos). */
public class DateOnlyDateTimeConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyDateTimeConverter()
        : base(
            date => date.ToDateTime(TimeOnly.MinValue),
            dateTime => DateOnly.FromDateTime(dateTime))
    {
    }
}