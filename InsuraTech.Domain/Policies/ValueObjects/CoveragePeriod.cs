using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.ValueObjects
{
    public sealed class CoveragePeriod : ValueObject
    {
        public DateOnly StartDate { get; }
        public DateOnly EndDate { get; }

        private static readonly int MinDays = 30;
        private static readonly int MaxYears = 5;

        public CoveragePeriod(DateOnly startDate, DateOnly endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public static CoveragePeriod Create(DateOnly startDate, DateOnly endDate)
        {
            if (startDate >= endDate)
                throw new ArgumentException("Start Date must be before end date. ");

            var days = endDate.DayNumber - startDate.DayNumber;
            if (days < MinDays)
                throw new ArgumentException($"Coverage period must be at least {MinDays} days. ");

            if (startDate.AddYears(MaxYears) < endDate)
                throw new ArgumentException($"Coverage peirod cannot exceed {MaxYears} years. ");

            return new CoveragePeriod(startDate, endDate);
        }

        public bool IsActive(DateOnly date) =>
            date >= StartDate && date <= EndDate;
        public bool IsExpired(DateOnly date) =>
            date > EndDate;
        public int DaysUntilExpiration(DateOnly fromDate) =>
            EndDate.DayNumber - fromDate.DayNumber;

        protected override IEnumerable<object> GetEqualityComponets()
        {
            yield return StartDate;
            yield return EndDate;
        }
        public override string ToString() => $"{StartDate:yyyy-MM-dd} → {EndDate:yyyy-MM-dd}";
    }
}