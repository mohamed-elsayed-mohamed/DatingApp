using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
      /// <summary>
      /// Converts <see cref="DateOnly" /> to <see cref="DateTime"/> and vice versa.
      /// </summary>
      public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
      {
          /// <summary>
          /// Creates a new instance of this converter.
          /// </summary>
          public DateOnlyConverter() : base(
                  d => d.ToDateTime(TimeOnly.MinValue),
                  d => DateOnly.FromDateTime(d))
          { }
      }
}