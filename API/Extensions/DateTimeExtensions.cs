using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateOnly dob)
        {
            var Today = DateOnly.FromDateTime(DateTime.UtcNow);

            var age = Today.Year - dob.Year;

            if(dob > Today.AddYears(-age)) age--;

            return age;
        }
    }
}