using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Claims
{
    public enum ClaimStatus
    {
        Registered = 1,
        UnderInvestigation = 2,
        Approved = 3,
        Rejected = 4,
        Appealed = 5,
        Paid = 6
    }
}
