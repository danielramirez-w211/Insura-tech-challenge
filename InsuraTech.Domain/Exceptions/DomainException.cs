using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Code { get; }
        protected DomainException(string code, string message) : base(message) 
        {
            Code = code;
        }
    }
}
