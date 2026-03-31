using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(Guid recientId, string subject, string body,
            CancellationToken cancellationToken = default);
    }
}
