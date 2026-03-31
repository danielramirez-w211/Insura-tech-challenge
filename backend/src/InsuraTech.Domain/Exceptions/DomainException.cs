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
    public sealed class BusinessRuleException : DomainException
    {
        public BusinessRuleException(string code, string message) :
            base(code, message)
        { }
    }
}
