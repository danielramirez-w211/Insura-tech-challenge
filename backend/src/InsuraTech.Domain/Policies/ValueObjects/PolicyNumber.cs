using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.ValueObjects
{
    public sealed class PolicyNumber : ValueObject
    {
        public string Value {  get; }
        private PolicyNumber(string value) => Value = value;

        public static PolicyNumber Create(int year, long sequence)
        {
            if (year < 2000 || year > 9999)
                throw new ArgumentOutOfRangeException("Year is out of valid range", nameof(year));
            if (sequence <= 0)
                throw new ArgumentException("Sequence must be greater than zero.",nameof(sequence));

            var value = $"POL-{year}-{sequence:D8}";
            return new PolicyNumber(value);
        }
        public static PolicyNumber Parse(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("Policy number cannot be empty. ", nameof(value));

            var parts = value.Split('-');
            if(parts.Length != 3 || parts[0] != "POL" )
                throw new ArgumentException($"Invalid policy number format: '{value}'. ", value);
            return new PolicyNumber(value);
        }



        protected override IEnumerable<object> GetEqualityComponets()
        {
            yield return Value;
        }
        public override string ToString() => Value;
    }
}
