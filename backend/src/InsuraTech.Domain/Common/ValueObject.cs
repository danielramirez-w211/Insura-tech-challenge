using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Common
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponets();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType()) return false;
            return GetEqualityComponets()
                    .SequenceEqual(((ValueObject)obj).GetEqualityComponets());
            
        }
        public override int GetHashCode() =>
            GetEqualityComponets()
            .Aggregate(1, (current, obj) =>
                HashCode.Combine(current, obj?.GetHashCode() ?? 0));

        public static bool operator ==(ValueObject? left, ValueObject? right) =>
                left?.Equals(right) ?? right is null;

        public static bool operator !=(ValueObject? left, ValueObject? right) =>
            !(left == right);
    }
}
