using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Policies
{
    public enum PolicyStatus
    {
        Pending   = 1,
        Active    = 2,
        Suspended = 3,
        Cancelled = 4,
        Exhausted = 5,
        Expired   = 6
    }
}
